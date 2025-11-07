using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;

[CustomEditor(typeof(GrindRail))]
public class GrindRailEditor : Editor
{
    private GrindRail rail;
    
    // Settings for auto-generation
    private int waypointCount = 10;
    private float heightOffset = 0f;
    private bool useVertexSampling = true;
    
    // Collider generation settings
    private bool autoGenerateColliders = true;
    private float triggerColliderScale = 1.3f;

    private void OnEnable()
    {
        rail = (GrindRail)target;
    }

    public override void OnInspectorGUI()
    {
        // Draw default inspector
        DrawDefaultInspector();
        
        EditorGUILayout.Space(20);
        EditorGUILayout.LabelField("Rail Generation Tools", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox("Generate waypoints automatically from mesh geometry", MessageType.Info);
        
        EditorGUILayout.Space(10);
        
        // Auto-generation settings
        EditorGUILayout.LabelField("Auto-Generation Settings", EditorStyles.boldLabel);
        waypointCount = EditorGUILayout.IntSlider("Waypoint Count", waypointCount, 2, 50);
        heightOffset = EditorGUILayout.Slider("Height Offset", heightOffset, -2f, 2f);
        useVertexSampling = EditorGUILayout.Toggle("Use Vertex Sampling", useVertexSampling);
        
        EditorGUILayout.Space(5);
        
        // Collider generation settings
        EditorGUILayout.LabelField("Collider Settings", EditorStyles.boldLabel);
        autoGenerateColliders = EditorGUILayout.Toggle("Auto-Generate Colliders", autoGenerateColliders);
        if (autoGenerateColliders)
        {
            EditorGUI.indentLevel++;
            triggerColliderScale = EditorGUILayout.Slider("Trigger Size Multiplier", triggerColliderScale, 1.1f, 2.0f);
            EditorGUI.indentLevel--;
        }
        
        EditorGUILayout.Space(5);
        
        // Buttons
        if (GUILayout.Button("🎯 Generate from Mesh", GUILayout.Height(35)))
        {
            GenerateWaypointsFromMesh();
        }
        
        if (GUILayout.Button("📏 Generate from Bounds", GUILayout.Height(30)))
        {
            GenerateWaypointsFromBounds();
        }
        
        EditorGUILayout.Space(10);
        
        // Manual tools
        EditorGUILayout.LabelField("Manual Tools", EditorStyles.boldLabel);
        
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("➕ Add Waypoint at End"))
        {
            AddWaypointAtEnd();
        }
        if (GUILayout.Button("➖ Remove Last"))
        {
            RemoveLastWaypoint();
        }
        EditorGUILayout.EndHorizontal();
        
        if (GUILayout.Button("🧹 Clear All Waypoints"))
        {
            if (EditorUtility.DisplayDialog("Clear Waypoints?", 
                "Are you sure you want to remove all waypoints?", "Yes", "Cancel"))
            {
                ClearWaypoints();
            }
        }
        
        EditorGUILayout.Space(10);
        
        // Refinement tools
        EditorGUILayout.LabelField("Refinement Tools", EditorStyles.boldLabel);
        
        if (GUILayout.Button("🔄 Reverse Direction"))
        {
            ReverseWaypoints();
        }
        
        if (GUILayout.Button("⬆️ Lift All Waypoints"))
        {
            LiftWaypoints(0.5f);
        }
        
        if (GUILayout.Button("⬇️ Lower All Waypoints"))
        {
            LiftWaypoints(-0.5f);
        }
        
        EditorGUILayout.Space(10);
        
        // Collider tools
        EditorGUILayout.LabelField("Collider Tools", EditorStyles.boldLabel);
        
        if (GUILayout.Button("🎯 Generate Colliders"))
        {
            GenerateColliders();
        }
        
        if (GUILayout.Button("🗑️ Remove All Colliders"))
        {
            RemoveAllColliders();
        }
        
        EditorGUILayout.Space(10);
        
        // Info display
        if (rail.waypoints != null && rail.waypoints.Length > 0)
        {
            EditorGUILayout.HelpBox($"Current waypoints: {rail.waypoints.Length}\nRail length: {rail.GetRailLength():F2}m", MessageType.None);
        }
    }

