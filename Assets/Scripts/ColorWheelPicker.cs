using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColorWheelPicker : MonoBehaviour
{
    private GameObject rightController;

    public Texture2D colorWheelTex;
    private Material colorWheelMat;

    public GameObject colorSourceTube;
    private Material colorSourceTubeMat;

    public GameObject rainbowSourceTube;
    private Material rainbowSourceTubeMat;

    private Color lastColorSelected;
    private bool isHovering;

    private MeshPainterController meshPainterController;
    public GameObject meshPainter;

    private bool paintMode; 

    // Start is called before the first frame update
    void Start()
    {
        rightController = GameObject.Find("RightControllerAnchor");

        colorWheelMat = GetComponent<MeshRenderer>().material;

        colorSourceTubeMat = colorSourceTube.GetComponent<Renderer>().material;
        rainbowSourceTubeMat = rainbowSourceTube.GetComponent<Renderer>().material;

        lastColorSelected = colorSourceTubeMat.GetColor("_Color");
        isHovering = false;

        meshPainterController = meshPainter.GetComponent<MeshPainterController>();
        paintMode = false;
    }

    void HandlePlatformUpdate(Ray ray, bool selectColorEvent)
    {
        RaycastHit hit;
        int layerMask = 1 << 10;

        if (Physics.Raycast(ray, out hit, Mathf.Infinity, layerMask))
        {
            //hit the color wheel 
            isHovering = true;
            Vector2 texCoord = hit.textureCoord;
            Color color = colorWheelTex.GetPixelBilinear(texCoord.x, texCoord.y);

            colorWheelMat.SetVector("_CursorPos", new Vector4(texCoord.x, texCoord.y));

            colorSourceTubeMat.SetColor("_Color", color);
            rainbowSourceTubeMat.SetColor("_Color", color);

            if (selectColorEvent)
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

        // check paint mode button 
        layerMask = 1 << 13;
        if (Physics.Raycast(ray, out hit, Mathf.Infinity, layerMask) && selectColorEvent)
        {
            paintMode = !paintMode;
            meshPainterController.objectPaintMode = paintMode;
        }
    }

    // Update is called once per frame
    void Update()
    {
        bool isConnected = OVRInput.IsControllerConnected(OVRInput.Controller.RTouch);

        if (!isConnected)
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            HandlePlatformUpdate(ray, Input.GetKeyDown(KeyCode.RightShift));
        }
        else
        {
            Ray ray = new Ray(rightController.transform.position, rightController.transform.forward);
            HandlePlatformUpdate(ray, OVRInput.GetDown(OVRInput.Button.SecondaryIndexTrigger));
        }
    }
}
