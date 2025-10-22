using UnityEngine;
using Cinemachine;

public class CameraController : MonoBehaviour
{
    #region Variables and References
    [Header("External References")]
    [SerializeField] private PlayerInputReader playerInputReader;
    [SerializeField] private CinemachineFreeLook freeLookCamera;

    [Header("Camera Settings")]
    [SerializeField] private float lookSensitivityXAxis = 150f;
    [SerializeField] private float lookSensitivityYAxis = 2f;
    [SerializeField] private bool invertY = false;

    private Vector2 cameraInputVector;
    #endregion

    #region Lifecycle Methods
    private void OnEnable()
    {
        playerInputReader.OnControlCameraEvent += HandleControlCameraEvent;
    }

    private void OnDisable()
    {
        playerInputReader.OnControlCameraEvent -= HandleControlCameraEvent;
    }

    private void Update()
    {
        if (freeLookCamera != null && cameraInputVector != Vector2.zero)
        {
            // Horizontal Axis Control
            freeLookCamera.m_XAxis.Value += cameraInputVector.x * lookSensitivityXAxis * Time.deltaTime;

            // Vertical Axis Control
            float yInput = invertY ? cameraInputVector.y : -cameraInputVector.y;
            freeLookCamera.m_YAxis.Value += yInput * lookSensitivityYAxis * Time.deltaTime;
        }
    }
    #endregion

    #region Camera Control Logic
    private void HandleControlCameraEvent(Vector2 newCameraInputVector)
    {
        cameraInputVector = newCameraInputVector;
    }
    #endregion
}
