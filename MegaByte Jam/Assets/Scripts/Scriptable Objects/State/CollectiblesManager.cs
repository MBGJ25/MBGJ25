using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu(fileName = "Collectible", menuName = "Scriptable Objects/State/New Collectible Data")]
public class CollectiblesManager : ScriptableObject
{
    public string name ;
    public Image image;
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && Input.GetKey(KeyCode.E))
        {
            //Add to the currentCollectibles
            // add to the list with the same name 
            // collectibleList.Add(name);
            // add to sticker counter
        }
    }

}
