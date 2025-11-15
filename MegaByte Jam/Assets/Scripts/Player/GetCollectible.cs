using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GetCollectible : MonoBehaviour
{

    private int Collectible = 0;

    public TextMeshProUGUI collectibleText;
    [SerializeField]private int totalStickers;

    private void Start()
    {
        collectibleText.text = Collectible.ToString() + "/" + totalStickers;
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.transform.tag == "Collectible")
        {
            Collectible++;
            collectibleText.text = Collectible.ToString() + "/" + totalStickers;
            Debug.Log(Collectible);
            Destroy(other.gameObject);
        }
    }

}
