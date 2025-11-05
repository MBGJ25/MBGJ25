using UnityEngine;

public class Collectible : PlayerInteractableBase
{
    [Header("Collectible Data")]
    [SerializeField] private CollectibleData data;
    
    private GameObject spawnedModel;
    private bool hasBeenCollected = false;

    private void Start()
    {
        // Spawn the visual model from the ScriptableObject data
        if (data != null && data.WorldModel != null)
        {
            spawnedModel = Instantiate(data.WorldModel, transform);
            spawnedModel.transform.localPosition = Vector3.zero;
            spawnedModel.transform.localRotation = Quaternion.identity;
        }
        else
        {
            #if UNITY_EDITOR
            Debug.LogWarning($"Collectible on {gameObject.name} is missing data or world model!", this);
            #endif
        }
    }

    public override bool CanInteract(GameObject player)
    {
        // Can't interact if already collected or missing data
        if (hasBeenCollected || data == null)
            return false;

        return true;
    }

    public override void Interact(GameObject player)
    {
        // Safety checks
        if (hasBeenCollected || data == null)
            return;

        PlayerInteraction playerInteraction = player.GetComponent<PlayerInteraction>();
        if (playerInteraction != null)
        {
            // Attempt to collect the item through the player's inventory system
            bool collected = playerInteraction.CollectItem(data);
            
            if (collected)
            {
                hasBeenCollected = true;
                
                // Spawn pickup particles if available in the data
                if (data.PickupParticles != null)
                {
                    ParticleSystem particles = Instantiate(
                        data.PickupParticles, 
                        transform.position, 
                        Quaternion.identity
                    );
                    
                    // Auto-destroy particles after they finish playing
                    Destroy(particles.gameObject, particles.main.duration + particles.main.startLifetime.constantMax);
                }
                
                #if UNITY_EDITOR
                Debug.Log($"Collected: {data.CollectibleName}");
                #endif
                
                // Disable the collectible GameObject
                gameObject.SetActive(false);
            }
        }
    }

    public override string GetInteractionPrompt()
    {
        if (data == null)
            return $"[E] Pick up {data.CollectibleName}";

        return interactionPrompt;
    }

    public override PlayerInteractables GetInteractableType()
    {
        return interactableType;
    }

    // Public getter for external access to the collectible data
    public CollectibleData Data => data;
    public bool HasBeenCollected => hasBeenCollected;
}