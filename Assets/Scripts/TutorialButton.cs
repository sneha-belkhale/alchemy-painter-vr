using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialButton : MonoBehaviour
{
    public GameObject colorWheel;

    private RaycastHit lastRaycastHit;
    private bool raycasted;

    private Material buttonMat;

    private MeshPainterController meshPainterController;

    void Start()
    {
        buttonMat = GetComponent<MeshRenderer>().material;

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
                GameObject[] g =GameObject.FindGameObjectsWithTag("Tutorial");
                if (g[0].transform.GetChild(0).gameObject.activeSelf)
                {
                    colorWheel.GetComponent<MeshRenderer>().material.SetFloat("_Alpha", 1f);
                    for ( int i = 0; i<g.Length; i++)
                    {
                        for (int j = 0; j < g[i].transform.childCount; j++)
                        {
                            g[i].transform.GetChild(j).gameObject.SetActive(false);
                        }
                    }
                }
                else
                {
                    colorWheel.GetComponent<MeshRenderer>().material.SetFloat("_Alpha", 0.2f);
                    for (int i = 0; i < g.Length; i++)
                    {
                        for (int j = 0; j < g[i].transform.childCount; j++)
                        {
                            g[i].transform.GetChild(j).gameObject.SetActive(true);
                        }
                    }
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
