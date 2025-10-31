using UnityEngine;
using UnityEngine.Events;


namespace PhysicsCharacterController
{
    [RequireComponent(typeof(CapsuleCollider))]
    [RequireComponent(typeof(Rigidbody))]
    public class CharacterManager : MonoBehaviour
    {
        [Header("Movement specifics")]
        [SerializeField] LayerMask groundMask;
        public float movementSpeed = 14f;
        [Range(0f, 1f)]
        public float crouchSpeedMultiplier = 0.248f;
        [Range(0.01f, 0.99f)]
        public float movementThrashold = 0.01f;
        [Space(10)]
        public float dampSpeedUp = 0.2f;
        public float dampSpeedDown = 0.1f;
        public bool airCrouchCompleteStop = true;


        [Header("Jump and gravity specifics")]
        public float jumpVelocity = 20f;
        public float fallMultiplier = 1.7f;
        public float holdJumpMultiplier = 5f;
        [Range(0f, 1f)]
        public float frictionAgainstFloor = 0.3f;
        [Range(0.01f, 0.99f)]
        public float frictionAgainstWall = 0.839f;
        [Space(10)]
        public bool canLongJump = true;
        public float airCrouchGravityMultiplier = 24f;


        [Header("Slope and step specifics")]
        public float groundCheckerThrashold = 0.1f;
        public float slopeCheckerThrashold = 0.51f;
        public float stepCheckerThrashold = 0.6f;
        [Space(10)]
        [Range(1f, 89f)]
        public float maxClimbableSlopeAngle = 53.6f;
        public float maxStepHeight = 0.74f;
        [Space(10)]
        public AnimationCurve speedMultiplierOnAngle = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
        [Range(0.01f, 1f)]
        public float canSlideMultiplierCurve = 0.061f;
        [Range(0.01f, 1f)]
        public float cantSlideMultiplierCurve = 0.039f;
        [Range(0.01f, 1f)]
        public float climbingStairsMultiplierCurve = 0.637f;
        [Space(10)]
        public float gravityMultiplier = 6f;
        public float gravityMultiplyerOnSlideChange = 3f;
        public float gravityMultiplierIfUnclimbableSlope = 30f;
        [Space(10)]
        public bool lockOnSlope = false;


        [Header("Wall slide specifics")]
        public float wallCheckerThrashold = 0.8f;
        public float hightWallCheckerChecker = 0.5f;
        [Space(10)]
        public float jumpFromWallMultiplier = 30f;
        public float multiplierVerticalLeap = 1f;


        [Header("Sprint and crouch specifics")]
        public float sprintSpeed = 20f;
        public float crouchHeightMultiplier = 0.5f;
        public Vector3 POV_normalHeadHeight = new Vector3(0f, 0.5f, -0.1f);
        public Vector3 POV_crouchHeadHeight = new Vector3(0f, -0.1f, -0.1f);


        [Header("References")]
        public GameObject characterCamera;
        public GameObject characterModel;
        public float characterModelRotationSmooth = 0.1f;
        [Space(10)]
        public GameObject meshCharacter;
        public GameObject meshCharacterCrouch;
        public Transform headPoint;
        [Space(10)]
        public InputReader input;
        [Space(10)]
        [HideInInspector]
        public bool debug = true;


        [Header("Events")]
        [SerializeField] UnityEvent OnJump;
        [Space(15)]
        public float minimumVerticalSpeedToLandEvent;
        [SerializeField] UnityEvent OnLand;
        [Space(15)]
        public float minimumHorizontalSpeedToFastEvent;
        [SerializeField] UnityEvent OnFast;
        [Space(15)]
        [SerializeField] UnityEvent OnWallSlide;
        [Space(15)]
        [SerializeField] UnityEvent OnSprint;
        [Space(15)]
        [SerializeField] UnityEvent OnCrouch;
        [Space(15)]
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
        private float originalGravityMultiplier;
        private float originalCrouchSpeedMultiplier;

        private Vector3 currVelocity = Vector3.zero;
        private float turnSmoothVelocity;
        private bool lockRotation = false;
        private bool lockToCamera = false;


        /**/

        #region Grind Values

