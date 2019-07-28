using UnityEngine;
using System.Collections;

public class FluidFiller : MonoBehaviour
{
    public GameObject Syringe;
    public Material SyringePartsMat;
    private GameObject Presser;
    private GameObject Applicator;
    private GameObject Liquid;

    private GameObject rightController;

    public GameObject MeshPainter;

    public GameObject FluidToFill;
    private Material FluidMat;
    private Material FluidMatTemp;
    private Material LiquidMat;
    private MeshPainterController MeshPainterController;
    private Material lastHighlightedMat;

    private float scaleMaxFill;
    private float maxFill;
    private float minFill;

    // Use this for initialization
    void Start()
    {
        rightController = GameObject.Find("RightControllerAnchor");

        FluidMat = FluidToFill.GetComponent<Renderer>().material;
        FluidMatTemp = new Material(FluidMat);
        lastHighlightedMat = FluidMat;

        InitSyringeComponents();

        maxFill = 0.5f;
        minFill = -1f;

        float scale = 1 / FluidMat.GetFloat("_Size");
        scaleMaxFill = scale * (maxFill + 1f) - 1f;

        MeshPainterController = MeshPainter.GetComponent<MeshPainterController>();
    }

    void InitSyringeComponents()
    {
        Liquid = Syringe.transform.Find("Liquid").gameObject;
        LiquidMat = Liquid.GetComponent<Renderer>().material;


        Presser = Syringe.transform.Find("Presser").gameObject;
        Applicator = Syringe.transform.Find("Applicator").gameObject;

        Renderer[] renderers = Applicator.GetComponentsInChildren<Renderer>();

        for (int i = 0; i < renderers.Length; i++)
        {
            renderers[i].material = SyringePartsMat;
        }

        ResetMaterialParams(LiquidMat);
        ResetMaterialParams(SyringePartsMat);
    }

    void ResetMaterialParams(Material mat) 
    {
        mat.SetFloat("_FillAmount", -1f);
        mat.SetFloat("_GlitterPercent", 0);
        mat.SetFloat("_PoisonPercent", 0);
        mat.SetFloat("_ColorPercent", 0);
        mat.SetFloat("_RainbowPercent", 0);
    }

