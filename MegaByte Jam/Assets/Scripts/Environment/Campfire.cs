using UnityEngine;

public class Campfire : PlayerInteractableBase
{
    [Header("Campfire Settings")]
    [SerializeField] private ParticleSystem fireParticles;
    [SerializeField] private Light fireLight;
    [SerializeField] private AudioClip lightLanternSound;
    
    private AudioSource audioSource;
    private bool isLit = true;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
    }

    public override void Interact(GameObject player)
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
    
    public override string GetInteractionPrompt()
    {
        return interactionPrompt;
    }

    public override bool CanInteract(GameObject player)
    {
        if (!isLit) return false;

        PlayerInteraction playerInteraction = player.GetComponent<PlayerInteraction>();
        return playerInteraction != null && !playerInteraction.HasLitLantern;
    }

    public override PlayerInteractables GetInteractableType()
    {
        return interactableType;
    }
}
