using UnityEngine;

/// <summary>
/// This is currently only used for our 2D trees but could be used for any
/// 2D "billboard-like" asset in the future
/// </summary>
public class BillboardSprite : MonoBehaviour
{
    [Header("Billboard Settings")]
    [Tooltip("If true, only rotates on Y-axis (good for trees). If false, faces camera completely.")]
    public bool cylindricalBillboard = true;
    
    [Tooltip("If true, uses LateUpdate for smoother camera following")]
    public bool useLateUpdate = true;

    private Camera mainCamera;
    private Transform cameraTransform;

    void Start()
    {
        mainCamera = Camera.main;
        
        if (mainCamera != null)
        {
            cameraTransform = mainCamera.transform;
        }
        else
        {
            Debug.LogWarning("SpriteTree: No main camera found!");
        }
    }

    void Update()
    {
        if (!useLateUpdate)
        {
            FaceCamera();
        }
    }

    void LateUpdate()
    {
        if (useLateUpdate)
        {
            FaceCamera();
        }
    }

    void FaceCamera()
    {
        if (cameraTransform == null) return;

        if (cylindricalBillboard)
        {
            // Only rotate on Y-axis (keeps trees upright)
            Vector3 directionToCamera = cameraTransform.position - transform.position;
            directionToCamera.y = 0; // Ignore vertical difference
            
            if (directionToCamera != Vector3.zero)
            {
                transform.rotation = Quaternion.LookRotation(directionToCamera);
            }
        }
        else
        {
            // Fully face the camera
            transform.LookAt(cameraTransform);
        }
    }
}
