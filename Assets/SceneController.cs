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

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.O))
        {
            //switch scenes 
            meshPainterController.EnableSceneryNamed("DuckPond");
            meshPainterController.DisableSceneryNamed("Pond");
        }
        if (Input.GetKeyDown(KeyCode.I))
        {
            //switch scenes 
            meshPainterController.EnableSceneryNamed("Pond");
            meshPainterController.DisableSceneryNamed("DuckPond");
        }
    }
}
