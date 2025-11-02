using System;
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

    [Header("Lantern References")]
    [SerializeField] private GameObject lanternLight;
    [SerializeField] private ParticleSystem lanternParticles;
    [SerializeField] private GameObject temporaryFireVFX;
    [SerializeField] private float lanternBurnoutTime = 60f; // ✨ Default 60 seconds

    private IInteractable currentInteractable;
    private bool hasLitLantern = false;
    private float lanternTimeRemaining;
    public bool HasLitLantern => hasLitLantern;
    public float LanternTimeRemaining => lanternTimeRemaining;
    #endregion

    #region Lifecycle Methods
    private void Update()
    {
        // ✨ Check for interactables every frame
        CheckForInteractable();
        
        // ✨ Handle interaction immediately when input is pressed
        if (input.interact && currentInteractable != null) 
        {
            currentInteractable.Interact(gameObject);
        }

        // ✨Count down lantern timer
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
    
    
    #region Methods
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

    // ✨Extinguish the lantern
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

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactionRange);
    }
    #endregion
}