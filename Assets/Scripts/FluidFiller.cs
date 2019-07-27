using UnityEngine;
using System.Collections;

public class FluidFiller : MonoBehaviour
{
    public GameObject Syringe;
    public GameObject Presser;
    public GameObject MeshPainter;

    public GameObject FluidToFill;
    private Material FluidMat;
    private Material FluidMatTemp;
    private Material SyringeMat;
    private MeshPainterController MeshPainterController;
    private Material lastHighlightedMat;

    private float scaleMaxFill;
    private float maxFill;
    private float minFill;

    // Use this for initialization
    void Start()
    {
        FluidMat = FluidToFill.GetComponent<Renderer>().material;
        FluidMatTemp = new Material(FluidMat);
        lastHighlightedMat = FluidMat;
        SyringeMat = Syringe.GetComponent<Renderer>().material;
        SyringeMat.SetFloat("_FillAmount", -1f);
        SyringeMat.SetFloat("_GlitterPercent", 0);
        SyringeMat.SetFloat("_PoisonPercent", 0);
        SyringeMat.SetFloat("_ColorPercent", 0);
        SyringeMat.SetFloat("_RainbowPercent", 0);
        maxFill = 0.5f;
        minFill = -1f;

        float scale = 1 / FluidMat.GetFloat("_Size");
        scaleMaxFill = scale * (maxFill + 1f) - 1f;

        MeshPainterController = MeshPainter.GetComponent<MeshPainterController>();
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
            Ray ray = new Ray();
            ray.origin = Syringe.transform.position;
            ray.direction = -Syringe.transform.up;
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
        float sFillAmount = SyringeMat.GetFloat("_FillAmount");
        if (sFillAmount > minFill)
        {
            sFillAmount -= Time.deltaTime;
            SyringeMat.SetFloat("_FillAmount", sFillAmount);

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



    void FillTube(Material tubeMat)
    {
        float sFillAmount = SyringeMat.GetFloat("_FillAmount");
        float fillAmount = tubeMat.GetFloat("_FillAmount");

        Color sColor = SyringeMat.GetColor("_Color");
        Color color = tubeMat.GetColor("_Color");

        if (sFillAmount > minFill && fillAmount < scaleMaxFill)
        {
            float blend = (fillAmount - minFill) / (Mathf.Abs(minFill) + maxFill);
            float blendP = blend / (blend + Time.deltaTime);

            sFillAmount -= Time.deltaTime;
            fillAmount += Time.deltaTime;
            tubeMat.SetFloat("_FillAmount", fillAmount);
            SyringeMat.SetFloat("_FillAmount", sFillAmount);

            if(sColor.a > 0)
            {
                color = (1f - blendP) * sColor + blendP * color;
            }
            tubeMat.SetColor("_Color", color);

            RecalculateWeights(SyringeMat, tubeMat, blend);
            SetPresserPos(sFillAmount);
        }
    }

    void EmptyTube(Material tubeMat)
    {
        float sFillAmount = SyringeMat.GetFloat("_FillAmount");
        float fillAmount = tubeMat.GetFloat("_FillAmount");

        Color sColor = SyringeMat.GetColor("_Color");
        Color color = tubeMat.GetColor("_Color");

        if (fillAmount > minFill && sFillAmount < maxFill)
        {
            float blend = (sFillAmount + 1f) / 1.5f;
            float blendP = blend / (blend + Time.deltaTime);

            sFillAmount += Time.deltaTime;
            fillAmount -= Time.deltaTime;
            tubeMat.SetFloat("_FillAmount", fillAmount);
            SyringeMat.SetFloat("_FillAmount", sFillAmount);

            if (color.a > 0)
            {
                sColor = (1f - blendP) * color + blendP * sColor;
            }

            SyringeMat.SetColor("_Color", sColor);

            RecalculateWeights(tubeMat, SyringeMat, blend);

            SetPresserPos(sFillAmount);

        }
    }
}