    private void GenerateWaypointsFromMesh()
    {
        MeshFilter meshFilter = rail.GetComponent<MeshFilter>();
        
        if (meshFilter == null)
        {
            // Check children
            meshFilter = rail.GetComponentInChildren<MeshFilter>();
        }
        
        if (meshFilter == null || meshFilter.sharedMesh == null)
        {
            EditorUtility.DisplayDialog("Error", 
                "No MeshFilter found! Please ensure this GameObject or its children have a MeshFilter component.", 
                "OK");
            return;
        }

        Undo.RecordObject(rail, "Generate Waypoints from Mesh");

        if (useVertexSampling)
        {
            GenerateFromVertexAnalysis(meshFilter);
        }
        else
        {
            GenerateFromBoundsAnalysis(meshFilter);
        }

        // Generate colliders after waypoints are created
        GenerateColliders();

        EditorUtility.SetDirty(rail);
        
        Debug.Log($"Generated waypoints and colliders for {rail.gameObject.name}");
    }

    private void GenerateFromVertexAnalysis(MeshFilter meshFilter)
    {
        Mesh mesh = meshFilter.sharedMesh;
        Transform meshTransform = meshFilter.transform;
        
        // Get all vertices in world space
        Vector3[] localVertices = mesh.vertices;
        List<Vector3> worldVertices = new List<Vector3>();
        
        foreach (Vector3 v in localVertices)
        {
            worldVertices.Add(meshTransform.TransformPoint(v));
        }

        // Find the "spine" of the rail by analyzing vertex distribution
        List<Vector3> spinePoints = ExtractSpinePath(worldVertices, waypointCount);
        
        // Create waypoint GameObjects
        CreateWaypointObjects(spinePoints, heightOffset);
    }

    private List<Vector3> ExtractSpinePath(List<Vector3> vertices, int targetCount)
    {
        if (vertices.Count == 0) return new List<Vector3>();

        // Find the bounds to determine the main axis
        Bounds bounds = new Bounds(vertices[0], Vector3.zero);
        foreach (Vector3 v in vertices)
        {
            bounds.Encapsulate(v);
        }

        // Determine primary axis (the longest dimension)
        Vector3 size = bounds.size;
        int primaryAxis = 0; // 0=X, 1=Y, 2=Z
        if (size.y > size.x && size.y > size.z) primaryAxis = 1;
        else if (size.z > size.x && size.z > size.y) primaryAxis = 2;

        // Sort vertices along primary axis
        vertices.Sort((a, b) => GetAxisValue(a, primaryAxis).CompareTo(GetAxisValue(b, primaryAxis)));

        // Divide into segments and find center of each segment
        List<Vector3> spinePoints = new List<Vector3>();
        int verticesPerSegment = Mathf.Max(1, vertices.Count / targetCount);

        for (int i = 0; i < targetCount; i++)
        {
            int startIdx = i * verticesPerSegment;
            int endIdx = Mathf.Min(startIdx + verticesPerSegment, vertices.Count);
            
            if (startIdx >= vertices.Count) break;

            // Find centroid of this segment
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

        // Smooth the path
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
        smoothed.Add(points[0]); // Keep first point

        // Simple averaging smoothing
        for (int i = 1; i < points.Count - 1; i++)
        {
            Vector3 smoothPoint = (points[i - 1] + points[i] + points[i + 1]) / 3f;
            smoothed.Add(smoothPoint);
        }

        smoothed.Add(points[points.Count - 1]); // Keep last point
        
        return smoothed;
    }

    private void GenerateFromBoundsAnalysis(MeshFilter meshFilter)
    {
        Renderer renderer = meshFilter.GetComponent<Renderer>();
        if (renderer == null)
        {
            EditorUtility.DisplayDialog("Error", "No Renderer found!", "OK");
            return;
        }

        Bounds bounds = renderer.bounds;
        
        // Determine primary axis
        Vector3 size = bounds.size;
        Vector3 start = bounds.min;
        Vector3 end = bounds.max;
        
        // Use the longest axis
        if (size.x >= size.y && size.x >= size.z)
        {
            // X axis is primary
            start.y = bounds.center.y;
            start.z = bounds.center.z;
            end.y = bounds.center.y;
            end.z = bounds.center.z;
        }
        else if (size.z >= size.x && size.z >= size.y)
        {
            // Z axis is primary
            start.x = bounds.center.x;
            start.y = bounds.center.y;
            end.x = bounds.center.x;
            end.y = bounds.center.y;
        }
        else
        {
            // Y axis is primary
            start.x = bounds.center.x;
            start.z = bounds.center.z;
            end.x = bounds.center.x;
            end.z = bounds.center.z;
        }

        // Generate points along the axis
        List<Vector3> points = new List<Vector3>();
        for (int i = 0; i < waypointCount; i++)
        {
            float t = i / (float)(waypointCount - 1);
            points.Add(Vector3.Lerp(start, end, t));
        }

        CreateWaypointObjects(points, heightOffset);
    }

    private void GenerateWaypointsFromBounds()
    {
        Renderer renderer = rail.GetComponent<Renderer>();
        
        if (renderer == null)
        {
            renderer = rail.GetComponentInChildren<Renderer>();
        }
        
        if (renderer == null)
        {
            EditorUtility.DisplayDialog("Error", 
                "No Renderer found! Please ensure this GameObject or its children have a Renderer component.", 
                "OK");
            return;
        }

        Undo.RecordObject(rail, "Generate Waypoints from Bounds");

        Bounds bounds = renderer.bounds;
        
        // Simple method: create waypoints along the longest axis
        Vector3 size = bounds.size;
        Vector3 start, end;
        
        if (size.x >= size.y && size.x >= size.z)
        {
            // Longest axis is X
            start = new Vector3(bounds.min.x, bounds.center.y + heightOffset, bounds.center.z);
            end = new Vector3(bounds.max.x, bounds.center.y + heightOffset, bounds.center.z);
        }
        else if (size.z >= size.x && size.z >= size.y)
        {
            // Longest axis is Z
            start = new Vector3(bounds.center.x, bounds.center.y + heightOffset, bounds.min.z);
            end = new Vector3(bounds.center.x, bounds.center.y + heightOffset, bounds.max.z);
        }
        else
        {
            // Longest axis is Y
            start = new Vector3(bounds.center.x, bounds.min.y + heightOffset, bounds.center.z);
            end = new Vector3(bounds.center.x, bounds.max.y + heightOffset, bounds.center.z);
        }

        List<Vector3> points = new List<Vector3>();
        for (int i = 0; i < waypointCount; i++)
        {
            float t = i / (float)(waypointCount - 1);
            points.Add(Vector3.Lerp(start, end, t));
        }

        CreateWaypointObjects(points, heightOffset);
        
        // Generate colliders after waypoints are created
        GenerateColliders();
        
        EditorUtility.SetDirty(rail);
    }

    private void CreateWaypointObjects(List<Vector3> positions, float yOffset)
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

        // Create new waypoint GameObjects
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
        
        Debug.Log($"Generated {positions.Count} waypoints for {rail.gameObject.name}");
    }