        private bool isGrinding = false;
        private GrindRail currentRail = null;
        private float grindPositionT = 0f;
        private Vector3 grindDirection = Vector3.zero;
        private float grindHeightOffset = 0.5f; // Height above rail

        #endregion

        private void Awake()
        {
            rigidbody = this.GetComponent<Rigidbody>();
            collider = this.GetComponent<CapsuleCollider>();
            originalColliderHeight = collider.height;
            originalGravityMultiplier = gravityMultiplier;
            originalCrouchSpeedMultiplier = crouchSpeedMultiplier;

            SetFriction(frictionAgainstFloor, true);
            currentLockOnSlope = lockOnSlope;
        }


        private void Update()
        {
            //input
            axisInput = input.axisInput;
            jump = input.jump;
            jumpHold = input.jumpHold;
            sprint = input.sprint;
            crouch = input.crouch;
        }


        private void FixedUpdate()
        {
            // Check if grinding - skip ALL normal movement and gravity
            if (isGrinding)
            {
                UpdateGrinding();
                UpdateEvents(); // Keep events running
                return; // Exit early - skip everything below
            }

            //local vectors
            CheckGrounded();
            CheckStep();
            CheckWall();
            CheckSlopeAndDirections();

            //movement
            MoveCrouch();
            MoveWalk();

            if (!lockToCamera) MoveRotation();
            else ForceRotation();

            MoveJump();

            //gravity
            ApplyGravity();

            //events
            UpdateEvents();
        }
        private void OnTriggerEnter(Collider other)
        {
            if (isGrinding) return;

            GrindRail rail = other.GetComponent<GrindRail>();
            if (rail != null)
            {
                // Check if conditions are met to start grinding
                if (rail.CanStartGrinding(rigidbody.velocity))
                {
                    StartGrinding(rail);
                }
            }
        }


        #region Checks

        private void CheckGrounded()
        {
            prevGrounded = isGrounded;
            isGrounded = Physics.CheckSphere(transform.position - new Vector3(0, originalColliderHeight / 2f, 0), groundCheckerThrashold, groundMask);
        }


        private void CheckStep()
        {
            bool tmpStep = false;
            Vector3 bottomStepPos = transform.position - new Vector3(0f, originalColliderHeight / 2f, 0f) + new Vector3(0f, 0.05f, 0f);

            RaycastHit stepLowerHit;
            if (Physics.Raycast(bottomStepPos, globalForward, out stepLowerHit, stepCheckerThrashold, groundMask))
            {
                RaycastHit stepUpperHit;
                if (RoundValue(stepLowerHit.normal.y) == 0 && !Physics.Raycast(bottomStepPos + new Vector3(0f, maxStepHeight, 0f), globalForward, out stepUpperHit, stepCheckerThrashold + 0.05f, groundMask))
                {
                    //rigidbody.position -= new Vector3(0f, -stepSmooth, 0f);
                    tmpStep = true;
                }
            }

            RaycastHit stepLowerHit45;
            if (Physics.Raycast(bottomStepPos, Quaternion.AngleAxis(45, transform.up) * globalForward, out stepLowerHit45, stepCheckerThrashold, groundMask))
            {
                RaycastHit stepUpperHit45;
                if (RoundValue(stepLowerHit45.normal.y) == 0 && !Physics.Raycast(bottomStepPos + new Vector3(0f, maxStepHeight, 0f), Quaternion.AngleAxis(45, Vector3.up) * globalForward, out stepUpperHit45, stepCheckerThrashold + 0.05f, groundMask))
                {
                    //rigidbody.position -= new Vector3(0f, -stepSmooth, 0f);
                    tmpStep = true;
                }
            }

            RaycastHit stepLowerHitMinus45;
            if (Physics.Raycast(bottomStepPos, Quaternion.AngleAxis(-45, transform.up) * globalForward, out stepLowerHitMinus45, stepCheckerThrashold, groundMask))
            {
                RaycastHit stepUpperHitMinus45;
                if (RoundValue(stepLowerHitMinus45.normal.y) == 0 && !Physics.Raycast(bottomStepPos + new Vector3(0f, maxStepHeight, 0f), Quaternion.AngleAxis(-45, Vector3.up) * globalForward, out stepUpperHitMinus45, stepCheckerThrashold + 0.05f, groundMask))
                {
                    //rigidbody.position -= new Vector3(0f, -stepSmooth, 0f);
                    tmpStep = true;
                }
            }

            isTouchingStep = tmpStep;
        }


