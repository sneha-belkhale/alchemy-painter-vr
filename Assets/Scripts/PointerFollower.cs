using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PointerFollower : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {


    }

    // Update is called once per frame
    void Update()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        transform.position = ray.origin + 0.5f * ray.direction + 0.5f * transform.up;
    }
}
