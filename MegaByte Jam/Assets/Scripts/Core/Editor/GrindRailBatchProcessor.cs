using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

/// <summary>
/// Batch processes multiple grind rails at once
/// Useful for: Converting many old rails, generating waypoints for multiple meshes, or cleaning up a scene
/// </summary>
public class GrindRailBatchProcessor : EditorWindow
{
    private List<GrindRail> railsToProcess = new List<GrindRail>();
    private Vector2 scrollPosition;
    
    // Batch settings
    private int batchWaypointCount = 10;
    private float batchHeightOffset = 0f;
    private bool batchUseVertexSampling = true;
    private bool batchRecalculateCurves = false;
    
    [MenuItem("Tools/Grind Rail Batch Processor")]
    public static void ShowWindow()
    {
        GetWindow<GrindRailBatchProcessor>("Batch Rail Processor");
    }

    private void OnGUI()
    {
        GUILayout.Label("Grind Rail Batch Processor", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox("Process multiple grind rails at once. Perfect for converting old rails or setting up many rails quickly.", MessageType.Info);
        
        EditorGUILayout.Space(10);
        
        // Quick add buttons
        EditorGUILayout.LabelField("Quick Add", EditorStyles.boldLabel);
        
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("➕ Add Selected Rails"))
        {
            AddSelectedRails();
        }
        if (GUILayout.Button("🔍 Find All in Scene"))
        {
            FindAllRailsInScene();
        }
        EditorGUILayout.EndHorizontal();
        
        if (GUILayout.Button("🗑️ Clear List"))
        {
            railsToProcess.Clear();
        }
        
        EditorGUILayout.Space(10);
        
        // Rails list
        EditorGUILayout.LabelField($"Rails to Process ({railsToProcess.Count})", EditorStyles.boldLabel);
        
        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, GUILayout.Height(150));
        
        List<GrindRail> toRemove = new List<GrindRail>();
        
        for (int i = 0; i < railsToProcess.Count; i++)
        {
            EditorGUILayout.BeginHorizontal();
            
            railsToProcess[i] = (GrindRail)EditorGUILayout.ObjectField(railsToProcess[i], typeof(GrindRail), true);
            
            if (GUILayout.Button("✖", GUILayout.Width(25)))
            {
                toRemove.Add(railsToProcess[i]);
            }
            
            EditorGUILayout.EndHorizontal();
        }
        
        foreach (var rail in toRemove)
        {
            railsToProcess.Remove(rail);
        }
        
        EditorGUILayout.EndScrollView();
        
        EditorGUILayout.Space(10);
        
        // Batch settings
        EditorGUILayout.LabelField("Batch Settings", EditorStyles.boldLabel);
        batchWaypointCount = EditorGUILayout.IntSlider("Waypoint Count", batchWaypointCount, 2, 50);
        batchHeightOffset = EditorGUILayout.Slider("Height Offset", batchHeightOffset, -2f, 2f);
        batchUseVertexSampling = EditorGUILayout.Toggle("Use Vertex Sampling", batchUseVertexSampling);
        batchRecalculateCurves = EditorGUILayout.Toggle("Recalculate Existing Curves", batchRecalculateCurves);
        
        EditorGUILayout.Space(10);
        
        // Actions
        EditorGUILayout.LabelField("Batch Actions", EditorStyles.boldLabel);
        
        if (GUILayout.Button("🚀 Generate All from Mesh", GUILayout.Height(40)))
        {
            if (EditorUtility.DisplayDialog("Batch Generate", 
                $"Generate waypoints for {railsToProcess.Count} rails?\n\nThis will replace existing waypoints.", 
                "Yes", "Cancel"))
            {
                BatchGenerateFromMesh();
            }
        }
        
        if (GUILayout.Button("📊 Recalculate All Curves"))
        {
            if (EditorUtility.DisplayDialog("Recalculate Curves", 
                $"Recalculate curve data for {railsToProcess.Count} rails?", 
                "Yes", "Cancel"))
            {
                BatchRecalculateCurves();
            }
        }
        
        EditorGUILayout.Space(10);
        
