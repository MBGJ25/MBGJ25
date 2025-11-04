using System.Collections;
using UnityEngine;

namespace PhysicsCharacterController
{
    public class PlayerSoundManager : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private InputReader input;
        [SerializeField] private CharacterManager characterManager;
        [SerializeField] private PlayerSounds playerSounds;

        [Header("Footstep Settings")]
        [SerializeField] private float walkFootstepInterval = 0.5f;  // Time between footsteps when walking
        [SerializeField] private float sprintFootstepInterval = 0.3f; // Time between footsteps when sprinting
        [SerializeField] private float movementThreshold = 0.01f;     // Minimum input to trigger footsteps

        private float footstepTimer = 0f;
        private bool wasGrounded = false;

        private void Update()
        {
            HandleFootsteps();
        }

        private void HandleFootsteps()
        {
            // Get current input and state
            Vector2 axisInput = input.axisInput;
            bool isGrounded = characterManager.GetGrounded();
            bool isSprinting = input.sprint;
            bool isCrouching = characterManager.GetCrouching();

            // Only play footsteps when grounded and moving
            if (isGrounded && axisInput.magnitude > movementThreshold && !isCrouching)
            {
                // Determine interval based on sprint state
                float currentInterval = isSprinting ? sprintFootstepInterval : walkFootstepInterval;

                // Update timer
                footstepTimer += Time.deltaTime;

                // Play footstep when timer exceeds interval
                if (footstepTimer >= currentInterval)
                {
                    playerSounds.PlayFootstep();
                    footstepTimer = 0f; // Reset timer
                }
            }
            else
            {
                // Reset timer when not moving or not grounded
                footstepTimer = 0f;
            }

            wasGrounded = isGrounded;
        }
    }
}