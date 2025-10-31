using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class SceneControllerManager : SingletonMonoBehavior<SceneControllerManager>
{
    [SerializeField] private InputActionAsset thirdPartyAsset;
    [SerializeField] private InputActionAsset myAsset;

    protected override void Awake()
    {
        base.Awake();
    
        // Debug what action maps are available
        Debug.Log("=== My Asset Action Maps ===");
        foreach (var map in myAsset.actionMaps)
        {
            Debug.Log($"Found action map: '{map.name}'");
        }
    
        Debug.Log("=== Third Party Asset Action Maps ===");
        foreach (var map in thirdPartyAsset.actionMaps)
        {
            Debug.Log($"Found action map: '{map.name}'");
        }

        // Verify assets are assigned
        if (myAsset == null)
            Debug.LogError("myAsset is not assigned!");
        if (thirdPartyAsset == null)
            Debug.LogError("thirdPartyAsset is not assigned!");

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log($"Scene loaded: {scene.name}");
        
        switch (scene.name)
        {
            case "Start Scene":
                SwitchToMyAsset();
                break;
            default:
                SwitchToThirdPartyAsset();
                break;
        }
    }

    public void SwitchToMyAsset()
    {
        Debug.Log("SwitchToMyAsset called");
        
        // Just disable and enable unconditionally
        thirdPartyAsset.Disable();
        Debug.Log("Disabled third party asset");
        
        myAsset.Enable();
        Debug.Log("Enabled my asset");

        Debug.Log("Switched to My Input Asset");
    }

    public void SwitchToThirdPartyAsset()
    {
        Debug.Log("SwitchToThirdPartyAsset called");
        
        // Just disable and enable unconditionally
        myAsset.Disable();
        Debug.Log("Disabled my asset");
        
        thirdPartyAsset.Enable();
        Debug.Log("Enabled third party asset");

        Debug.Log("Switched to Third Party Input Asset");
    }
}