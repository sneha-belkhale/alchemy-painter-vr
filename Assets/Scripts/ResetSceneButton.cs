using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResetSceneButton : MonoBehaviour
{
    private RaycastHit lastRaycastHit;
    private bool raycasted;

    private Material buttonMat;

    private MeshPainterController meshPainterController;

    void Start()
    {
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
                meshPainterController.ResetActiveScene();
            }
        }
        else
        {
            buttonMat.SetFloat("_Highlight", 0);
            buttonMat.SetVector("_CursorPos", new Vector4(0, 0));
        }

        raycasted = false;
    }

    // Update is called once per frame
    void Update()
    {

    }
}
