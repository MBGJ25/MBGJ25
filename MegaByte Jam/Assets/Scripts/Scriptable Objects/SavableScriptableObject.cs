using UnityEngine;

/// <summary>
/// Base class for all ScriptableObjects that need to be saved and loaded.
/// Inherit from this instead of ScriptableObject directly.
/// </summary>
public abstract class SavableScriptableObject : ScriptableObject
{
    /// <summary>
    /// Unique key used for saving/loading this ScriptableObject.
    /// Override this in derived classes to provide a unique identifier.
    /// </summary>
    public abstract string SaveKey { get; }
    
    /// <summary>
    /// Serializes this ScriptableObject to JSON format.
    /// Override this if you need custom serialization logic.
    /// </summary>
    public virtual string SerializeToJson()
    {
        return JsonUtility.ToJson(this);
    }
    
    /// <summary>
    /// Deserializes JSON data into this ScriptableObject.
    /// Override this if you need custom deserialization logic.
    /// </summary>
    public virtual void DeserializeFromJson(string json)
    {
        JsonUtility.FromJsonOverwrite(json, this);
    }
    
    /// <summary>
    /// Called after data is loaded into this ScriptableObject.
    /// Override this if you need to perform any initialization after loading.
    /// </summary>
    public virtual void OnAfterLoad() { }
}