        private void CheckWall()
        {
            bool tmpWall = false;
            Vector3 tmpWallNormal = Vector3.zero;
            Vector3 topWallPos = new Vector3(transform.position.x, transform.position.y + hightWallCheckerChecker, transform.position.z);

            RaycastHit wallHit;
            if (Physics.Raycast(topWallPos, globalForward, out wallHit, wallCheckerThrashold, groundMask))
            {
                tmpWallNormal = wallHit.normal;
                tmpWall = true;
            }
            else if (Physics.Raycast(topWallPos, Quaternion.AngleAxis(45, transform.up) * globalForward, out wallHit, wallCheckerThrashold, groundMask))
            {
                tmpWallNormal = wallHit.normal;
                tmpWall = true;
            }
            else if (Physics.Raycast(topWallPos, Quaternion.AngleAxis(90, transform.up) * globalForward, out wallHit, wallCheckerThrashold, groundMask))
            {
                tmpWallNormal = wallHit.normal;
                tmpWall = true;
            }
            else if (Physics.Raycast(topWallPos, Quaternion.AngleAxis(135, transform.up) * globalForward, out wallHit, wallCheckerThrashold, groundMask))
            {
                tmpWallNormal = wallHit.normal;
                tmpWall = true;
            }
            else if (Physics.Raycast(topWallPos, Quaternion.AngleAxis(180, transform.up) * globalForward, out wallHit, wallCheckerThrashold, groundMask))
            {
                tmpWallNormal = wallHit.normal;
                tmpWall = true;
            }
            else if (Physics.Raycast(topWallPos, Quaternion.AngleAxis(225, transform.up) * globalForward, out wallHit, wallCheckerThrashold, groundMask))
            {
                tmpWallNormal = wallHit.normal;
                tmpWall = true;
            }
            else if (Physics.Raycast(topWallPos, Quaternion.AngleAxis(270, transform.up) * globalForward, out wallHit, wallCheckerThrashold, groundMask))
            {
                tmpWallNormal = wallHit.normal;
                tmpWall = true;
            }
            else if (Physics.Raycast(topWallPos, Quaternion.AngleAxis(315, transform.up) * globalForward, out wallHit, wallCheckerThrashold, groundMask))
            {
                tmpWallNormal = wallHit.normal;
                tmpWall = true;
            }

            isTouchingWall = tmpWall;
            wallNormal = tmpWallNormal;
        }


