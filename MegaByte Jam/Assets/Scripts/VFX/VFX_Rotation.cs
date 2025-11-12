using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VFX_Rotation : MonoBehaviour
{
    [SerializeField] private Vector3 _rotation;
    [SerializeField] private float _rotationSpeed;

    void Update()
    {
        transform.Rotate(_rotation * _rotationSpeed * Time.deltaTime);
    }
}

