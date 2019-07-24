﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColorWheelPicker : MonoBehaviour
{
    public Texture2D colorWheelTex;
    public GameObject colorSourceTube;
    private Material colorSourceTubeMat;

    public GameObject rainbowSourceTube;
    private Material rainbowSourceTubeMat;

    public GameObject syringe;


    private Color lastColorSelected;
    private bool isHovering;
    // Start is called before the first frame update
    void Start()
    {
        colorSourceTubeMat = colorSourceTube.GetComponent<Renderer>().material;
        rainbowSourceTubeMat = rainbowSourceTube.GetComponent<Renderer>().material;
        lastColorSelected = colorSourceTubeMat.GetColor("_Color");
        isHovering = false;
    }

    // Update is called once per frame
    void Update()
    {
        RaycastHit hit;
        int layerMask = 1 << 10;

        // ************** Keyboard Controls ************** //

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out hit, Mathf.Infinity, layerMask))
        {
            //hit the color wheel 
            isHovering = true;
            Vector2 texCoord = hit.textureCoord;
            Color color = colorWheelTex.GetPixelBilinear(texCoord.x, texCoord.y);
            colorSourceTubeMat.SetColor("_Color", color);
            rainbowSourceTubeMat.SetColor("_Color", color);

            if (Input.GetKey(KeyCode.RightShift))
            {
                lastColorSelected = color;
            }
        }
        else if(isHovering)
        {
            //restore to default state
            colorSourceTubeMat.SetColor("_Color", lastColorSelected);
            rainbowSourceTubeMat.SetColor("_Color", lastColorSelected);
            isHovering = false;
        }

        // ************** VR Controls ************** //

        ray.origin = syringe.transform.position;
        ray.direction = -syringe.transform.up;
        if (Physics.Raycast(ray, out hit, Mathf.Infinity, layerMask))
        {
            //hit the color wheel 
            isHovering = true;
            Vector2 texCoord = hit.textureCoord;
            Color color = colorWheelTex.GetPixelBilinear(texCoord.x, texCoord.y);
            colorSourceTubeMat.SetColor("_Color", color);
            rainbowSourceTubeMat.SetColor("_Color", color);

            if (OVRInput.Get(OVRInput.Button.SecondaryIndexTrigger))
            {
                lastColorSelected = color;
            }
        }
        else if (isHovering)
        {
            //restore to default state
            colorSourceTubeMat.SetColor("_Color", lastColorSelected);
            rainbowSourceTubeMat.SetColor("_Color", lastColorSelected);
            isHovering = false;
        }
    }
}
