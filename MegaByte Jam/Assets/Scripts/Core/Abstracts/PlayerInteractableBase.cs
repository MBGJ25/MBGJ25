using UnityEngine;

public abstract class PlayerInteractableBase : MonoBehaviour, IInteractable
{
    [Header("Interactable Settings")]
    [SerializeField] protected PlayerInteractables interactableType;
    [SerializeField] protected string interactionPrompt;
    
    public abstract bool CanInteract(GameObject player);
    public abstract void Interact(GameObject player);
    public abstract string GetInteractionPrompt();
    public abstract PlayerInteractables GetInteractableType();
}
