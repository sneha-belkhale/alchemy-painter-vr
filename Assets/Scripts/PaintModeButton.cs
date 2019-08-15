using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PaintModeButton : MonoBehaviour
{
    private RaycastHit lastRaycastHit;
    private bool raycasted;

    private Material buttonMat;
    private bool paintMode;

    public Texture2D trianglePaintTex;
    public Texture2D objectPaintTex;

    private MeshPainterController meshPainterController;

    void Start()
    {
        paintMode = false;
        buttonMat = GetComponent<MeshRenderer>().material;

        GameObject meshPainter = GameObject.Find("MeshPainter");
        meshPainterController = meshPainter.GetComponent<MeshPainterController>();

    }

    void onRaycastHit(RaycastHit hit)
    {
        raycasted = true;
        lastRaycastHit = hit;
    }

    private void LateUpdate()
    {
        if (raycasted)
        {
            buttonMat.SetFloat("_Highlight", 0.5f);

            Vector2 texCoord = lastRaycastHit.textureCoord;
            buttonMat.SetVector("_CursorPos", new Vector4(texCoord.x, texCoord.y));

                if (Input.GetKeyDown(KeyCode.RightShift) || OVRInput.GetDown(OVRInput.Button.SecondaryIndexTrigger))
                {
                    paintMode = !paintMode;
                    meshPainterController.objectPaintMode = paintMode;

                if (paintMode)
                {
                    buttonMat.SetTexture("_MainTex", objectPaintTex);
                }
                else
                {
                    buttonMat.SetTexture("_MainTex", trianglePaintTex);
                }
            }
        }
        else
        {
            buttonMat.SetFloat("_Highlight", 0);
            buttonMat.SetVector("_CursorPos", new Vector4(0,0));
        }

        raycasted = false;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
