using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GetCollectible : MonoBehaviour
{

    public TextMeshProUGUI collectibleText;
    [SerializeField] private int totalStickers;
    [SerializeField] private int Collectible = 0;

    private void Start()
    {
        collectibleText.text = Collectible.ToString() + "/" + totalStickers;
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.transform.tag == "Collectible")
        {
            FMODUnity.RuntimeManager.PlayOneShot("event:/SFX/UI/GetCollectible");
            Collectible++;
            collectibleText.text = Collectible.ToString() + "/" + totalStickers;
            Debug.Log(Collectible);
            Destroy(other.gameObject);
        }
    }

}