        EditorGUILayout.LabelField("Bulk Modifications", EditorStyles.boldLabel);
        
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("⬆️ Lift All (+0.5m)"))
        {
            BatchLiftRails(0.5f);
        }
        if (GUILayout.Button("⬇️ Lower All (-0.5m)"))
        {
            BatchLiftRails(-0.5f);
        }
        EditorGUILayout.EndHorizontal();
        
        if (GUILayout.Button("🔄 Reverse All"))
        {
            BatchReverseRails();
        }
        
        EditorGUILayout.Space(10);
        
        // Settings adjustment
        EditorGUILayout.LabelField("Batch Settings Adjustment", EditorStyles.boldLabel);
        
        EditorGUILayout.BeginHorizontal();
        float newGrindSpeed = EditorGUILayout.FloatField("Set Grind Speed:", 18f);
        if (GUILayout.Button("Apply to All"))
        {
            BatchSetGrindSpeed(newGrindSpeed);
        }
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.BeginHorizontal();
        int newResolution = EditorGUILayout.IntSlider("Set Resolution:", 50, 10, 200);
        if (GUILayout.Button("Apply to All"))
        {
            BatchSetCurveResolution(newResolution);
        }
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.Space(10);
        
        // Statistics
        if (railsToProcess.Count > 0)
        {
            EditorGUILayout.HelpBox(GetStatistics(), MessageType.None);
        }
    }

    private void AddSelectedRails()
    {
        foreach (GameObject obj in Selection.gameObjects)
        {
            GrindRail rail = obj.GetComponent<GrindRail>();
            if (rail != null && !railsToProcess.Contains(rail))
            {
                railsToProcess.Add(rail);
            }
        }
    }

    private void FindAllRailsInScene()
    {
        railsToProcess.Clear();
        railsToProcess.AddRange(FindObjectsOfType<GrindRail>());
        Debug.Log($"Found {railsToProcess.Count} rails in scene");
    }

    private void BatchGenerateFromMesh()
    {
        int successCount = 0;
        int failCount = 0;
        
        EditorUtility.DisplayProgressBar("Batch Processing", "Generating waypoints...", 0f);
        
        for (int i = 0; i < railsToProcess.Count; i++)
        {
            GrindRail rail = railsToProcess[i];
            if (rail == null) continue;
            
            float progress = (float)i / railsToProcess.Count;
            EditorUtility.DisplayProgressBar("Batch Processing", $"Processing {rail.name}...", progress);
            
            // Skip if rail already has waypoints and recalculate is off
            if (!batchRecalculateCurves && rail.waypoints != null && rail.waypoints.Length > 0)
            {
                Debug.Log($"Skipped {rail.name} (already has waypoints)");
                continue;
            }
            
            MeshFilter meshFilter = rail.GetComponent<MeshFilter>();
            if (meshFilter == null)
            {
                meshFilter = rail.GetComponentInChildren<MeshFilter>();
            }
            
            if (meshFilter == null || meshFilter.sharedMesh == null)
            {
                Debug.LogWarning($"Skipped {rail.name} (no mesh found)");
                failCount++;
                continue;
            }
            
            try
            {
                GenerateWaypointsForRail(rail, meshFilter);
                successCount++;
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Failed to process {rail.name}: {e.Message}");
                failCount++;
            }
        }
        
        EditorUtility.ClearProgressBar();
        
        string message = $"Batch processing complete!\n\nSuccess: {successCount}\nFailed: {failCount}";
        EditorUtility.DisplayDialog("Batch Complete", message, "OK");
    }

    private void GenerateWaypointsForRail(GrindRail rail, MeshFilter meshFilter)
    {
        Undo.RecordObject(rail, "Batch Generate Waypoints");
        
        // Use similar logic to GrindRailEditor
        Mesh mesh = meshFilter.sharedMesh;
        Transform meshTransform = meshFilter.transform;
        
        // Get all vertices in world space
        Vector3[] localVertices = mesh.vertices;
        List<Vector3> worldVertices = new List<Vector3>();
        
        foreach (Vector3 v in localVertices)
        {
            worldVertices.Add(meshTransform.TransformPoint(v));
        }
        
        List<Vector3> spinePoints = ExtractSpinePath(worldVertices, batchWaypointCount);
        CreateWaypointObjects(rail, spinePoints, batchHeightOffset);
        
        EditorUtility.SetDirty(rail);
    }

    private List<Vector3> ExtractSpinePath(List<Vector3> vertices, int targetCount)
    {
        if (vertices.Count == 0) return new List<Vector3>();
        
        Bounds bounds = new Bounds(vertices[0], Vector3.zero);
        foreach (Vector3 v in vertices)
        {
            bounds.Encapsulate(v);
        }
        
        Vector3 size = bounds.size;
        int primaryAxis = 0;
        if (size.y > size.x && size.y > size.z) primaryAxis = 1;
        else if (size.z > size.x && size.z > size.y) primaryAxis = 2;
        
        vertices.Sort((a, b) => GetAxisValue(a, primaryAxis).CompareTo(GetAxisValue(b, primaryAxis)));
        
        List<Vector3> spinePoints = new List<Vector3>();
        int verticesPerSegment = Mathf.Max(1, vertices.Count / targetCount);
        
        for (int i = 0; i < targetCount; i++)
        {
            int startIdx = i * verticesPerSegment;
            int endIdx = Mathf.Min(startIdx + verticesPerSegment, vertices.Count);
            
            if (startIdx >= vertices.Count) break;
            
            Vector3 centroid = Vector3.zero;
            int count = 0;
            
            for (int j = startIdx; j < endIdx; j++)
            {
                centroid += vertices[j];
                count++;
            }
            
            if (count > 0)
            {
                centroid /= count;
                spinePoints.Add(centroid);
            }
        }
        
        return SmoothPath(spinePoints);
    }

    private float GetAxisValue(Vector3 v, int axis)
    {
        switch (axis)
        {
            case 0: return v.x;
            case 1: return v.y;
            case 2: return v.z;
            default: return v.x;
        }
    }

    private List<Vector3> SmoothPath(List<Vector3> points)
    {
        if (points.Count < 3) return points;
        
        List<Vector3> smoothed = new List<Vector3>();
        smoothed.Add(points[0]);
        
        for (int i = 1; i < points.Count - 1; i++)
        {
            Vector3 smoothPoint = (points[i - 1] + points[i] + points[i + 1]) / 3f;
            smoothed.Add(smoothPoint);
        }
        
        smoothed.Add(points[points.Count - 1]);
        return smoothed;
    }

    private void CreateWaypointObjects(GrindRail rail, List<Vector3> positions, float yOffset)
    {
        // Clear existing waypoints
        if (rail.waypoints != null)
        {
            foreach (Transform wp in rail.waypoints)
            {
                if (wp != null)
                {
                    Undo.DestroyObjectImmediate(wp.gameObject);
                }
            }
        }
        
        Transform[] newWaypoints = new Transform[positions.Count];
        
        for (int i = 0; i < positions.Count; i++)
        {
            GameObject wpObj = new GameObject($"Waypoint_{i}");
            Undo.RegisterCreatedObjectUndo(wpObj, "Create Waypoint");
            
            wpObj.transform.position = positions[i] + Vector3.up * yOffset;
            wpObj.transform.SetParent(rail.transform);
            
            newWaypoints[i] = wpObj.transform;
        }
        
        rail.waypoints = newWaypoints;
    }

    private void BatchRecalculateCurves()
    {
        int count = 0;
        
        foreach (GrindRail rail in railsToProcess)
        {
            if (rail == null) continue;
            
            // Trigger recalculation
            EditorUtility.SetDirty(rail);
            count++;
        }
        
        Debug.Log($"Recalculated {count} rail curves");
    }

    private void BatchLiftRails(float amount)
    {
        foreach (GrindRail rail in railsToProcess)
        {
            if (rail == null || rail.waypoints == null) continue;
            
            Undo.RecordObject(rail, "Batch Lift Rails");
            
            foreach (Transform wp in rail.waypoints)
            {
                if (wp != null)
                {
                    Undo.RecordObject(wp, "Lift Waypoint");
                    wp.position += Vector3.up * amount;
                }
            }
            
            EditorUtility.SetDirty(rail);
        }
        
        Debug.Log($"Lifted {railsToProcess.Count} rails by {amount}m");
    }

    private void BatchReverseRails()
    {
        foreach (GrindRail rail in railsToProcess)
        {
            if (rail == null || rail.waypoints == null || rail.waypoints.Length < 2) continue;
            
            Undo.RecordObject(rail, "Batch Reverse Rails");
            
            System.Array.Reverse(rail.waypoints);
            
            for (int i = 0; i < rail.waypoints.Length; i++)
            {
                if (rail.waypoints[i] != null)
                {
                    rail.waypoints[i].gameObject.name = $"Waypoint_{i}";
                }
            }
            
            EditorUtility.SetDirty(rail);
        }
        
        Debug.Log($"Reversed {railsToProcess.Count} rails");
    }

    private void BatchSetGrindSpeed(float speed)
    {
        foreach (GrindRail rail in railsToProcess)
        {
            if (rail == null) continue;
            
            Undo.RecordObject(rail, "Batch Set Grind Speed");
            rail.grindSpeed = speed;
            EditorUtility.SetDirty(rail);
        }
        
        Debug.Log($"Set grind speed to {speed} for {railsToProcess.Count} rails");
    }

    private void BatchSetCurveResolution(int resolution)
    {
        foreach (GrindRail rail in railsToProcess)
        {
            if (rail == null) continue;
            
            Undo.RecordObject(rail, "Batch Set Curve Resolution");
            rail.curveResolution = resolution;
            EditorUtility.SetDirty(rail);
        }
        
        Debug.Log($"Set curve resolution to {resolution} for {railsToProcess.Count} rails");
    }

    private string GetStatistics()
    {
        int totalWaypoints = 0;
        int railsWithWaypoints = 0;
        float totalLength = 0f;
        
        foreach (GrindRail rail in railsToProcess)
        {
            if (rail == null) continue;
            
            if (rail.waypoints != null && rail.waypoints.Length > 0)
            {
                totalWaypoints += rail.waypoints.Length;
                railsWithWaypoints++;
            }
            
            totalLength += rail.GetRailLength();
        }
        
        return $"Statistics:\n" +
               $"Total Rails: {railsToProcess.Count}\n" +
               $"Rails with Waypoints: {railsWithWaypoints}\n" +
               $"Total Waypoints: {totalWaypoints}\n" +
               $"Total Rail Length: {totalLength:F1}m";
    }
}