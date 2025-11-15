using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GetCollectible : MonoBehaviour
{

    private int Collectible = 0;

    public TextMeshProUGUI collectibleText;

    private void Start()
    {
        collectibleText.text = Collectible.ToString() + "/10";
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.transform.tag == "Collectible")
        {
            Collectible++;
            collectibleText.text = Collectible.ToString() + "/10";
            Debug.Log(Collectible);
            Destroy(other.gameObject);
        }
    }

}
