using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using PhysicsCharacterController;

public class PlayerInteraction : MonoBehaviour
{
    #region Fields and References
    [Header("Interaction Settings")]
    [SerializeField] private float interactionRange = 2f;
    [SerializeField] private LayerMask interactionLayer;
    
    [Header("UI References")]
    [SerializeField] private Text interactionTextPrompt;
    
    [Header("System References")]
    [SerializeField] private InputReader input;
    // CS TODO: Implement once FMOD branch is merged in
    // [SerializeField] private PlayerSounds playerSounds;

    [Header("Lantern References")]
    [SerializeField] private GameObject lanternLight;
    [SerializeField] private ParticleSystem lanternParticles;
    [SerializeField] private GameObject temporaryFireVFX;
    [SerializeField] private float lanternBurnoutTime = 60f;

    private IInteractable currentInteractable;
    private bool hasLitLantern = false;
    private float lanternTimeRemaining;
    
    // Collectible tracking
    private HashSet<string> collectedItemIDs = new HashSet<string>();
    private List<CollectibleData> collectedItems = new List<CollectibleData>();
    
    // Public properties
    public bool HasLitLantern => hasLitLantern;
    public float LanternTimeRemaining => lanternTimeRemaining;
    public int CollectiblesCount => collectedItems.Count;
    public IReadOnlyList<CollectibleData> CollectedItems => collectedItems.AsReadOnly();
    
    // Events
    public event Action<CollectibleData> OnCollectiblePickedUp;
    #endregion

    #region Lifecycle Methods
    private void OnEnable()
    {
        if (input != null)
            input.InteractEvent += HandleInteract;
    }

    private void OnDisable()
    {
        if (input != null)
            input.InteractEvent -= HandleInteract;
    }

    private void Update()
    {
        // Check for interactables every frame
        CheckForInteractable();

        // Count down lantern timer
        if (hasLitLantern && lanternTimeRemaining > 0f)
        {
            lanternTimeRemaining -= Time.deltaTime;
            
            if (lanternTimeRemaining <= 0f)
            {
                lanternTimeRemaining = 0f;
                ExtinguishLantern();
            }
        }
    }
    #endregion
    
    
    #region Interaction Methods
    private void HandleInteract()
    {
        if (currentInteractable != null)
        {
            currentInteractable.Interact(gameObject);
        }
    }

    private void CheckForInteractable()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, interactionRange, interactionLayer);

        IInteractable closestInteractableObject = null;
        float closestInteractableObjectDistance = interactionRange;

        foreach (Collider hit in hits)
        {
            IInteractable interactable = hit.GetComponent<IInteractable>();
            if (interactable != null && interactable.CanInteract(gameObject))
            {
                float distance = Vector3.Distance(transform.position, hit.transform.position);

                if (distance < closestInteractableObjectDistance)
                {
                    closestInteractableObjectDistance = distance;
                    closestInteractableObject = interactable;
                }
            }
        }
        
        currentInteractable = closestInteractableObject;
        
        if (interactionTextPrompt != null)
        {
            if (currentInteractable != null)
                interactionTextPrompt.text = currentInteractable.GetInteractionPrompt();
            else 
                interactionTextPrompt.text = "";
        }
    }
    #endregion

    #region Lantern Methods
    public void LightLantern()
    {
        hasLitLantern = true;
        lanternTimeRemaining = lanternBurnoutTime;
    
        if (lanternLight != null)
            lanternLight.SetActive(true);
    
        if (lanternParticles != null)
            lanternParticles.Play();
    
        if (temporaryFireVFX != null)
        {
            temporaryFireVFX.SetActive(true);
            
            ParticleSystem vfxParticles = temporaryFireVFX.GetComponent<ParticleSystem>();
            if (vfxParticles != null)
            {
                vfxParticles.Play();
                float vfxDuration = 3f;
                Invoke(nameof(DisableTemporaryVFX), vfxDuration);
            }
        }
    
        #if UNITY_EDITOR
        Debug.Log($"Lantern lit! Will burn out in {lanternBurnoutTime} seconds");
        #endif
    }
    
    private void ExtinguishLantern()
    {
        hasLitLantern = false;
        lanternTimeRemaining = 0f;
        
        if (lanternLight != null)
            lanternLight.SetActive(false);
        
        if (lanternParticles != null)
            lanternParticles.Stop();
        
        #if UNITY_EDITOR
        Debug.Log("Lantern burned out!");
        #endif
    }

    private void DisableTemporaryVFX()
    {
        if (temporaryFireVFX != null)
            temporaryFireVFX.SetActive(false);
    }
    #endregion

    #region Collectible Methods
    /// <summary>
    /// Attempt to collect an item. Returns true if successfully collected, false if already collected or invalid.
    /// </summary>
    public bool CollectItem(CollectibleData collectibleData)
    {
        // Validation
        if (collectibleData == null)
        {
            #if UNITY_EDITOR
            Debug.LogWarning("Attempted to collect null collectible data!");
            #endif
            return false;
        }

        // Check if already collected
        if (collectedItemIDs.Contains(collectibleData.CollectibleID))
        {
            #if UNITY_EDITOR
            Debug.LogWarning($"Already collected: {collectibleData.CollectibleName}");
            #endif
            return false;
        }

        // Add to both collections
        collectedItemIDs.Add(collectibleData.CollectibleID);
        collectedItems.Add(collectibleData);
        
        // CS TODO: Implement once FMOD is set up
        // if (playerSounds != null)
        // {
        //     playerSounds.PlayCollectiblePickup(collectibleData.PickupSound);
        // }
        
        // Fire event for other systems
        OnCollectiblePickedUp?.Invoke(collectibleData);
        
        #if UNITY_EDITOR
        Debug.Log($"Collected: {collectibleData.CollectibleName} ({collectedItems.Count} total)");
        #endif
        
        return true;
    }

    /// <summary>
    /// Check if a specific collectible has been collected
    /// </summary>
    public bool HasCollected(CollectibleData collectibleData)
    {
        if (collectibleData == null)
            return false;
            
        return collectedItemIDs.Contains(collectibleData.CollectibleID);
    }

    /// <summary>
    /// Check if a collectible with a specific ID has been collected
    /// </summary>
    public bool HasCollected(string collectibleID)
    {
        return collectedItemIDs.Contains(collectibleID);
    }

    /// <summary>
    /// Reset all collected items (call this when restarting the game/level)
    /// </summary>
    public void ResetCollectibles()
    {
        collectedItemIDs.Clear();
        collectedItems.Clear();
        
        #if UNITY_EDITOR
        Debug.Log("Collectibles reset!");
        #endif
    }

    /// <summary>
    /// Get a specific collected item by index
    /// </summary>
    public CollectibleData GetCollectedItem(int index)
    {
        if (index >= 0 && index < collectedItems.Count)
            return collectedItems[index];
            
        return null;
    }
    #endregion

    #region Debug
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactionRange);
    }
    #endregion
}