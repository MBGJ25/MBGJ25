using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class prototypeStickers : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && Input.GetKey(KeyCode.E))
        {
            gameObject.SetActive(false);
        }
    }
}
