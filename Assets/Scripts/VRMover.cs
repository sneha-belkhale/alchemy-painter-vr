using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VRMover : MonoBehaviour
{
  public float speed = 1.0f;

  void Start()
  {
    // This camera is controlled by headset rotation
  }

  void Update()
  {
        Vector2 thumbstick = OVRInput.Get(OVRInput.Axis2D.PrimaryThumbstick);
        if (Mathf.Abs(thumbstick.x)>0.001f & Mathf.Abs(thumbstick.y)>0.001f)
        {
            Vector3 dir = Camera.main.transform.right * thumbstick.x + Camera.main.transform.forward * thumbstick.y;
            transform.Translate(dir * speed * Time.deltaTime, Space.World);
        }
    }
}