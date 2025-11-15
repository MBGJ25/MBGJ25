using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rotator : MonoBehaviour
{
    private float i = 0f;
    private bool up = true;
    private void Start()
    {
         
    }

    void FixedUpdate()
    {
        transform.Rotate(0, 1f, 0, Space.Self);

    }
}
