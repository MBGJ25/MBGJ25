using UnityEngine;

public abstract class PlayerInteractableBase : MonoBehaviour, IInteractable
{
    [Header("Interactable Type")]
    [SerializeField] private PlayerInteractables interactableType;
    [SerializeField] private string interactablePrompt;
    
    public abstract bool CanInteract(GameObject player);
    public abstract void Interact(GameObject player);
    public abstract string GetInteractionPrompt();
    public abstract PlayerInteractables GetInteractableType();
}
