using UnityEngine;

public class Campfire : MonoBehaviour, IInteractable
{
    [Header("Campfire Settings")]
    [SerializeField] private ParticleSystem fireParticles;
    [SerializeField] private Light fireLight;
    [SerializeField] private AudioClip lightLanternSound;
    [SerializeField] private string interactionPrompt = "Press F/Y to Light Lantern";
    
    private AudioSource audioSource;
    private bool isLit = true;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
    }

    public void Interact(GameObject player)
    {
        PlayerInteraction playerInteraction = player.GetComponent<PlayerInteraction>();
        if (playerInteraction != null && !playerInteraction.HasLitLantern)
        {
            playerInteraction.LightLantern();
            
            if (audioSource != null && lightLanternSound != null)
                audioSource.PlayOneShot(lightLanternSound);
            
            #if UNITY_EDITOR
            Debug.Log("Interact method fired in campfire");
            #endif
        }
    }
    
    public string GetInteractionPrompt()
    {
        return interactionPrompt;
    }

    public bool CanInteract(GameObject player)
    {
        if (!isLit) return false;

        PlayerInteraction playerInteraction = player.GetComponent<PlayerInteraction>();
        return playerInteraction != null && !playerInteraction.HasLitLantern;
    }
}
