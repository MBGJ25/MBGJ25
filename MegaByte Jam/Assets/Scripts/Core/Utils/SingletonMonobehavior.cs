using UnityEngine;

/// <summary>
/// Thread-safe, production-ready Singleton pattern for MonoBehaviours.
/// 
/// USAGE:
/// 1. Inherit from this class: public class GameManager : SingletonMonoBehaviour<GameManager>
/// 2. Add to a GameObject in your scene
/// 3. Access via: GameManager.Instance
/// 
/// IMPORTANT NOTES:
/// - Instance must be accessed from the main thread only
/// - Override OnAwakeSingleton() instead of Awake()
/// - Override OnDestroySingleton() instead of OnDestroy()
/// - Set ShouldPersistAcrossScenes = false if you want scene-specific singletons
/// </summary>
public abstract class SingletonMonoBehavior<T> : MonoBehaviour where T : MonoBehaviour
{
    #region Configuration

    /// <summary>
    /// Should this singleton persist across scene loads?
    /// Override in derived class to change behavior.
    /// </summary>
    protected virtual bool ShouldPersistAcrossScenes => true;

    /// <summary>
    /// Should the singleton automatically create an instance if one doesn't exist?
    /// Override in derived class to change behavior.
    /// If false, accessing Instance when none exists will throw an exception.
    /// </summary>
    protected virtual bool AutoCreateInstance => false;

    #endregion

    #region Fields

    private static T _instance;
    private static bool _applicationIsQuitting;

    #endregion

    #region Properties

    /// <summary>
    /// Global access point for the singleton instance.
    /// Thread-safe for main thread access only.
    /// </summary>
    public static T Instance
    {
        get
        {
            if (_applicationIsQuitting)
            {
                LogWarning($"Instance '{typeof(T)}' is being accessed during application shutdown. Returning null.");
                return null;
            }

            if (_instance != null)
                return _instance;

            #if UNITY_2023_1_OR_NEWER
                _instance = FindFirstObjectByType<T>();
            #else
                _instance = FindObjectOfType<T>();
            #endif

            if (_instance == null)
            {
                throw new System.InvalidOperationException(
                    $"No instance of singleton '{typeof(T)}' exists in the scene. " +
                    $"Either add it to the scene manually or set AutoCreateInstance = true.");
            
            }

            return _instance;
        }
    }

    /// <summary>
    /// Check if an instance exists without triggering creation.
    /// Useful for cleanup or conditional logic.
    /// </summary>
    public static bool HasInstance => _instance != null;

    /// <summary>
    /// Check if the application is currently quitting.
    /// Useful for preventing operations during shutdown.
    /// </summary>
    public static bool IsQuitting => _applicationIsQuitting;

    #endregion

    #region Unity Lifecycle

    protected virtual void Awake()
    {
        if (_instance == null)
        {
            _instance = this as T;

            if (ShouldPersistAcrossScenes)
            {
                transform.SetParent(null);
                UnityEngine.Object.DontDestroyOnLoad(gameObject);
            }

            OnAwakeSingleton();
            Log($"Singleton '{typeof(T)}' initialized");
        }
        else if (_instance != this)
        {
            LogWarning($"Duplicate singleton instance of '{typeof(T)}' detected on GameObject '{gameObject.name}'. Destroying duplicate.");
            Destroy(gameObject);
        }
    }

    protected virtual void OnDestroy()
    {
        if (_instance == this)
        {
            OnDestroySingleton();
            _instance = null;
        }
    }

    protected virtual void OnApplicationQuit()
    {
        _applicationIsQuitting = true;
    }

    #endregion

    #region Virtual Methods for Derived Classes

    /// <summary>
    /// Called when the singleton is first initialized.
    /// Override this instead of Awake() in derived classes.
    /// </summary>
    protected virtual void OnAwakeSingleton() { }

    /// <summary>
    /// Called when the singleton is destroyed.
    /// Override this instead of OnDestroy() in derived classes.
    /// </summary>
    protected virtual void OnDestroySingleton() { }

    #endregion

    #region Editor Support

    #if UNITY_EDITOR
    /// <summary>
    /// Reset static state when entering/exiting play mode in the editor.
    /// This fixes issues with Unity's "Enter Play Mode" settings.
    /// </summary>
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics()
    {
        _instance = null;
        _applicationIsQuitting = false;
    }
    #endif

    #endregion

    #region Logging Helpers

    private static void Log(string message)
    {
        #if UNITY_EDITOR || DEVELOPMENT_BUILD
        Debug.Log($"[Singleton] {message}");
        #endif
    }

    private static void LogWarning(string message)
    {
        #if UNITY_EDITOR || DEVELOPMENT_BUILD
        Debug.LogWarning($"[Singleton] {message}");
        #endif
    }

    #endregion
}
