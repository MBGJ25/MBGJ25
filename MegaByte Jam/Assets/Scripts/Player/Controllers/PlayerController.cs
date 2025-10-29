using UnityEngine;

public class PlayerController : MonoBehaviour
{
    #region Internal Values
    private Vector3 movement;
    private bool isRunning = false;
    private Rigidbody rb;
    #endregion

    #region Serialized Values
    [Header("External References")]
    [SerializeField] private PlayerInputReader playerInputReader;
    [SerializeField] private Transform cameraTransform;

    [Header("Movement Settings")]
    [SerializeField] private int movementSpeed = 5;
    [SerializeField] private float runSpeedMultiplier = 1.75f;
    [SerializeField] private float rotationSpeed = 15f;
    #endregion

    #region Lifecycle Methods
    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        
        if (cameraTransform == null)
        {
            Debug.LogError("No cinemachine brain transform attached to PlayerController.cs");
        }
    }

    private void Update()
    {
        // CS TODO: Add animator updates and such here   
    }

    private void FixedUpdate()
    {
        ApplyMovement();
    }
    private void OnEnable()
    {
        if (playerInputReader != null)
        {
            playerInputReader.SwitchToPlayerCharacterControls();
            playerInputReader.OnMoveEvent += HandleMoveEvent;
            playerInputReader.OnRunEvent += HandleRunEvent;
        }
        else
        {
            Debug.LogError("Player input reader null in PlayerController.cs");
        }
    }

    private void OnDisable()
    {
        if (playerInputReader != null)
        {
            playerInputReader.OnMoveEvent -= HandleMoveEvent;
            playerInputReader.OnRunEvent -= HandleRunEvent;
        }
    }
    #endregion

    #region Movement Logic
    private void HandleMoveEvent(Vector2 movementInput)
    {
        movement = new Vector3(movementInput.x, 0, movementInput.y);
    }

    private void ApplyMovement()
    {
        if (cameraTransform == null) return;

        // Convert input to camera-relative direction
        Vector3 cameraForward = cameraTransform.forward;
        Vector3 cameraRight = cameraTransform.right;

        // Flatten camera vectors on Y axis
        cameraForward.y = 0;
        cameraRight.y = 0;
        cameraForward.Normalize();
        cameraRight.Normalize();

        // Calculate movement direction relative to camera
        Vector3 moveDirection = (cameraRight * movement.x + cameraForward * movement.z).normalized;

        // Apply movement
        float targetSpeed = movementSpeed * (isRunning ? runSpeedMultiplier : 1f);
        Vector3 targetVelocity = moveDirection * targetSpeed;
        rb.velocity = new Vector3(targetVelocity.x, rb.velocity.y, targetVelocity.z);

        // Rotate character to face movement direction
        if (moveDirection.magnitude > 0.1f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
            rb.MoveRotation(Quaternion.Slerp(rb.rotation, targetRotation, Time.fixedDeltaTime * rotationSpeed));
        }
    }

    private void HandleRunEvent(bool isPlayerRunning)
    {
        isRunning = isPlayerRunning;
    }
    #endregion
}