        private void CheckSlopeAndDirections()
        {
            prevGroundNormal = groundNormal;

            RaycastHit slopeHit;
            if (Physics.SphereCast(transform.position, slopeCheckerThrashold, Vector3.down, out slopeHit, originalColliderHeight / 2f + 0.5f, groundMask))
            {
                groundNormal = slopeHit.normal;

                if (slopeHit.normal.y == 1)
                {

                    forward = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward;
                    globalForward = forward;
                    reactionForward = forward;

                    if (!isCrouch)
                    {
                        SetFriction(frictionAgainstFloor, true);
                    }
                    currentLockOnSlope = lockOnSlope;

                    currentSurfaceAngle = 0f;
                    isTouchingSlope = false;

                }
                else
                {
                    //set forward
                    Vector3 tmpGlobalForward = transform.forward.normalized;
                    Vector3 tmpForward = new Vector3(tmpGlobalForward.x, Vector3.ProjectOnPlane(transform.forward.normalized, slopeHit.normal).normalized.y, tmpGlobalForward.z);
                    Vector3 tmpReactionForward = new Vector3(tmpForward.x, tmpGlobalForward.y - tmpForward.y, tmpForward.z);

                    if (currentSurfaceAngle <= maxClimbableSlopeAngle && !isTouchingStep)
                    {
                        //set forward
                        forward = tmpForward * ((speedMultiplierOnAngle.Evaluate(currentSurfaceAngle / 90f) * canSlideMultiplierCurve) + 1f);
                        globalForward = tmpGlobalForward * ((speedMultiplierOnAngle.Evaluate(currentSurfaceAngle / 90f) * canSlideMultiplierCurve) + 1f);
                        reactionForward = tmpReactionForward * ((speedMultiplierOnAngle.Evaluate(currentSurfaceAngle / 90f) * canSlideMultiplierCurve) + 1f);

                        if (!isCrouch)
                        {
                            SetFriction(frictionAgainstFloor, true);
                        }
                        currentLockOnSlope = lockOnSlope;
                    }
                    else if (isTouchingStep)
                    {
                        //set forward
                        forward = tmpForward * ((speedMultiplierOnAngle.Evaluate(currentSurfaceAngle / 90f) * climbingStairsMultiplierCurve) + 1f);
                        globalForward = tmpGlobalForward * ((speedMultiplierOnAngle.Evaluate(currentSurfaceAngle / 90f) * climbingStairsMultiplierCurve) + 1f);
                        reactionForward = tmpReactionForward * ((speedMultiplierOnAngle.Evaluate(currentSurfaceAngle / 90f) * climbingStairsMultiplierCurve) + 1f);

                        if (!isCrouch)
                        {
                            SetFriction(frictionAgainstFloor, true);
                        }
                        currentLockOnSlope = true;
                    }
                    else
                    {
                        //set forward
                        forward = tmpForward * ((speedMultiplierOnAngle.Evaluate(currentSurfaceAngle / 90f) * cantSlideMultiplierCurve) + 1f);
                        globalForward = tmpGlobalForward * ((speedMultiplierOnAngle.Evaluate(currentSurfaceAngle / 90f) * cantSlideMultiplierCurve) + 1f);
                        reactionForward = tmpReactionForward * ((speedMultiplierOnAngle.Evaluate(currentSurfaceAngle / 90f) * cantSlideMultiplierCurve) + 1f);

                        SetFriction(0f, true);
                        currentLockOnSlope = lockOnSlope;
                    }

                    currentSurfaceAngle = Vector3.Angle(Vector3.up, slopeHit.normal);
                    isTouchingSlope = true;
                }

                //set down
                down = Vector3.Project(Vector3.down, slopeHit.normal);
                globalDown = Vector3.down.normalized;
                reactionGlobalDown = Vector3.up.normalized;
            }
            else
            {
                groundNormal = Vector3.zero;

                forward = Vector3.ProjectOnPlane(transform.forward, slopeHit.normal).normalized;
                globalForward = forward;
                reactionForward = forward;

                //set down
                down = Vector3.down.normalized;
                globalDown = Vector3.down.normalized;
                reactionGlobalDown = Vector3.up.normalized;

                SetFriction(frictionAgainstFloor, true);
                currentLockOnSlope = lockOnSlope;
            }
        }

        #endregion


        #region Move

