using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class StartMenuController : MonoBehaviour
{
    #region Serialized Values
    [Header("Input")]
    [SerializeField] private PlayerInputReader playerInputReader;

    [Header("Menu Configuration")]
    [SerializeField] private List<GameObject> menuButtons = new List<GameObject>();
    [SerializeField] private List<MenuAction> menuActions = new List<MenuAction>();

    [Header("Visual Feedback")]
    [SerializeField] private Color selectedColor = Color.yellow;
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private float selectedScale = 1.2f;
    [SerializeField] private float normalScale = 1f;
    [SerializeField] private float navigationCooldown = 0.2f;
    #endregion

    #region Internal Values
    private int currentButtonIndex = 0;
    private float lastNavigationTime = 0f;
    #endregion

    #region Lifecycle Methods
    private void Start()
    {
        if (menuButtons.Count == 0)
        {
            Debug.LogError("No menu buttons assigned!");
            return;
        }

        UpdateButtonVisuals();
    }

    private void OnEnable()
    {
        if (playerInputReader != null)
        {
            playerInputReader.SwitchToMenuNavigationControls();
            playerInputReader.OnNavigateEvent += HandleMenuNavigation;
            playerInputReader.OnConfirmEvent += HandleConfirm;
            playerInputReader.OnBackEvent += HandleBack;
        }
    }

    private void OnDisable()
    {
        if (playerInputReader != null)
        {
            playerInputReader.OnNavigateEvent -= HandleMenuNavigation;
            playerInputReader.OnConfirmEvent -= HandleConfirm;
            playerInputReader.OnBackEvent -= HandleBack;
        }
    }
    #endregion

    #region Input Handlers
    private void HandleMenuNavigation(Vector2 inputNavigationValue)
    {
        if (menuButtons.Count <= 1) return;

        // Cooldown to prevent rapid navigation
        if (Time.time - lastNavigationTime < navigationCooldown) return;

        int previousIndex = currentButtonIndex;

        // Vertical navigation (up/down)
        if (inputNavigationValue.y > 0.5f) // Up
        {
            currentButtonIndex--;
            if (currentButtonIndex < 0)
                currentButtonIndex = menuButtons.Count - 1; // Wrap to bottom

            lastNavigationTime = Time.time;
        }
        else if (inputNavigationValue.y < -0.5f) // Down
        {
            currentButtonIndex++;
            if (currentButtonIndex >= menuButtons.Count)
                currentButtonIndex = 0; // Wrap to top

            lastNavigationTime = Time.time;
        }

        if (previousIndex != currentButtonIndex)
        {
            UpdateButtonVisuals();
            // CS TODO: Add sound effect when ready
        }
    }

    private void HandleConfirm()
    {
        if (menuButtons.Count == 0 || currentButtonIndex >= menuActions.Count)
        {
            Debug.LogWarning("No valid action for current button!");
            return;
        }

        MenuAction action = menuActions[currentButtonIndex];

        switch (action.actionType)
        {
            case MenuActionType.LoadScene:
                if (!string.IsNullOrEmpty(action.sceneName))
                {
                    SceneTransitionManager.Instance.LoadScene(action.sceneName);
                }
                else
                {
                    Debug.LogError("Scene name not set for LoadScene action!");
                }
                break;

            case MenuActionType.QuitGame:
                QuitGame();
                break;

            case MenuActionType.Custom:
                // For future expansion - could invoke UnityEvents here
                Debug.Log("Custom action triggered!");
                break;
        }
    }

    private void HandleBack()
    {
        QuitGame();
    }
    #endregion

    #region Visual Feedback
    private void UpdateButtonVisuals()
    {
        for (int i = 0; i < menuButtons.Count; i++)
        {
            if (menuButtons[i] == null) continue;

            bool isSelected = (i == currentButtonIndex);
            Image buttonImage = menuButtons[i].GetComponent<Image>();

            if (buttonImage != null)
            {
                buttonImage.color = isSelected ? selectedColor : normalColor;
            }
            else
            {
                Debug.LogWarning($"Button {menuButtons[i].name} has no Image component for background color!");
            }

            menuButtons[i].transform.localScale = Vector3.one * (isSelected ? selectedScale : normalScale);
        }
    }
    #endregion

    #region Utility Methods
    private void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
    }

    /// <summary>
    /// Programmatically select a specific button index
    /// </summary>
    public void SelectButton(int index)
    {
        if (index >= 0 && index < menuButtons.Count)
        {
            currentButtonIndex = index;
            UpdateButtonVisuals();
        }
    }
    #endregion
}

#region Menu Action System
[System.Serializable]
public class MenuAction
{
    public MenuActionType actionType;
    public string sceneName; // Used when actionType is LoadScene
}

public enum MenuActionType
{
    LoadScene,
    QuitGame,
    Custom
}
#endregion