using UnityEditorInternal.VersionControl;
using UnityEngine;
using UnityEngine.Events;
#if ENABLE_INPUT_SYSTEM
    using UnityEngine.InputSystem;
#endif


namespace PhysicsCharacterController
{
    #region Abstract for both input systems

    public interface IInputBackend
    {
        Vector2 GetMovement();
        Vector2 GetCameraDelta();
        bool GetJumpDown();
        bool GetJumpUp();
        bool GetSprint();
        bool GetCrouch();

        bool IsMouseKeyboard();

        void Enable();
        void Disable();

        bool ToggleCameraPressed();
        bool ToggleDebugPressed();
    }


    // -----------------------
    // New Input System
    // -----------------------
    #if ENABLE_INPUT_SYSTEM
    
    public class NewInputBackend : IInputBackend
    {
        private MovementActions actions;
        private InputControl lastControl;


        public NewInputBackend()
        {
            actions = new MovementActions();

            // Subscribe to detect device change
            actions.Gameplay.Movement.performed += ctx => lastControl = ctx.control;
            actions.Gameplay.Camera.performed += ctx => lastControl = ctx.control;
            actions.Gameplay.Jump.started += ctx => lastControl = ctx.control;
            actions.Gameplay.Sprint.started += ctx => lastControl = ctx.control;
            actions.Gameplay.Crouch.started += ctx => lastControl = ctx.control;

            actions.Enable();
        }


        public Vector2 GetMovement() => actions.Gameplay.Movement.ReadValue<Vector2>();
        public Vector2 GetCameraDelta() => actions.Gameplay.Camera.ReadValue<Vector2>();


        public bool GetJumpDown() => actions.Gameplay.Jump.WasPressedThisFrame();
        public bool GetJumpUp() => actions.Gameplay.Jump.WasReleasedThisFrame();
        public bool GetSprint() => actions.Gameplay.Sprint.IsPressed();
        public bool GetCrouch() => actions.Gameplay.Crouch.IsPressed();


        public bool IsMouseKeyboard()
        {
            if (lastControl == null) return true; // default
            return lastControl.device is Keyboard || lastControl.device is Mouse;
        }


        public void Enable() => actions.Enable();
        public void Disable() => actions.Disable();


        public bool ToggleCameraPressed() => Keyboard.current.mKey.wasPressedThisFrame;
        public bool ToggleDebugPressed() => Keyboard.current.nKey.wasPressedThisFrame;
    }

    #endif


    // -----------------------
    // Old Input System
    // -----------------------
    #if ENABLE_LEGACY_INPUT_MANAGER

    public class OldInputBackend : IInputBackend
    {
        private bool lastWasMouseKeyboard = true;


        public Vector2 GetMovement()
        {
            Vector2 value = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
            if (Mathf.Abs(value.x) > 0.01f || Mathf.Abs(value.y) > 0.01f)
                lastWasMouseKeyboard = Input.GetJoystickNames().Length == 0;
            return value;
        }


        public Vector2 GetCameraDelta()
        {
            Vector2 value = new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));
            if (Mathf.Abs(value.x) > 0.01f || Mathf.Abs(value.y) > 0.01f)
                lastWasMouseKeyboard = true;
            return value;
        }


        public bool GetJumpDown() => Input.GetButtonDown("Jump");
        public bool GetJumpUp() => Input.GetButtonUp("Jump");
        public bool GetSprint() => Input.GetButton("Fire3");
        public bool GetCrouch() => Input.GetButton("Fire1");

        public bool IsMouseKeyboard() => lastWasMouseKeyboard;


        public void Enable() { }
        public void Disable() { }


        public bool ToggleCameraPressed() => Input.GetKeyDown(KeyCode.M);
        public bool ToggleDebugPressed() => Input.GetKeyDown(KeyCode.N);
    }

    #endif

    #endregion


    public class InputReader : MonoBehaviour
    {
        [Header("Events")]
        public UnityEvent changedInputToMouseAndKeyboard;
        [Space(15)]

        public UnityEvent changedInputToGamepad;
        [Space(15)]

        public UnityEvent toggledCamera;
        [Space(15)]

        public UnityEvent toggledDebug;
        [Space(15)]

        [Header("Enable inputs")]
        public bool enableJump = true;
        public bool enableCrouch = true;
        public bool enableSprint = true;

        [HideInInspector] public Vector2 axisInput;
        [HideInInspector] public Vector2 cameraDelta;
        [HideInInspector] public bool jump;
        [HideInInspector] public bool jumpHold;
        [HideInInspector] public bool sprint;
        [HideInInspector] public bool crouch;


        private IInputBackend backend;
        private bool lastWasMouseKeyboard = true;
        private bool jumpBuffered;


        /**/


        #region Setup

        private void Awake()
        {
            #if ENABLE_INPUT_SYSTEM
                backend = new NewInputBackend();   // new system takes priority if both enabled
            #elif ENABLE_LEGACY_INPUT_MANAGER
                backend = new OldInputBackend();
            #else
                Debug.LogError("No input system enabled in Player Settings.");
            #endif
        }


        private void OnEnable() => backend?.Enable();
        private void OnDisable() => backend?.Disable();

        #endregion


        #region HandleInputs

        private void Update()
        {
            // Movement
            axisInput = backend.GetMovement();

            // Camera
            cameraDelta = backend.GetCameraDelta();

            // Jump
            if (enableJump)
            {
                if (backend.GetJumpDown()) jumpBuffered = true;
                if (backend.GetJumpUp()) jumpHold = false;
                if (jumpBuffered) jumpHold = true;
            }

            // Sprint and Crouch
            if (enableSprint) sprint = backend.GetSprint();
            if (enableCrouch) crouch = backend.GetCrouch();


            // Toggle camera & Debug
            if (backend.ToggleCameraPressed()) toggledCamera.Invoke();
            if (backend.ToggleDebugPressed()) toggledDebug.Invoke();

            // Detect new input
            DetectDeviceChange();
        }


        private void FixedUpdate()
        {
            // Expose buffered jump for this physics step
            jump = jumpBuffered;
            jumpBuffered = false;
        }


        private void DetectDeviceChange()
        {
            bool current = backend.IsMouseKeyboard();
            if (current != lastWasMouseKeyboard)
            {
                if (current) changedInputToMouseAndKeyboard.Invoke();
                else changedInputToGamepad.Invoke();
                lastWasMouseKeyboard = current;

                Debug.Log("Input device was changed");
            }
        }

        #endregion
    }
}