        //Edit this for surfing/skiing/slide mechanic
        private void MoveCrouch()
        {
            if (crouch && isGrounded)
            {
                isCrouch = true;
                if (meshCharacterCrouch != null && meshCharacter != null) meshCharacter.SetActive(false);
                if (meshCharacterCrouch != null) meshCharacterCrouch.SetActive(true);

                float newHeight = originalColliderHeight * crouchHeightMultiplier;
                collider.height = newHeight;
                collider.center = new Vector3(0f, -newHeight * crouchHeightMultiplier, 0f);

                crouchSpeedMultiplier = originalCrouchSpeedMultiplier;
                gravityMultiplier = originalGravityMultiplier;

                headPoint.position = new Vector3(transform.position.x + POV_crouchHeadHeight.x, transform.position.y + POV_crouchHeadHeight.y, transform.position.z + POV_crouchHeadHeight.z);
            }
            //Air Crouch
            else if (crouch && !isGrounded)
            {
                isCrouch = true;

                if (meshCharacterCrouch != null && meshCharacter != null) meshCharacter.SetActive(false);
                if (meshCharacterCrouch != null) meshCharacterCrouch.SetActive(true);

                float newHeight = originalColliderHeight * crouchHeightMultiplier;
                collider.height = newHeight;
                collider.center = new Vector3(0f, -newHeight * crouchHeightMultiplier, 0f);

                headPoint.position = new Vector3(transform.position.x + POV_crouchHeadHeight.x, transform.position.y + POV_crouchHeadHeight.y, transform.position.z + POV_crouchHeadHeight.z);

                //change majority of vilocity to down
                crouchSpeedMultiplier = 0f;

                currVelocity = new Vector3(0f, currVelocity.y - Mathf.Abs(currVelocity.x) - Mathf.Abs(currVelocity.z), 0f);
                if (airCrouchCompleteStop)
                {
                    rigidbody.velocity = new Vector3(0f, rigidbody.velocity.y - Mathf.Abs(rigidbody.velocity.x / 2) - Mathf.Abs(rigidbody.velocity.z / 2), 0f);
                }
                gravityMultiplier = airCrouchGravityMultiplier;
            }
            else
            {
                isCrouch = false;

                if (meshCharacterCrouch != null && meshCharacter != null) meshCharacter.SetActive(true);
                if (meshCharacterCrouch != null) meshCharacterCrouch.SetActive(false);

                crouchSpeedMultiplier = originalCrouchSpeedMultiplier;
                gravityMultiplier = originalGravityMultiplier;

                collider.height = originalColliderHeight;
                collider.center = Vector3.zero;

                headPoint.position = new Vector3(transform.position.x + POV_normalHeadHeight.x, transform.position.y + POV_normalHeadHeight.y, transform.position.z + POV_normalHeadHeight.z);
            }
        }


        private void MoveWalk()
        {
            float crouchMultiplier = 1f;
            if (isCrouch) crouchMultiplier = crouchSpeedMultiplier;

            if (axisInput.magnitude > movementThrashold)
            {
                targetAngle = Mathf.Atan2(axisInput.x, axisInput.y) * Mathf.Rad2Deg + characterCamera.transform.eulerAngles.y;
                if (!sprint) rigidbody.velocity = Vector3.SmoothDamp(rigidbody.velocity, forward * movementSpeed * crouchMultiplier, ref currVelocity, dampSpeedUp);
                else rigidbody.velocity = Vector3.SmoothDamp(rigidbody.velocity, forward * sprintSpeed * crouchMultiplier, ref currVelocity, dampSpeedUp);
            }
            else rigidbody.velocity = Vector3.SmoothDamp(rigidbody.velocity, Vector3.zero * crouchMultiplier, ref currVelocity, dampSpeedDown);
        }


        private void MoveRotation()
        {
            float angle = Mathf.SmoothDampAngle(characterModel.transform.eulerAngles.y, targetAngle, ref turnSmoothVelocity, characterModelRotationSmooth);
            transform.rotation = Quaternion.Euler(0f, targetAngle, 0f);

            if (!lockRotation) characterModel.transform.rotation = Quaternion.Euler(0f, angle, 0f);
            else
            {
                var lookPos = -wallNormal;
                lookPos.y = 0;
                var rotation = Quaternion.LookRotation(lookPos);
                characterModel.transform.rotation = rotation;
            }
        }


        public void ForceRotation()
        {
            characterModel.transform.rotation = Quaternion.Euler(0f, characterCamera.transform.rotation.eulerAngles.y, 0f);
        }


        private void MoveJump()
        {
            if (jump && isGrinding)
            {
                rigidbody.velocity = grindDirection * currentRail.grindSpeed + Vector3.up * jumpVelocity;
                StopGrinding();
                isJumping = true;
                return;
            }

            //jumped
            if (jump && isGrounded && ((isTouchingSlope && currentSurfaceAngle <= maxClimbableSlopeAngle) || !isTouchingSlope))
            {
                rigidbody.velocity += Vector3.up * jumpVelocity;
                isJumping = true;
            }
            //jumped from wall
            else if (jump && !isGrounded && isTouchingWall)
            {
                rigidbody.velocity += wallNormal * jumpFromWallMultiplier + (Vector3.up * jumpFromWallMultiplier) * multiplierVerticalLeap;
                isJumping = true;

                targetAngle = Mathf.Atan2(wallNormal.x, wallNormal.z) * Mathf.Rad2Deg;

                forward = wallNormal;
                globalForward = forward;
                reactionForward = forward;
            }
            //is falling
            if (rigidbody.velocity.y < 0 && !isGrounded) coyoteJumpMultiplier = fallMultiplier;
            else if (rigidbody.velocity.y > 0.1f && (currentSurfaceAngle <= maxClimbableSlopeAngle || isTouchingStep))
            {
                //is short jumping
                if (!jumpHold || !canLongJump) coyoteJumpMultiplier = 1f;
                //is long jumping
                else coyoteJumpMultiplier = 1f / holdJumpMultiplier;
            }
            else
            {
                isJumping = false;
                coyoteJumpMultiplier = 1f;
            }
        }

