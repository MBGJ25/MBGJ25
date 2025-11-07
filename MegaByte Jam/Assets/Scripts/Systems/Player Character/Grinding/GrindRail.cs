using UnityEngine;
using System.Collections.Generic;

public class GrindRail : MonoBehaviour
{
    [Header("Rail Configuration")]
    [Tooltip("Waypoints that define the rail path. For straight rails, use 2 points. For curves, add more.")]
    public Transform[] waypoints;
    
    [Header("Curve Settings")]
    [Tooltip("Number of segments to calculate for accurate arc length (higher = more accurate but slower)")]
    [Range(10, 200)]
    public int curveResolution = 50;
    
    [Tooltip("Use Catmull-Rom spline for smoother curves through all waypoints")]
    public bool useCatmullRom = true;

    [Header("Grind Settings")]
    public float grindSpeed = 18f;

    [Header("Entry Settings")]
    public float minEntrySpeed = 2f;
    public float maxEntryAngle = 90f;

    // Cached curve data
    private List<Vector3> curvePoints = new List<Vector3>();
    private List<float> arcLengths = new List<float>();
    private float totalRailLength;

    private void Start()
    {
        CalculateRailCurve();
    }

    private void OnValidate()
    {
        CalculateRailCurve();
    }

    private void CalculateRailCurve()
    {
        if (waypoints == null || waypoints.Length < 2)
        {
            Debug.LogWarning("GrindRail requires at least 2 waypoints!");
            return;
        }

        // Clear previous data
        curvePoints.Clear();
        arcLengths.Clear();
        totalRailLength = 0f;

        // Generate curve points based on waypoint count
        if (waypoints.Length == 2 || !useCatmullRom)
        {
            // Simple linear interpolation for straight rails
            GenerateLinearCurve();
        }
        else
        {
            // Catmull-Rom spline for curved rails
            GenerateCatmullRomCurve();
        }

        // Calculate arc lengths for uniform parameterization
        CalculateArcLengths();
    }

    private void GenerateLinearCurve()
    {
        for (int i = 0; i <= curveResolution; i++)
        {
            float t = i / (float)curveResolution;
            Vector3 point = Vector3.Lerp(waypoints[0].position, waypoints[1].position, t);
            curvePoints.Add(point);
        }
    }

    private void GenerateCatmullRomCurve()
    {
        // For each segment between waypoints
        for (int i = 0; i < waypoints.Length - 1; i++)
        {
            // Get the 4 control points for Catmull-Rom
            Vector3 p0 = GetWaypointPosition(i - 1);
            Vector3 p1 = waypoints[i].position;
            Vector3 p2 = waypoints[i + 1].position;
            Vector3 p3 = GetWaypointPosition(i + 2);

            // Generate points along this segment
            int segmentResolution = curveResolution / (waypoints.Length - 1);
            for (int j = 0; j <= segmentResolution; j++)
            {
                // Skip the first point of segments after the first to avoid duplicates
                if (i > 0 && j == 0) continue;

                float t = j / (float)segmentResolution;
                Vector3 point = CalculateCatmullRomPoint(t, p0, p1, p2, p3);
                curvePoints.Add(point);
            }
        }
    }

    private Vector3 GetWaypointPosition(int index)
    {
        // Handle out-of-bounds indices by extending the curve
        if (index < 0)
        {
            // Extend before start
            Vector3 dir = waypoints[0].position - waypoints[1].position;
            return waypoints[0].position + dir;
        }
        else if (index >= waypoints.Length)
        {
            // Extend after end
            Vector3 dir = waypoints[waypoints.Length - 1].position - waypoints[waypoints.Length - 2].position;
            return waypoints[waypoints.Length - 1].position + dir;
        }
        return waypoints[index].position;
    }

    private Vector3 CalculateCatmullRomPoint(float t, Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3)
    {
        // Catmull-Rom spline calculation
        float t2 = t * t;
        float t3 = t2 * t;

        Vector3 point = 0.5f * (
            (2f * p1) +
            (-p0 + p2) * t +
            (2f * p0 - 5f * p1 + 4f * p2 - p3) * t2 +
            (-p0 + 3f * p1 - 3f * p2 + p3) * t3
        );

        return point;
    }

    private void CalculateArcLengths()
    {
        arcLengths.Add(0f);
        totalRailLength = 0f;

        for (int i = 1; i < curvePoints.Count; i++)
        {
            float segmentLength = Vector3.Distance(curvePoints[i - 1], curvePoints[i]);
            totalRailLength += segmentLength;
            arcLengths.Add(totalRailLength);
        }
    }

