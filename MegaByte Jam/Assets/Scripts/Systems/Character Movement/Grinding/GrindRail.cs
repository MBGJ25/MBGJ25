using UnityEngine;

public class GrindRail : MonoBehaviour
{
    [Header("Rail Configuration")]
    public Transform startPoint;
    public Transform endPoint;
    public float grindSpeed = 18f;

    [Header("Entry Settings")]
    public float minEntrySpeed = 2f;
    public float maxEntryAngle = 90f;

    private Vector3 railDirection;
    private float railLength;

    private void Start()
    {
        CalculateRailProperties();
    }

    private void OnValidate()
    {
        CalculateRailProperties();
    }

    private void CalculateRailProperties()
    {
        if (startPoint != null && endPoint != null)
        {
            railDirection = (endPoint.position - startPoint.position).normalized;
            railLength = Vector3.Distance(startPoint.position, endPoint.position);
        }
    }

    public Vector3 GetRailDirection()
    {
        return railDirection;
    }

    public float GetRailLength()
    {
        return railLength;
    }

    public Vector3 GetStartPosition()
    {
        return startPoint.position;
    }

    public Vector3 GetEndPosition()
    {
        return endPoint.position;
    }

    // Get position along rail (0 = start, 1 = end)
    public Vector3 GetPositionAtT(float t)
    {
        t = Mathf.Clamp01(t);
        return Vector3.Lerp(startPoint.position, endPoint.position, t);
    }

    // Get closest point on rail to a world position
    public Vector3 GetClosestPointOnRail(Vector3 worldPosition, out float tValue)
    {
        Vector3 startToPoint = worldPosition - startPoint.position;
        float projection = Vector3.Dot(startToPoint, railDirection);

        tValue = Mathf.Clamp01(projection / railLength);
        return GetPositionAtT(tValue);
    }

    // Check if character's velocity is compatible with grinding
    public bool CanStartGrinding(Vector3 velocity)
    {
        if (velocity.magnitude < minEntrySpeed)
            return false;

        // Check if moving roughly in rail direction
        float angle = Vector3.Angle(velocity.normalized, railDirection);
        return angle <= maxEntryAngle;
    }

    private void OnDrawGizmos()
    {
        if (startPoint != null && endPoint != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(startPoint.position, endPoint.position);

            // Draw start sphere
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(startPoint.position, 0.2f);

            // Draw end sphere
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(endPoint.position, 0.2f);
        }
    }
}
