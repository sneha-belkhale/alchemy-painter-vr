using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VRMover : MonoBehaviour
{
    public float speed = 1.3f;
    private float lastRotate;
    private float rotationTimeout = 0.3f;
    private Quaternion initalRot;
    private Vector3 initalPos;

    void Start()
    {
        // This camera is controlled by headset rotation
        initalPos = transform.position;
        initalRot = transform.rotation;
    }

    public void ResetToInitialTransform()
    {
        transform.position = initalPos;
        transform.rotation = initalRot;
    }

    IEnumerator fastRotate(float dir)
    {
        lastRotate = Time.fixedTime;

        float rotAmt = 0f;
        Quaternion initialQuat = transform.rotation;
        Quaternion finalQuat = Quaternion.AngleAxis(dir * 90f, transform.up) * transform.rotation;

        while(rotAmt <= 1.5)
        {
            transform.rotation = Quaternion.Lerp(transform.rotation, finalQuat, rotAmt);
            rotAmt += 5f * Time.deltaTime;
            yield return null;

        }

        yield return 0;
    }

    void Update()
    {
        Vector2 thumbstick = OVRInput.Get(OVRInput.Axis2D.PrimaryThumbstick);
        if (Mathf.Abs(thumbstick.x) > 0.001f & Mathf.Abs(thumbstick.y) > 0.001f)
        {
            Vector3 dir = Camera.main.transform.right * thumbstick.x + Camera.main.transform.forward * thumbstick.y; 
            transform.Translate(dir * speed * Time.deltaTime, Space.World);
        }

        if (OVRInput.GetDown(OVRInput.RawButton.X) && Time.fixedTime - lastRotate > rotationTimeout)
        {
            StartCoroutine("fastRotate", -1f);
        }
        if (OVRInput.GetDown(OVRInput.RawButton.Y) && Time.fixedTime - lastRotate > rotationTimeout)
        {
            StartCoroutine("fastRotate", 1f);
        }
    }
}