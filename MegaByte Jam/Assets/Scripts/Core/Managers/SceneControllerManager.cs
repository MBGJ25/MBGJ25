using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class SceneControllerManager : SingletonMonoBehavior<SceneControllerManager>
{
    [SerializeField] private InputActionAsset thirdPartyAsset;
    [SerializeField] private InputActionAsset myAsset;

    private InputActionMap myMap;
    private InputActionMap thirdPartyMap;

    protected override void Awake()
    {
        base.Awake();
        myMap = myAsset.FindActionMap("PlayerControls");
        thirdPartyMap = thirdPartyAsset.FindActionMap("MovementActions");

        // Subscribe to scene loaded event
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDestroy()
    {
        // Always unsubscribe to avoid memory leaks
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Switch based on the scene name
        switch (scene.name)
        {
            case "Start Scene":
                SwitchToMyMap();
                break;
            default:
                SwitchToThirdPartyMap();
                break;
        }
    }

    public void SwitchToMyMap()
    {
        if (thirdPartyMap is { enabled: true })
            thirdPartyMap.Disable();

        if (myMap != null && !myMap.enabled)
            myMap.Enable();

        Debug.Log("Switched to My Input Map");
    }

    public void SwitchToThirdPartyMap()
    {
        if (myMap is { enabled: true })
            myMap.Disable();

        if (thirdPartyMap is { enabled: false })
            thirdPartyMap.Enable();

        Debug.Log("Switched to Third Party Input Map");
    }
}
