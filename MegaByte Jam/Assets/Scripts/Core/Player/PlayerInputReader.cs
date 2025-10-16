using UnityEngine;
using UnityEngine.InputSystem;
using System;
using static PlayerControls;

[CreateAssetMenu(fileName = "Player Input Reader", menuName = "Core/Input/Player Input Reader")]
public class PlayerInputReader : ScriptableObject, IPlayerCharacterActions, IMenuNavigationActions
{
    private PlayerControls inputActions;
    private InputActionMap currentActionMap;

    #region Properties
    public PlayerControls.PlayerCharacterActions PlayerInputs => inputActions.PlayerCharacter;
    public PlayerControls.MenuNavigationActions MenuInputs => inputActions.MenuNavigation;
    #endregion

    #region Lifecycle Methods
    private void OnEnable() 
    {
        if (inputActions == null) {
            InitializeInputActions();
            // CS TODO: Modify this once we have multiple input maps
            SwitchToPlayerCharacterControls();
        }
    }

    private void OnDisable() 
    {
        DisableAllInputs();
        if (Application.isPlaying) 
        {
            inputActions?.Dispose();
        }
    }
    #endregion

    #region Initialization
    private void InitializeInputActions()
    {
        inputActions = new PlayerControls();
        inputActions.Enable();
        PlayerInputs.SetCallbacks(this);
        MenuInputs.SetCallbacks(this);
    }
    #endregion

    #region Action Map Management
    public void SwitchToPlayerCharacterControls ()
    {
        #if UNITY_EDITOR
        Debug.Log("Switching to default Player Character controls");
        #endif
        SwitchActionMap(inputActions.PlayerCharacter);
    }

    public void SwitchToMenuNavigationControls()
    {
        #if UNITY_EDITOR
        Debug.Log("Switching to Menu Navigation controls");
        #endif
        SwitchActionMap(inputActions.MenuNavigation);
    }

    private void SwitchActionMap(InputActionMap newActionMap)
    {
        DisableAllInputs();
        currentActionMap = newActionMap;
        currentActionMap?.Enable();
    }

    private void DisableAllInputs()
    {
        inputActions?.PlayerCharacter.Disable();
        inputActions?.MenuNavigation.Disable();
    }
    #endregion

    #region Player Character Input Actions
    public event Action<Vector2> OnMoveEvent;
    public event Action<Boolean> OnRunEvent;
    
    public void OnMove(InputAction.CallbackContext context)
    {
        Vector2 newMovementVector = context.ReadValue<Vector2>();
        newMovementVector.Normalize();
        OnMoveEvent?.Invoke(newMovementVector);
    }

    public void OnRun(InputAction.CallbackContext context)
    {
        OnRunEvent?.Invoke(context.phase == InputActionPhase.Performed);
    }
    #endregion

    #region Menu Navigation Input Actions
    public event Action OnConfirmEvent;
    public event Action OnBackEvent;
    public event Action<Vector2> OnNavigateEvent;


    public void OnConfirm(InputAction.CallbackContext context) 
    {
        if (context.performed)
            OnConfirmEvent?.Invoke();
    }

    public void OnBack(InputAction.CallbackContext context)
    {
        if (context.performed)
            OnBackEvent?.Invoke();
    }

    public void OnNavigate(InputAction.CallbackContext context)
    {
        if (context.performed) 
        {
            Vector2 navigationValue = context.ReadValue<Vector2>();
            OnNavigateEvent?.Invoke(navigationValue);
        }
    }
    #endregion
}