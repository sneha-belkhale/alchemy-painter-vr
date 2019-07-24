﻿using UnityEngine;
using System.Collections;

public class FluidFiller : MonoBehaviour
{
    public GameObject Syringe;
    public GameObject Presser;
    public GameObject FluidToFill;
    private Material FluidMat;
    private Material SyringeMat;

    private float maxFill;
    private float minFill;

    // Use this for initialization
    void Start()
    {
        FluidMat = FluidToFill.GetComponent<Renderer>().material;
        SyringeMat = Syringe.GetComponent<Renderer>().material;
        SyringeMat.SetFloat("_FillAmount", -1f);
        SyringeMat.SetFloat("_GlitterPercent", 0);
        SyringeMat.SetFloat("_PoisonPercent", 0);
        SyringeMat.SetFloat("_ColorPercent", 0);
        SyringeMat.SetFloat("_RainbowPercent", 0);
        maxFill = 0.5f;
        minFill = -1f;
    }

    // Update is called once per frame
    void Update()
    {
        // ************** Keyboard Controls ************** //

        if (Input.GetKey(KeyCode.F))
        {
            RaycastHit hit;
            int layerMask = 1 << 8;

            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out hit, Mathf.Infinity, layerMask))
            {
                FluidMat = hit.collider.gameObject.GetComponent<Renderer>().material;
                FillTube(FluidMat);
            }
            else
            {
                EmptySyringe();
            }
        }

        if (Input.GetKey(KeyCode.G))
        {
            RaycastHit hit;
            int layerMask = 1 << 8;

            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out hit, Mathf.Infinity, layerMask))
            {
                FluidMat = hit.collider.gameObject.GetComponent<Renderer>().material;
                EmptyTube(FluidMat);
            }

            layerMask = 1 << 11;

            if (Physics.Raycast(ray, out hit, Mathf.Infinity, layerMask))
            {
                FluidMat = hit.collider.gameObject.GetComponent<Renderer>().material;
                EmptyTube(FluidMat);
            }
        }

        // ************** VR Controls ************** //

        if (OVRInput.Get(OVRInput.Button.SecondaryThumbstickDown))
        {
            RaycastHit hit;
            int layerMask = 1 << 8;

            Ray ray = new Ray();
            ray.origin = Syringe.transform.position;
            ray.direction = -Syringe.transform.up;
            if (Physics.Raycast(ray, out hit, Mathf.Infinity, layerMask))
            {
                FluidMat = hit.collider.gameObject.GetComponent<Renderer>().material;
                EmptyTube(FluidMat);
            }

            layerMask = 1 << 11;

            if (Physics.Raycast(ray, out hit, Mathf.Infinity, layerMask))
            {
                FluidMat = hit.collider.gameObject.GetComponent<Renderer>().material;
                EmptyTube(FluidMat);
            }
        }

        if (OVRInput.Get(OVRInput.Button.SecondaryThumbstickUp))
        {
            RaycastHit hit;
            int layerMask = 1 << 8;

            Ray ray = new Ray();
            ray.origin = Syringe.transform.position;
            ray.direction = -Syringe.transform.up;
            if (Physics.Raycast(ray, out hit, Mathf.Infinity, layerMask))
            {
                FluidMat = hit.collider.gameObject.GetComponent<Renderer>().material;
                FillTube(FluidMat);
            }
            else
            {
                EmptySyringe();
            }
        }
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

        if (sFillAmount > minFill && fillAmount < maxFill)
        {
            float blend = (fillAmount - minFill) / (Mathf.Abs(minFill) + maxFill);
            float blendP = blend / (blend + Time.deltaTime);

            sFillAmount -= Time.deltaTime;
            fillAmount += Time.deltaTime;
            tubeMat.SetFloat("_FillAmount", fillAmount);
            SyringeMat.SetFloat("_FillAmount", sFillAmount);

            color = (1f - blendP) * sColor + blendP * color;
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

            sColor = (1f - blendP) * color + blendP * sColor;
            SyringeMat.SetColor("_Color", sColor);

            RecalculateWeights(tubeMat, SyringeMat, blend);

            SetPresserPos(sFillAmount);

        }
    }
}