    // Get position at normalized distance (0-1) along the curve
    public Vector3 GetPositionAtT(float t)
    {
        if (curvePoints.Count == 0)
        {
            CalculateRailCurve();
        }

        t = Mathf.Clamp01(t);
        
        // Convert uniform t to arc-length parameterized position
        float targetLength = t * totalRailLength;
        
        // Find the segment containing this arc length
        for (int i = 1; i < arcLengths.Count; i++)
        {
            if (arcLengths[i] >= targetLength)
            {
                // Interpolate between points i-1 and i
                float segmentLength = arcLengths[i] - arcLengths[i - 1];
                float segmentT = segmentLength > 0 
                    ? (targetLength - arcLengths[i - 1]) / segmentLength 
                    : 0f;
                
                return Vector3.Lerp(curvePoints[i - 1], curvePoints[i], segmentT);
            }
        }

        // Fallback to last point
        return curvePoints[curvePoints.Count - 1];
    }

    // Get tangent direction at normalized distance (0-1) along the curve
    public Vector3 GetDirectionAtT(float t)
    {
        if (curvePoints.Count == 0)
        {
            CalculateRailCurve();
        }

        t = Mathf.Clamp01(t);
        float targetLength = t * totalRailLength;

        // Find the segment containing this arc length
        for (int i = 1; i < arcLengths.Count; i++)
        {
            if (arcLengths[i] >= targetLength)
            {
                // Use the segment direction
                return (curvePoints[i] - curvePoints[i - 1]).normalized;
            }
        }

        // Fallback to last segment direction
        int lastIdx = curvePoints.Count - 1;
        return (curvePoints[lastIdx] - curvePoints[lastIdx - 1]).normalized;
    }

    public float GetRailLength()
    {
        if (totalRailLength == 0f)
        {
            CalculateRailCurve();
        }
        return totalRailLength;
    }

    public Vector3 GetStartPosition()
    {
        return waypoints != null && waypoints.Length > 0 ? waypoints[0].position : Vector3.zero;
    }

    public Vector3 GetEndPosition()
    {
        return waypoints != null && waypoints.Length > 0 ? waypoints[waypoints.Length - 1].position : Vector3.zero;
    }

    // Legacy method for compatibility
    public Vector3 GetRailDirection()
    {
        // Return average direction from start to end
        if (waypoints != null && waypoints.Length >= 2)
        {
            return (waypoints[waypoints.Length - 1].position - waypoints[0].position).normalized;
        }
        return Vector3.forward;
    }

    public Vector3 GetClosestPointOnRail(Vector3 worldPosition, out float tValue)
    {
        if (curvePoints.Count == 0)
        {
            CalculateRailCurve();
        }

        float closestDist = float.MaxValue;
        int closestIndex = 0;
        tValue = 0f;

        // Find closest point on the curve
        for (int i = 0; i < curvePoints.Count; i++)
        {
            float dist = Vector3.Distance(worldPosition, curvePoints[i]);
            if (dist < closestDist)
            {
                closestDist = dist;
                closestIndex = i;
            }
        }

        // Convert index to t value using arc length
        if (closestIndex < arcLengths.Count && totalRailLength > 0)
        {
            tValue = arcLengths[closestIndex] / totalRailLength;
        }

        return curvePoints[closestIndex];
    }

    public bool CanStartGrinding(Vector3 velocity, Vector3 playerForward, out Vector3 preferredDirection)
    {
        if (velocity.magnitude < minEntrySpeed)
        {
            preferredDirection = GetRailDirection();
            return false;
        }

        // Get general rail direction
        Vector3 railDir = GetRailDirection();

        // Determine which direction is closer to player's facing
        float dotForward = Vector3.Dot(playerForward, railDir);
        float dotBackward = Vector3.Dot(playerForward, -railDir);

        preferredDirection = (dotForward >= dotBackward) ? railDir : -railDir;
        return true;
    }

    private void OnDrawGizmos()
    {
        if (waypoints == null || waypoints.Length < 2) return;

        // Draw waypoints
        for (int i = 0; i < waypoints.Length; i++)
        {
            if (waypoints[i] == null) continue;

            // Draw waypoint sphere
            Gizmos.color = (i == 0) ? Color.green : (i == waypoints.Length - 1) ? Color.red : Color.yellow;
            Gizmos.DrawWireSphere(waypoints[i].position, 0.2f);

            // Draw label
            #if UNITY_EDITOR
            UnityEditor.Handles.Label(waypoints[i].position + Vector3.up * 0.3f, $"WP {i}");
            #endif
        }

        // Draw the curve
        if (curvePoints != null && curvePoints.Count > 1)
        {
            Gizmos.color = Color.cyan;
            for (int i = 1; i < curvePoints.Count; i++)
            {
                Gizmos.DrawLine(curvePoints[i - 1], curvePoints[i]);
            }
        }
        else if (waypoints.Length >= 2)
        {
            // Fallback: draw straight lines between waypoints
            Gizmos.color = Color.yellow;
            for (int i = 1; i < waypoints.Length; i++)
            {
                if (waypoints[i] != null && waypoints[i - 1] != null)
                {
                    Gizmos.DrawLine(waypoints[i - 1].position, waypoints[i].position);
                }
            }
        }
    }
}