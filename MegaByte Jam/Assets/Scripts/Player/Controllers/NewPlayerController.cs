using PhysicsCharacterController;
using UnityEngine;


[RequireComponent(typeof(CapsuleCollider))]
[RequireComponent(typeof(Rigidbody))]
public class NewPlayerController : MonoBehaviour
{
    #region Configurable Values
    [HideInInspector] public bool debug = true;
    [Header("Movement Values")] 
    public float movementSpeed = 14f;
    [Range(0f, 1f)]
    public float crouchSpeedMultiplier = 0.248f;
    [Range(0.01f, 0.99f)]
    public float movementThrashold = 0.01f;
    [Space(10)]
    public float dampSpeedUp = 0.2f;
    public float dampSpeedDown = 0.1f;
    

    [Space(10)]
    [Header("Jump and Gravity Values")]
    public float jumpVelocity = 20f;
    public float fallMultiplier = 1.7f;
    public float holdJumpMultiplier = 5f;
    [Range(0f, 1f)]
    public float frictionAgainstFloor = 0.3f;
    [Range(0.01f, 0.99f)]
    public float frictionAgainstWall = 0.839f;
    [Space(10)]
    public bool canLongJump = true;
    #endregion
    
    #region External References

    [Header("External References")] 
    [SerializeField] private PlayerInputReader playerInputReader;
    public GameObject characterCamera;
    public GameObject characterModel;
    public float characterModelRotationSmooth = 0.1f;
    [Space(10)]

    public GameObject meshCharacter;
    public GameObject meshCharacterCrouch;
    public Transform headPoint;
    [Space(10)]
    #endregion
    
    #region Internal Values
    private Vector3 forward;
    private Vector3 globalForward;
    private Vector3 reactionForward;
    private Vector3 down;
    private Vector3 globalDown;
    private Vector3 reactionGlobalDown;

    private float currentSurfaceAngle;
    private bool currentLockOnSlope;

    private Vector3 wallNormal;
    private Vector3 groundNormal;
    private Vector3 prevGroundNormal;
    private bool prevGrounded;

    private float coyoteJumpMultiplier = 1f;

    private bool isGrounded = false;
    private bool isTouchingSlope = false;
    private bool isTouchingStep = false;
    private bool isTouchingWall = false;
    private bool isJumping = false;
    private bool isCrouch = false;

    private Vector2 axisInput;
    private bool jump;
    private bool jumpHold;
    private bool sprint;
    private bool crouch;

    [HideInInspector]
    public float targetAngle;
    private Rigidbody rigidbody;
    private CapsuleCollider collider;
    private float originalColliderHeight;

    private Vector3 currVelocity = Vector3.zero;
    private float turnSmoothVelocity;
    private bool lockRotation = false;
    private bool lockToCamera = false;
    #endregion
}
