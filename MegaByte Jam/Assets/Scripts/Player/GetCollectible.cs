using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GetCollectible : MonoBehaviour
{

    private int Collectible = 0;

    public TextMeshProUGUI collectibleText;

    private void OnTriggerEnter(Collider other)
    {
        if (other.transform.tag == "Collectible")
        {
            Collectible++;
            collectibleText.text = "Coin: " + Collectible.ToString();
            Debug.Log(Collectible);
            Destroy(other.gameObject);
        }
    }

}
