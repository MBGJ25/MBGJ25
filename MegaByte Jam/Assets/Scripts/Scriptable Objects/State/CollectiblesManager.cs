using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Collectible", menuName = "Scriptable Objects/State/New Collectible Data")]
public class CollectiblesManager : ScriptableObject
{

    public GameObject Sticker;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && Input.GetKey(KeyCode.E))
        {
               //Add to the currentCollectibles
        }
    }

}
