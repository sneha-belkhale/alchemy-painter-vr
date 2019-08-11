using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColorWheelPicker : MonoBehaviour
{
    public Texture2D colorWheelTex;
    private Material colorWheelMat;

    public GameObject colorSourceTube;
    private Material colorSourceTubeMat;

    public GameObject rainbowSourceTube;
    private Material rainbowSourceTubeMat;

    public Color lastColorSelected;
    private bool isHovering;

    private RaycastHit lastRaycastHit;
    private bool raycasted;


    // Start is called before the first frame update
    void Start()
    {
        colorWheelMat = GetComponent<MeshRenderer>().material;

        colorSourceTubeMat = colorSourceTube.GetComponent<Renderer>().material;
        rainbowSourceTubeMat = rainbowSourceTube.GetComponent<Renderer>().material;

        colorSourceTubeMat.SetColor("_Color", lastColorSelected);
        rainbowSourceTubeMat.SetColor("_Color", lastColorSelected);

        isHovering = false;
    }

    void onRaycastHit( RaycastHit hit)
    {
        raycasted = true;
        lastRaycastHit = hit; 
    }
    private void LateUpdate()
    {
        if (raycasted)
        {
            //hit the color wheel 
            isHovering = true;
            Vector2 texCoord = lastRaycastHit.textureCoord;
            Color color = colorWheelTex.GetPixelBilinear(texCoord.x, texCoord.y);
            colorWheelMat.SetVector("_CursorPos", new Vector4(texCoord.x, texCoord.y));

            colorSourceTubeMat.SetColor("_Color", color);
            rainbowSourceTubeMat.SetColor("_Color", color);

            if (Input.GetKeyDown(KeyCode.RightShift) || OVRInput.GetDown(OVRInput.Button.SecondaryIndexTrigger))
            {
                lastColorSelected = color;
            }
        }
        else if (isHovering)
        {
            colorWheelMat.SetVector("_CursorPos", new Vector4(-1f, -1f));

            colorSourceTubeMat.SetColor("_Color", lastColorSelected);
            rainbowSourceTubeMat.SetColor("_Color", lastColorSelected);

            isHovering = false;
        }
        raycasted = false; 
    }
}
