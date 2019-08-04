using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScreenShotter : MonoBehaviour
{
    // Start is called before the first frame update
    private float index;
    public string name;
    void Start()
    {
        index = 0.0f;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKey(KeyCode.K))
        {
            string fullPath = Application.dataPath + "/Textures/"+ name + ".png";
            ScreenCapture.CaptureScreenshot(fullPath);
            index += 1.0f;
        }
    }
}
