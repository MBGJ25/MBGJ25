using UnityEngine;

// CS TODO WHEN BACK
// 1. Answer question on Claude thread and implement it's solution

public class PlayerController : MonoBehaviour
{
    #region Internal Values
    private Vector3 movement;
    private bool isRunning = false;
    private Rigidbody rb;
    #endregion

    #region Serialized Values
    [Header("Input Reader")]
    [SerializeField] private PlayerInputReader playerInputReader;

    [Header("Movement Settings")]
    [SerializeField] private int movementSpeed = 5;
    [SerializeField] private float runSpeedMultiplier = 1.75f;
    #endregion

    #region Lifecycle Methods
    private void Start()
    {
        rb = GetComponent<Rigidbody>();
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
            playerInputReader.OnMoveEvent += HandleMoveEvent;
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
        float targetSpeed = movementSpeed * (isRunning ? runSpeedMultiplier : 1f);
        Vector3 velocity = movement * targetSpeed * Time.fixedDeltaTime;
        rb.MovePosition(transform.position + velocity);

        if (movement.magnitude > 0.1f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(movement);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.fixedDeltaTime * 10f);
        }
    }
    #endregion
}
