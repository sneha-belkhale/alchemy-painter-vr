using UnityEngine;
using System.Collections;

public class Raycaster : MonoBehaviour
{
    private GameObject rightController;
    private GameObject fluidFiller;
    private GameObject meshPainter;

    private LineRenderer lineRenderer;

    void Start()
    {
        rightController = GameObject.Find("RightControllerAnchor");

        fluidFiller = GameObject.Find("FluidFiller");

        meshPainter = GameObject.Find("MeshPainter");

        lineRenderer = GetComponent<LineRenderer>();
    }

    bool HandleRaycast(Ray ray, out RaycastHit hit)
    {
        //raycast against color wheel items first 
        int layerMask = 1 << 10 | 1 << 13;

        if (Physics.Raycast(ray, out hit, Mathf.Infinity, layerMask))
        {
            hit.collider.gameObject.SendMessage("onRaycastHit", hit);
            return true;
        }

        layerMask = 1 << 8 | 1 << 11;
        if (Physics.Raycast(ray, out hit, Mathf.Infinity, layerMask))
        {
            fluidFiller.SendMessage("onRaycastHit", hit);
            return true;
        }

        layerMask = 1 << 9;
        if (Physics.Raycast(ray, out hit, Mathf.Infinity, layerMask))
        {
            meshPainter.SendMessage("onRaycastHit", hit);
            return true;
        }

        return false;

    }

    // All raycaster logic will now go here... 
    void Update()
    {
        bool isConnected = OVRInput.IsControllerConnected(OVRInput.Controller.RTouch);

        RaycastHit hit;
        Ray ray;
        float distance; 

        if (!isConnected)
        {
            ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        }
        else
        {
            ray = new Ray(rightController.transform.position + 0.01f * rightController.transform.right, rightController.transform.forward);
        }

        if ( HandleRaycast(ray, out hit))
        {
            distance = hit.distance;
        }
        else
        {
            distance = 5;
        }

        lineRenderer.SetPosition(0, ray.origin + 0.07f * ray.direction);
        lineRenderer.SetPosition(1, ray.origin + ray.direction * distance);
    }
}
