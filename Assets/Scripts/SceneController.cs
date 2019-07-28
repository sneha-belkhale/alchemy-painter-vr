using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SceneController : MonoBehaviour
{
    public GameObject meshPainterControllerGo;

    private MeshPainterController meshPainterController;
    // Start is called before the first frame update
    void Start()
    {
        meshPainterController = meshPainterControllerGo.GetComponent<MeshPainterController>();
    }

    void HandlePlatformUpdate(Ray ray, bool selectEvent)
    {
        if (selectEvent)
        {
            int layerMask = 1 << 12;
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, Mathf.Infinity, layerMask))
            {
                //get name of hit collider 
                string sceneName = hit.collider.gameObject.name.Substring(5);
                meshPainterController.EnableSceneryNamed(sceneName);
                gameObject.SetActive(false);
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        //if (Input.GetKeyDown(KeyCode.O))
        //{
        //    //switch scenes 
        //    meshPainterController.EnableSceneryNamed("DuckPond");
        //    meshPainterController.DisableSceneryNamed("Pond");
        //}

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
            //Ray ray = new Ray();
            //ray.origin = Syringe.transform.position;
            //ray.direction = -Syringe.transform.up;
            //HandlePlatformUpdate(ray, OVRInput.GetDown(OVRInput.Button.SecondaryIndexTrigger));
        }
    }
}
