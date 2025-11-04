using UnityEngine;

#region State Interfaces
public interface IInitializable 
{
    void Initialize();
    void CleanUp();
}

public interface ISaveable 
{
    void Save();
    void Load();
}

public interface IResettable 
{
    void Reset();
}
#endregion

#region World Object Interfaces
public interface IInteractable
{
    void   Interact(GameObject player);
    string GetInteractionPrompt();
    bool   CanInteract(GameObject player);
}
#endregion