    private void AddWaypointAtEnd()
    {
        Undo.RecordObject(rail, "Add Waypoint");

        List<Transform> waypointsList = new List<Transform>(rail.waypoints ?? new Transform[0]);
        
        Vector3 newPosition = Vector3.zero;
        
        if (waypointsList.Count > 0)
        {
            // Add after the last waypoint
            Transform last = waypointsList[waypointsList.Count - 1];
            if (waypointsList.Count > 1)
            {
                Transform secondLast = waypointsList[waypointsList.Count - 2];
                Vector3 direction = (last.position - secondLast.position).normalized;
                newPosition = last.position + direction * 2f;
            }
            else
            {
                newPosition = last.position + Vector3.forward * 2f;
            }
        }
        else
        {
            newPosition = rail.transform.position;
        }

        GameObject wpObj = new GameObject($"Waypoint_{waypointsList.Count}");
        Undo.RegisterCreatedObjectUndo(wpObj, "Create Waypoint");
        
        wpObj.transform.position = newPosition;
        wpObj.transform.SetParent(rail.transform);
        
        waypointsList.Add(wpObj.transform);
        rail.waypoints = waypointsList.ToArray();
        
        EditorUtility.SetDirty(rail);
        Selection.activeGameObject = wpObj;
    }

    private void RemoveLastWaypoint()
    {
        if (rail.waypoints == null || rail.waypoints.Length == 0) return;

        Undo.RecordObject(rail, "Remove Waypoint");

        Transform toRemove = rail.waypoints[rail.waypoints.Length - 1];
        
        List<Transform> waypointsList = new List<Transform>(rail.waypoints);
        waypointsList.RemoveAt(waypointsList.Count - 1);
        rail.waypoints = waypointsList.ToArray();
        
        if (toRemove != null)
        {
            Undo.DestroyObjectImmediate(toRemove.gameObject);
        }
        
        EditorUtility.SetDirty(rail);
    }

    private void ClearWaypoints()
    {
        if (rail.waypoints == null) return;

        Undo.RecordObject(rail, "Clear Waypoints");

        foreach (Transform wp in rail.waypoints)
        {
            if (wp != null)
            {
                Undo.DestroyObjectImmediate(wp.gameObject);
            }
        }

        rail.waypoints = new Transform[0];
        EditorUtility.SetDirty(rail);
    }

