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

    private IInteractable currentInteractable;
    private bool hasLitLantern = false;
    private bool interact;
    public bool HasLitLantern => hasLitLantern;
    #endregion

    #region Lifecycle Methods
    private void Update()
    {
        interact = input.interact;
        CheckForInteractable();
    }

    private void FixedUpdate()
    {
        if (interact && currentInteractable != null) 
            currentInteractable.Interact(gameObject);
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
        
        // CS TODO: Updating the UI--Reinstate when we have UI
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
        
        if (lanternLight != null)
            lanternLight.SetActive(true);
        
        if (lanternParticles != null)
            lanternParticles.Play();
        
        #if UNITY_EDITOR
        Debug.Log("We lit babyyyy");
        #endif
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactionRange);
    }
    #endregion
}
