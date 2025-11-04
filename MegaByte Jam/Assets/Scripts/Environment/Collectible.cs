using UnityEngine;
public class Collectible : PlayerInteractableBase
{
    

    public override string GetInteractionPrompt()
    {
        throw new System.NotImplementedException();
    }
    public override PlayerInteractables GetInteractableType()
    {
        throw new System.NotImplementedException();
    }
    public override bool CanInteract(GameObject player)
    {
        return true;
    }
    public override void Interact(GameObject player)
    {
        // TODO: Add to player inventory
    }
}