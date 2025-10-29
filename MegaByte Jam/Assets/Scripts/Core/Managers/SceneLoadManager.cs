using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SceneTransitionManager : SingletonMonoBehavior<SceneTransitionManager>
{
    [Header("Transition Settings")]
    [SerializeField] private float fadeOutDuration = 0.5f;
    [SerializeField] private float fadeInDuration = 0.5f;
    [SerializeField] private float minimumLoadTime = 0.5f;
    [SerializeField] private float fadeInDelay = 0.1f;
    [SerializeField] private Color fadeColor = Color.black;

    [Header("UI References")]
    [SerializeField] private Canvas transitionCanvas;
    [SerializeField] private Image fadeImage;

    // Events
    public event Action<string> OnTransitionStart;
    public event Action OnSceneLoaded;
    public event Action OnTransitionComplete;

    // State
    private bool isTransitioning = false;
    private string currentSceneName;

    protected override void Awake()
    {
        base.Awake();

        if (transitionCanvas != null)
        {
            transitionCanvas.sortingOrder = 999; // Always on top
            DontDestroyOnLoad(transitionCanvas.gameObject);
        }

        if (fadeImage != null)
        {
            fadeImage.color = new Color(fadeColor.r, fadeColor.g, fadeColor.b, 0f);
        }

        currentSceneName = SceneManager.GetActiveScene().name;
    }

    /// <summary>
    /// Transition to a scene by name
    /// </summary>
    public void LoadScene(string sceneName)
    {
        if (isTransitioning)
        {
            Debug.LogWarning("Scene transition already in progress!");
            return;
        }

        StartCoroutine(TransitionToScene(sceneName, LoadSceneMode.Single));
    }

    /// <summary>
    /// Transition to a scene by build index
    /// </summary>
    public void LoadScene(int sceneIndex)
    {
        if (isTransitioning)
        {
            Debug.LogWarning("Scene transition already in progress!");
            return;
        }

        if (sceneIndex < 0 || sceneIndex >= SceneManager.sceneCountInBuildSettings)
        {
            Debug.LogError($"Invalid scene index: {sceneIndex}");
            return;
        }

        string sceneName = System.IO.Path.GetFileNameWithoutExtension(
            SceneUtility.GetScenePathByBuildIndex(sceneIndex)
        );
        StartCoroutine(TransitionToScene(sceneName, LoadSceneMode.Single));
    }

    /// <summary>
    /// Reload the current scene
    /// </summary>
    public void ReloadCurrentScene()
    {
        LoadScene(currentSceneName);
    }

    /// <summary>
    /// Load a scene additively (for overlays, multi-scene setups)
    /// NOTE: Not currently used in our project!
    /// </summary>
    public void LoadSceneAdditive(string sceneName)
    {
        if (isTransitioning)
        {
            Debug.LogWarning("Scene transition already in progress!");
            return;
        }

        StartCoroutine(TransitionToScene(sceneName, LoadSceneMode.Additive));
    }

    private IEnumerator TransitionToScene(string sceneName, LoadSceneMode mode)
    {
        isTransitioning = true;
        OnTransitionStart?.Invoke(sceneName);
        yield return StartCoroutine(Fade(1f, fadeOutDuration));
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName, mode);
        asyncLoad.allowSceneActivation = false;

        while (asyncLoad.progress < 0.9f)
        {
            yield return null;
        }

        yield return new WaitForSeconds(minimumLoadTime);
        asyncLoad.allowSceneActivation = true;
        yield return asyncLoad;

        if (mode == LoadSceneMode.Single)
        {
            currentSceneName = sceneName;
        }

        OnSceneLoaded?.Invoke();

        yield return new WaitForSeconds(fadeInDelay);
        yield return StartCoroutine(Fade(0f, fadeInDuration));
        isTransitioning = false;
        OnTransitionComplete?.Invoke();
    }

    private IEnumerator Fade(float targetAlpha, float duration)
    {
        if (fadeImage == null) yield break;

        float startAlpha = fadeImage.color.a;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(startAlpha, targetAlpha, elapsed / duration);
            fadeImage.color = new Color(fadeColor.r, fadeColor.g, fadeColor.b, alpha);
            yield return null;
        }

        fadeImage.color = new Color(fadeColor.r, fadeColor.g, fadeColor.b, targetAlpha);
    }

    /// <summary>
    /// Check if a transition is currently happening
    /// </summary>
    public bool IsTransitioning => isTransitioning;

    /// <summary>
    /// Get the current scene name
    /// </summary>
    public string CurrentSceneName => currentSceneName;
}