    void HandlePlatformUpdate(Ray ray, bool fillEvent, bool emptyEvent)
    {

        int layerMask = 1 << 8 | 1 << 11;
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, Mathf.Infinity, layerMask))
        {
            FluidMat = hit.collider.gameObject.GetComponent<Renderer>().material;

            if (fillEvent && hit.collider.gameObject.layer != 11)
            {
                FillTube(FluidMat);
            }
            else if (emptyEvent)
            {
                EmptyTube(FluidMat);
            }
            else
            {
                HighlightTube(FluidMat);
            }
        }
        else if (emptyEvent && MeshPainterController.lastHighlightedIndex.x > 0)
        {
            SetMaterialFromTriangle();
            EmptyTube(FluidMatTemp);
        }
        else if (fillEvent)
        {
            EmptySyringe();
        }
    }

    // Update is called once per frame
    void Update()
    {
        RemoveHighlights();

        bool isConnected = OVRInput.IsControllerConnected(OVRInput.Controller.RTouch);

        // ************** Keyboard Controls ************** //
        if (!isConnected)
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            HandlePlatformUpdate(ray, Input.GetKey(KeyCode.F), Input.GetKey(KeyCode.G));
        }
        // ************** VR Controls ************** //
        else {
            Ray ray = new Ray(rightController.transform.position, rightController.transform.forward);
            HandlePlatformUpdate(ray, OVRInput.Get(OVRInput.Button.SecondaryThumbstickUp), OVRInput.Get(OVRInput.Button.SecondaryThumbstickDown));
        }
    }

    void HighlightTube(Material mat)
    {
        mat.SetFloat("_FresnelPower", 2);
        lastHighlightedMat = mat;
    }

    void RemoveHighlights()
    {
        lastHighlightedMat.SetFloat("_FresnelPower", 1);
    }
    void SetPresserPos(float fillAmount)
    {
        Vector3 pos = Presser.transform.localPosition;
        pos.y = 2f * (fillAmount + 1f) / 1.5f;
        Presser.transform.localPosition = pos;
    }


    void EmptySyringe()
    {
        float sFillAmount = LiquidMat.GetFloat("_FillAmount");
        if (sFillAmount > minFill)
        {
            sFillAmount -= Time.deltaTime;
            LiquidMat.SetFloat("_FillAmount", sFillAmount);

            CopyWeightsToSyringeParts();
            SetPresserPos(sFillAmount);
        }
    }

    void SetMaterialFromTriangle()
    {
        Mesh mesh = MeshPainterController.lastHighlightedMesh;
        int tIdx = MeshPainterController.lastHighlightedIndex.x;

        float _ColorPercent = mesh.tangents[tIdx].x;
        float _GlitterPercent = mesh.tangents[tIdx].y;
        float _PoisonPercent = mesh.tangents[tIdx].z;
        float _RainbowPercent = mesh.tangents[tIdx].w;

        Color color = new Color(mesh.uv2[tIdx].x, mesh.uv2[tIdx].y, mesh.uv3[tIdx].x);

        FluidMatTemp.SetColor("_Color", color.gamma);
        FluidMatTemp.SetFloat("_GlitterPercent", _GlitterPercent);
        FluidMatTemp.SetFloat("_ColorPercent", _ColorPercent);
        FluidMatTemp.SetFloat("_PoisonPercent", _PoisonPercent);
        FluidMatTemp.SetFloat("_RainbowPercent", _RainbowPercent);
        FluidMatTemp.SetFloat("_FillAmount", maxFill);
    }

    void RecalculateWeights(Material fromMat, Material toMat, float blend)
    {
        float sColorPct = fromMat.GetFloat("_ColorPercent");
        float colorPct = toMat.GetFloat("_ColorPercent");

        float sGlitterPct = fromMat.GetFloat("_GlitterPercent");
        float glitterPct = toMat.GetFloat("_GlitterPercent");

        float sPoisonPct = fromMat.GetFloat("_PoisonPercent");
        float poisonPct = toMat.GetFloat("_PoisonPercent");

        float sRainbowPct = fromMat.GetFloat("_RainbowPercent");
        float rainbowPct = toMat.GetFloat("_RainbowPercent");


        float glitterPctNext = (glitterPct * blend + (sGlitterPct * Time.deltaTime)) / (blend + Time.deltaTime);
        float colorPctNext = (colorPct * blend + (sColorPct * Time.deltaTime)) / (blend + Time.deltaTime);
        float poisonPctNext = (poisonPct * blend + (sPoisonPct * Time.deltaTime)) / (blend + Time.deltaTime);
        float rainbowPctNext = (rainbowPct * blend + (sRainbowPct * Time.deltaTime)) / (blend + Time.deltaTime);
        toMat.SetFloat("_GlitterPercent", glitterPctNext);
        toMat.SetFloat("_ColorPercent", colorPctNext);
        toMat.SetFloat("_PoisonPercent", poisonPctNext);
        toMat.SetFloat("_RainbowPercent", rainbowPctNext);
    }

    void CopyWeightsToSyringeParts()
    {

        SyringePartsMat.SetColor("_Color", LiquidMat.GetColor("_Color"));

        SyringePartsMat.SetFloat("_FillAmount", LiquidMat.GetFloat("_FillAmount"));
        SyringePartsMat.SetFloat("_GlitterPercent", LiquidMat.GetFloat("_GlitterPercent"));
        SyringePartsMat.SetFloat("_ColorPercent", LiquidMat.GetFloat("_ColorPercent"));
        SyringePartsMat.SetFloat("_PoisonPercent", LiquidMat.GetFloat("_PoisonPercent"));
        SyringePartsMat.SetFloat("_RainbowPercent", LiquidMat.GetFloat("_RainbowPercent"));
    }

    void FillTube(Material tubeMat)
    {
        float sFillAmount = LiquidMat.GetFloat("_FillAmount");
        float fillAmount = tubeMat.GetFloat("_FillAmount");

        Color sColor = LiquidMat.GetColor("_Color");
        Color color = tubeMat.GetColor("_Color");

        if (sFillAmount > minFill && fillAmount < scaleMaxFill)
        {
            float blend = (fillAmount - minFill) / (Mathf.Abs(minFill) + maxFill);
            float blendP = blend / (blend + Time.deltaTime);

            sFillAmount -= Time.deltaTime;
            fillAmount += Time.deltaTime;
            tubeMat.SetFloat("_FillAmount", fillAmount);
            LiquidMat.SetFloat("_FillAmount", sFillAmount);

            if(sColor.a > 0)
            {
                color = (1f - blendP) * sColor + blendP * color;
            }
            tubeMat.SetColor("_Color", color);

            RecalculateWeights(LiquidMat, tubeMat, blend);

            CopyWeightsToSyringeParts();
            SetPresserPos(sFillAmount);
        }
    }

    void EmptyTube(Material tubeMat)
    {
        float sFillAmount = LiquidMat.GetFloat("_FillAmount");
        float fillAmount = tubeMat.GetFloat("_FillAmount");

        Color sColor = LiquidMat.GetColor("_Color");
        Color color = tubeMat.GetColor("_Color");

        if (fillAmount > minFill && sFillAmount < maxFill)
        {
            float blend = (sFillAmount + 1f) / 1.5f;
            float blendP = blend / (blend + Time.deltaTime);

            sFillAmount += Time.deltaTime;
            fillAmount -= Time.deltaTime;
            tubeMat.SetFloat("_FillAmount", fillAmount);
            LiquidMat.SetFloat("_FillAmount", sFillAmount);

            if (color.a > 0)
            {
                sColor = (1f - blendP) * color + blendP * sColor;
            }

            LiquidMat.SetColor("_Color", sColor);

            RecalculateWeights(tubeMat, LiquidMat, blend);

            CopyWeightsToSyringeParts();
            SetPresserPos(sFillAmount);

        }


    }
}