        #endregion


        #region Gravity

        private void ApplyGravity()
        {
            Vector3 gravity = Vector3.zero;

            if ((currentLockOnSlope && isGrounded) || isTouchingStep) gravity = down * gravityMultiplier * -Physics.gravity.y * coyoteJumpMultiplier;
            else if (currentLockOnSlope && !isGrounded) gravity = new Vector3(0f, down.y, 0f) * gravityMultiplier * -Physics.gravity.y * coyoteJumpMultiplier;

            else gravity = globalDown * gravityMultiplier * -Physics.gravity.y * coyoteJumpMultiplier;

            //avoid little jump
            if (groundNormal.y != 1 && groundNormal.y != 0 && isTouchingSlope && prevGroundNormal != groundNormal)
            {
                //Debug.Log("Added correction jump on slope");
                gravity *= gravityMultiplyerOnSlideChange;
            }

            //slide if angle too big
            if (groundNormal.y != 1 && groundNormal.y != 0 && (currentSurfaceAngle > maxClimbableSlopeAngle && !isTouchingStep))
            {
                //Debug.Log("Slope angle too high, character is sliding");
                if (currentSurfaceAngle > 0f && currentSurfaceAngle <= 30f) gravity = globalDown * gravityMultiplierIfUnclimbableSlope * -Physics.gravity.y;
                else if (currentSurfaceAngle > 30f && currentSurfaceAngle <= 89f) gravity = globalDown * gravityMultiplierIfUnclimbableSlope / 2f * -Physics.gravity.y;
            }

            //friction when touching wall
            if (isTouchingWall && rigidbody.velocity.y < 0) gravity *= frictionAgainstWall;

            rigidbody.AddForce(gravity);
        }

        #endregion

        #region Grinding

        private void StartGrinding(GrindRail rail)
        {
            currentRail = rail;
            isGrinding = true;

            // Find closest point on rail and starting position
            Vector3 closestPoint = rail.GetClosestPointOnRail(transform.position, out grindPositionT);

            // Determine grind direction based on velocity
            grindDirection = rail.GetRailDirection();
            if (Vector3.Dot(rigidbody.velocity, grindDirection) < 0)
            {
                grindDirection = -grindDirection;
            }

            // Lock rotation to rail direction FIRST
            targetAngle = Mathf.Atan2(grindDirection.x, grindDirection.z) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0f, targetAngle, 0f);

            // Calculate proper offset in WORLD SPACE based on character rotation
            float colliderBottom = collider.center.y - (collider.height / 2f);
            Vector3 colliderCenterWorld = transform.rotation * collider.center;

            Vector3 properOffset = new Vector3(
                -colliderCenterWorld.x,
                grindHeightOffset - colliderBottom,
                -colliderCenterWorld.z
            );

            Vector3 finalPosition = closestPoint + properOffset;

            // ROUND X and Z to snap to rail
            finalPosition.x = Mathf.Round(finalPosition.x);
            finalPosition.z = Mathf.Round(finalPosition.z);

            transform.position = finalPosition;

            Debug.Log("Started grinding on rail");
        }

