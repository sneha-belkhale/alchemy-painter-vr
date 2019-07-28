using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SceneController : MonoBehaviour
{
    public GameObject meshPainterControllerGo;
    private GameObject rightController;

    private MeshPainterController meshPainterController;
    private Material lastHighlightedMat;
    // Start is called before the first frame update
    void Start()
    {
        meshPainterController = meshPainterControllerGo.GetComponent<MeshPainterController>();
        rightController = GameObject.Find("RightControllerAnchor");
    }

    void HandlePlatformUpdate(Ray ray, bool selectEvent)
    {
        if (lastHighlightedMat)
        {
            lastHighlightedMat.SetFloat("_OutlineIntensity", 0f);
        }
        int layerMask = 1 << 12;
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, Mathf.Infinity, layerMask))
        {
            //get name of hit collider 
            string sceneName = hit.collider.gameObject.name.Substring(5);

            if (selectEvent)
            {
                meshPainterController.EnableSceneryNamed(sceneName);
                gameObject.SetActive(false);
            }
            else
            {
                //just highlight 
                lastHighlightedMat = hit.collider.gameObject.GetComponent<MeshRenderer>().material;
                lastHighlightedMat.SetFloat("_OutlineIntensity", 1f);
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        //raycast against menu items
        bool isConnected = OVRInput.IsControllerConnected(OVRInput.Controller.RTouch);

        // ************** Keyboard Controls ************** //
        if (!isConnected)
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            HandlePlatformUpdate(ray, Input.GetMouseButtonDown(0));
        }
        // ************** VR Controls ************** //
        else
        {
            Ray ray = new Ray(rightController.transform.position, rightController.transform.forward);
            HandlePlatformUpdate(ray, OVRInput.GetDown(OVRInput.Button.SecondaryIndexTrigger));
        }
    }
}
