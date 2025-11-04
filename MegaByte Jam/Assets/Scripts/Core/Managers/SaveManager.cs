using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Manages saving and loading of all SavableScriptableObjects in the game.
/// Automatically loads data on Awake and saves on application quit.
/// </summary>
public class SaveManager : SingletonMonoBehaviour<SaveManager>
{
    [Header("Savable Objects")]
    [Tooltip("Drag and drop all ScriptableObjects that need to be saved here")]
    [SerializeField] private List<SavableScriptableObject> savableObjects = new List<SavableScriptableObject>();
    
    [Header("Settings")]
    [SerializeField] private string saveFileName = "GameSave.es3";
    [SerializeField] private bool loadOnStart = true;
    [SerializeField] private bool saveOnQuit = true;
    [SerializeField] private bool debugMode = false;
    
    protected override void OnAwakeSingleton()
    {
        if (loadOnStart)
        {
            LoadAll();
        }
    }
    
    /// <summary>
    /// Saves all registered SavableScriptableObjects to disk.
    /// </summary>
    public void SaveAll()
    {
        if (savableObjects == null || savableObjects.Count == 0)
        {
            if (debugMode) Debug.LogWarning("SaveManager: No savable objects registered!");
            return;
        }
        
        int savedCount = 0;
        foreach (var savableObject in savableObjects)
        {
            if (savableObject == null) continue;
            
            try
            {
                string jsonData = savableObject.SerializeToJson();
                ES3.Save(savableObject.SaveKey, jsonData, saveFileName);
                savedCount++;
                
                if (debugMode) 
                    Debug.Log($"SaveManager: Saved {savableObject.SaveKey}");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"SaveManager: Failed to save {savableObject.SaveKey}. Error: {e.Message}");
            }
        }
        
        if (debugMode) 
            Debug.Log($"SaveManager: Successfully saved {savedCount}/{savableObjects.Count} objects");
    }
    
    /// <summary>
    /// Loads all registered SavableScriptableObjects from disk.
    /// If no save data exists, keeps the default values.
    /// </summary>
    public void LoadAll()
    {
        if (savableObjects == null || savableObjects.Count == 0)
        {
            if (debugMode) Debug.LogWarning("SaveManager: No savable objects registered!");
            return;
        }
        
        int loadedCount = 0;
        foreach (var savableObject in savableObjects)
        {
            if (savableObject == null) continue;
            
            try
            {
                if (ES3.KeyExists(savableObject.SaveKey, saveFileName))
                {
                    string jsonData = ES3.Load<string>(savableObject.SaveKey, saveFileName);
                    savableObject.DeserializeFromJson(jsonData);
                    savableObject.OnAfterLoad();
                    
                    loadedCount++;
                    
                    if (debugMode) 
                        Debug.Log($"SaveManager: Loaded {savableObject.SaveKey}");
                }
                else
                {
                    if (debugMode) 
                        Debug.Log($"SaveManager: No save data found for {savableObject.SaveKey}, using default values");
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"SaveManager: Failed to load {savableObject.SaveKey}. Error: {e.Message}");
            }
        }
        
        if (debugMode) 
            Debug.Log($"SaveManager: Successfully loaded {loadedCount}/{savableObjects.Count} objects");
    }
    
    /// <summary>
    /// Saves a specific SavableScriptableObject.
    /// </summary>
    public void Save(SavableScriptableObject savableObject)
    {
        if (savableObject == null)
        {
            Debug.LogError("SaveManager: Cannot save null object");
            return;
        }
        
        try
        {
            string jsonData = savableObject.SerializeToJson();
            ES3.Save(savableObject.SaveKey, jsonData, saveFileName);
            
            if (debugMode) 
                Debug.Log($"SaveManager: Saved {savableObject.SaveKey}");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"SaveManager: Failed to save {savableObject.SaveKey}. Error: {e.Message}");
        }
    }
    
    /// <summary>
    /// Loads a specific SavableScriptableObject.
    /// </summary>
    public void Load(SavableScriptableObject savableObject)
    {
        if (savableObject == null)
        {
            Debug.LogError("SaveManager: Cannot load null object");
            return;
        }
        
        try
        {
            if (ES3.KeyExists(savableObject.SaveKey, saveFileName))
            {
                string jsonData = ES3.Load<string>(savableObject.SaveKey, saveFileName);
                savableObject.DeserializeFromJson(jsonData);
                savableObject.OnAfterLoad();
                
                if (debugMode) 
                    Debug.Log($"SaveManager: Loaded {savableObject.SaveKey}");
            }
            else
            {
                if (debugMode) 
                    Debug.Log($"SaveManager: No save data found for {savableObject.SaveKey}");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"SaveManager: Failed to load {savableObject.SaveKey}. Error: {e.Message}");
        }
    }
    
    /// <summary>
    /// Deletes all save data.
    /// </summary>
    public void DeleteAllSaveData()
    {
        if (ES3.FileExists(saveFileName))
        {
            ES3.DeleteFile(saveFileName);
            if (debugMode) Debug.Log("SaveManager: Deleted all save data");
        }
    }
    
    /// <summary>
    /// Checks if any save data exists.
    /// </summary>
    public bool SaveDataExists()
    {
        return ES3.FileExists(saveFileName);
    }
    
    protected override void OnApplicationQuit()
    {
        base.OnApplicationQuit();
        
        if (saveOnQuit)
        {
            SaveAll();
        }
    }
    
    // For mobile platforms
    private void OnApplicationPause(bool pauseStatus)
    {
        if (pauseStatus && saveOnQuit)
        {
            SaveAll();
        }
    }
}