        private void UpdateGrinding()
        {
            if (currentRail == null)
            {
                StopGrinding();
                return;
            }

            // Move along rail
            float moveAmount = (currentRail.grindSpeed * Time.fixedDeltaTime) / currentRail.GetRailLength();
            grindPositionT += moveAmount;

            // Check if reached end of rail
            if (grindPositionT >= 1f)
            {
                StopGrinding();
                return;
            }

            // Update rotation FIRST
            transform.rotation = Quaternion.Euler(0f, targetAngle, 0f);
            characterModel.transform.rotation = Quaternion.Euler(0f, targetAngle, 0f);

            // Update position along rail
            Vector3 railPosition = currentRail.GetPositionAtT(grindPositionT);

            // Calculate proper offset in WORLD SPACE based on character rotation
            float colliderBottom = collider.center.y - (collider.height / 2f);
            Vector3 colliderCenterWorld = transform.rotation * collider.center;

            Vector3 properOffset = new Vector3(
                -colliderCenterWorld.x,
                grindHeightOffset - colliderBottom,
                -colliderCenterWorld.z
            );

            Vector3 finalPosition = railPosition + properOffset;
            finalPosition.x = Mathf.Round(finalPosition.x);
            transform.position = finalPosition;
            rigidbody.velocity = grindDirection * currentRail.grindSpeed;
        }
        private void StopGrinding()
        {
            if (!isGrinding) return;

            isGrinding = false;
            currentRail = null;
            rigidbody.velocity = grindDirection * (currentRail != null ? currentRail.grindSpeed : movementSpeed);
        }

        #endregion

        #region Events

        private void UpdateEvents()
        {
            if ((jump && isGrounded && ((isTouchingSlope && currentSurfaceAngle <= maxClimbableSlopeAngle) || !isTouchingSlope)) || (jump && !isGrounded && isTouchingWall)) OnJump.Invoke();
            if (isGrounded && !prevGrounded && rigidbody.velocity.y > -minimumVerticalSpeedToLandEvent) OnLand.Invoke();
            if (Mathf.Abs(rigidbody.velocity.x) + Mathf.Abs(rigidbody.velocity.z) > minimumHorizontalSpeedToFastEvent) OnFast.Invoke();
            if (isTouchingWall && rigidbody.velocity.y < 0) OnWallSlide.Invoke();
            if (sprint) OnSprint.Invoke();
            if (isCrouch) OnCrouch.Invoke();
        }

        #endregion


        #region Friction and Round

        private void SetFriction(float _frictionWall, bool _isMinimum)
        {
            collider.material.dynamicFriction = 0.6f * _frictionWall;
            collider.material.staticFriction = 0.6f * _frictionWall;

            if (_isMinimum) collider.material.frictionCombine = PhysicMaterialCombine.Minimum;
            else collider.material.frictionCombine = PhysicMaterialCombine.Maximum;
        }


        private float RoundValue(float _value)
        {
            float unit = (float)Mathf.Round(_value);

            if (_value - unit < 0.000001f && _value - unit > -0.000001f) return unit;
            else return _value;
        }

        #endregion


        #region Getters & Setters

        public bool  GetGrounded()               { return isGrounded; }
        public bool  GetTouchingSlope()          { return isTouchingSlope; }
        public bool  GetTouchingStep()           { return isTouchingStep; }
        public bool  GetTouchingWall()           { return isTouchingWall; }
        public bool  GetJumping()                { return isJumping; }
        public bool  GetCrouching()              { return isCrouch; }
        public float GetOriginalColliderHeight() { return originalColliderHeight; }

        public void SetLockRotation(bool _lock) { lockRotation = _lock; }
        public void SetLockToCamera(bool _lockToCamera)
        {
            lockToCamera = _lockToCamera;
            if (!_lockToCamera) targetAngle = characterModel.transform.eulerAngles.y;
        }
        public bool GetGrinding() { return isGrinding; }

        #endregion


        #region Gizmos

        public void ToggleDebug()
        {
            debug = !debug;
        }