    private void ReverseWaypoints()
    {
        if (rail.waypoints == null || rail.waypoints.Length < 2) return;

        Undo.RecordObject(rail, "Reverse Waypoints");

        System.Array.Reverse(rail.waypoints);
        
        // Rename them
        for (int i = 0; i < rail.waypoints.Length; i++)
        {
            if (rail.waypoints[i] != null)
            {
                rail.waypoints[i].gameObject.name = $"Waypoint_{i}";
            }
        }
        
        EditorUtility.SetDirty(rail);
    }

    private void LiftWaypoints(float amount)
    {
        if (rail.waypoints == null) return;

        Undo.RecordObject(rail, "Lift Waypoints");

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

    private void GenerateColliders()
    {
        if (!autoGenerateColliders)
        {
            Debug.Log("Auto-generate colliders is disabled. Enable it in settings to generate colliders.");
            return;
        }

        if (rail.waypoints == null || rail.waypoints.Length < 2)
        {
            EditorUtility.DisplayDialog("Error", 
                "Rail must have at least 2 waypoints to generate colliders!", 
                "OK");
            return;
        }

        // Remove existing box colliders
        RemoveAllColliders();

        Undo.RecordObject(rail.gameObject, "Generate Rail Colliders");

        // Calculate bounds from waypoints
        Bounds railBounds = CalculateWaypointBounds();

        // Create collision box collider (normal, for physical blocking)
        BoxCollider collisionCollider = Undo.AddComponent<BoxCollider>(rail.gameObject);
        collisionCollider.center = railBounds.center - rail.transform.position;
        collisionCollider.size = railBounds.size;
        collisionCollider.isTrigger = false;

        // Create trigger box collider (for grind detection, slightly larger)
        BoxCollider triggerCollider = Undo.AddComponent<BoxCollider>(rail.gameObject);
        triggerCollider.center = railBounds.center - rail.transform.position;
        triggerCollider.size = railBounds.size * triggerColliderScale;
        triggerCollider.isTrigger = true;

        EditorUtility.SetDirty(rail.gameObject);
        
        Debug.Log($"Generated colliders for {rail.gameObject.name}:\n" +
                  $"- Collision Box: {railBounds.size}\n" +
                  $"- Trigger Box: {railBounds.size * triggerColliderScale}");
    }

    private Bounds CalculateWaypointBounds()
    {
        // Initialize bounds with first waypoint position
        Bounds bounds = new Bounds(rail.waypoints[0].position, Vector3.zero);

        // Expand to include all waypoints
        foreach (Transform waypoint in rail.waypoints)
        {
            if (waypoint != null)
            {
                bounds.Encapsulate(waypoint.position);
            }
        }

        // Add some padding to the bounds
        // Expand slightly in all directions for better coverage
        Vector3 padding = new Vector3(0.5f, 0.5f, 0.5f);
        bounds.Expand(padding);

        return bounds;
    }

    private void RemoveAllColliders()
    {
        BoxCollider[] existingColliders = rail.GetComponents<BoxCollider>();
        
        if (existingColliders.Length > 0)
        {
            Undo.RecordObject(rail.gameObject, "Remove Rail Colliders");
            
            foreach (BoxCollider col in existingColliders)
            {
                Undo.DestroyObjectImmediate(col);
            }
            
            EditorUtility.SetDirty(rail.gameObject);
            Debug.Log($"Removed {existingColliders.Length} box collider(s) from {rail.gameObject.name}");
        }
    }

    // Scene view handles for easier waypoint manipulation
    private void OnSceneGUI()
    {
        if (rail.waypoints == null) return;

        // Draw handles for each waypoint
        for (int i = 0; i < rail.waypoints.Length; i++)
        {
            if (rail.waypoints[i] == null) continue;

            EditorGUI.BeginChangeCheck();
            Vector3 newPos = Handles.PositionHandle(rail.waypoints[i].position, Quaternion.identity);
            
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(rail.waypoints[i], "Move Waypoint");
                rail.waypoints[i].position = newPos;
                EditorUtility.SetDirty(rail);
            }

            // Draw labels
            Handles.Label(rail.waypoints[i].position + Vector3.up * 0.5f, 
                $"WP {i}", 
                new GUIStyle() { normal = new GUIStyleState() { textColor = Color.white } });
        }
    }
}