        private void OnDrawGizmos()
        {
            if (debug)
            {
                rigidbody = this.GetComponent<Rigidbody>();
                collider = this.GetComponent<CapsuleCollider>();

                Vector3 bottomStepPos = transform.position - new Vector3(0f, originalColliderHeight / 2f, 0f) + new Vector3(0f, 0.05f, 0f);
                Vector3 topWallPos = new Vector3(transform.position.x, transform.position.y + hightWallCheckerChecker, transform.position.z);

                //ground and slope
                Gizmos.color = Color.blue;
                Gizmos.DrawWireSphere(transform.position - new Vector3(0, originalColliderHeight / 2f, 0), groundCheckerThrashold);

                Gizmos.color = Color.green;
                Gizmos.DrawWireSphere(transform.position - new Vector3(0, originalColliderHeight / 2f, 0), slopeCheckerThrashold);

                //direction
                Gizmos.color = Color.blue;
                Gizmos.DrawLine(transform.position, transform.position + forward * 2f);

                Gizmos.color = Color.cyan;
                Gizmos.DrawLine(transform.position, transform.position + globalForward * 2);

                Gizmos.color = Color.cyan;
                Gizmos.DrawLine(transform.position, transform.position + reactionForward * 2f);

                Gizmos.color = Color.red;
                Gizmos.DrawLine(transform.position, transform.position + down * 2f);

                Gizmos.color = Color.magenta;
                Gizmos.DrawLine(transform.position, transform.position + globalDown * 2f);

                Gizmos.color = Color.magenta;
                Gizmos.DrawLine(transform.position, transform.position + reactionGlobalDown * 2f);

                //step check
                Gizmos.color = Color.black;
                Gizmos.DrawLine(bottomStepPos, bottomStepPos + globalForward * stepCheckerThrashold);

                Gizmos.color = Color.black;
                Gizmos.DrawLine(bottomStepPos + new Vector3(0f, maxStepHeight, 0f), bottomStepPos + new Vector3(0f, maxStepHeight, 0f) + globalForward * (stepCheckerThrashold + 0.05f));

                Gizmos.color = Color.black;
                Gizmos.DrawLine(bottomStepPos, bottomStepPos + Quaternion.AngleAxis(45, transform.up) * (globalForward * stepCheckerThrashold));

                Gizmos.color = Color.black;
                Gizmos.DrawLine(bottomStepPos + new Vector3(0f, maxStepHeight, 0f), bottomStepPos + Quaternion.AngleAxis(45, Vector3.up) * (globalForward * stepCheckerThrashold) + new Vector3(0f, maxStepHeight, 0f));

                Gizmos.color = Color.black;
                Gizmos.DrawLine(bottomStepPos, bottomStepPos + Quaternion.AngleAxis(-45, transform.up) * (globalForward * stepCheckerThrashold));

                Gizmos.color = Color.black;
                Gizmos.DrawLine(bottomStepPos + new Vector3(0f, maxStepHeight, 0f), bottomStepPos + Quaternion.AngleAxis(-45, Vector3.up) * (globalForward * stepCheckerThrashold) + new Vector3(0f, maxStepHeight, 0f));

                //wall check
                Gizmos.color = Color.black;
                Gizmos.DrawLine(topWallPos, topWallPos + globalForward * wallCheckerThrashold);

                Gizmos.color = Color.black;
                Gizmos.DrawLine(topWallPos, topWallPos + Quaternion.AngleAxis(45, transform.up) * (globalForward * wallCheckerThrashold));

                Gizmos.color = Color.black;
                Gizmos.DrawLine(topWallPos, topWallPos + Quaternion.AngleAxis(90, transform.up) * (globalForward * wallCheckerThrashold));

                Gizmos.color = Color.black;
                Gizmos.DrawLine(topWallPos, topWallPos + Quaternion.AngleAxis(135, transform.up) * (globalForward * wallCheckerThrashold));

                Gizmos.color = Color.black;
                Gizmos.DrawLine(topWallPos, topWallPos + Quaternion.AngleAxis(180, transform.up) * (globalForward * wallCheckerThrashold));

                Gizmos.color = Color.black;
                Gizmos.DrawLine(topWallPos, topWallPos + Quaternion.AngleAxis(225, transform.up) * (globalForward * wallCheckerThrashold));

                Gizmos.color = Color.black;
                Gizmos.DrawLine(topWallPos, topWallPos + Quaternion.AngleAxis(270, transform.up) * (globalForward * wallCheckerThrashold));

                Gizmos.color = Color.black;
                Gizmos.DrawLine(topWallPos, topWallPos + Quaternion.AngleAxis(315, transform.up) * (globalForward * wallCheckerThrashold));
            }
        }

        #endregion
    }
}
