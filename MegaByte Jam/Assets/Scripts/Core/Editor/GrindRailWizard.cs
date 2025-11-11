using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

/// <summary>
/// "we love to grind in wizard time
/// happy wizarads like to grind" - Jasmine
/// </summary>
public class GrindRailWizard : EditorWindow
{
    private GrindRail targetRail;
    private bool placementMode = false;
    private List<Vector3> tempWaypoints = new List<Vector3>();
    private float placementHeight = 0.5f;
    private bool snapToSurface = true;
    private float snapDistance = 10f;

    [MenuItem("Tools/Grind Rail Wizard")]
    public static void ShowWindow()
    {
        GetWindow<GrindRailWizard>("Grind Rail Wizard");
    }

    private void OnEnable()
    {
        SceneView.duringSceneGui += OnSceneGUI;
    }

    private void OnDisable()
    {
        SceneView.duringSceneGui -= OnSceneGUI;
        placementMode = false;
    }

    private void OnGUI()
    {
        GUILayout.Label("Grind Rail Wizard", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox("Interactive tool for creating grind rails by clicking on meshes in the Scene view.", MessageType.Info);

        EditorGUILayout.Space(10);

        // Target rail selection
        EditorGUILayout.LabelField("Setup", EditorStyles.boldLabel);
        targetRail = (GrindRail)EditorGUILayout.ObjectField("Target Rail", targetRail, typeof(GrindRail), true);

        if (targetRail == null && GUILayout.Button("Create New Rail"))
        {
            CreateNewRail();
        }

        EditorGUILayout.Space(10);

        // Placement settings
        EditorGUILayout.LabelField("Placement Settings", EditorStyles.boldLabel);
        snapToSurface = EditorGUILayout.Toggle("Snap to Surface", snapToSurface);

        if (snapToSurface)
        {
            snapDistance = EditorGUILayout.Slider("Snap Distance", snapDistance, 1f, 50f);
        }
        else
        {
            placementHeight = EditorGUILayout.Slider("Placement Height", placementHeight, -5f, 5f);
        }

        EditorGUILayout.Space(10);

        // Placement mode toggle
        EditorGUILayout.LabelField("Interactive Placement", EditorStyles.boldLabel);

        if (!placementMode)
        {
            if (GUILayout.Button("🎨 Start Placement Mode", GUILayout.Height(40)))
            {
                StartPlacementMode();
            }

            EditorGUILayout.HelpBox("Click 'Start Placement Mode', then click in the Scene view to place waypoints along your rail mesh.", MessageType.None);
        }
        else
        {
            EditorGUILayout.HelpBox($"PLACEMENT MODE ACTIVE\nWaypoints placed: {tempWaypoints.Count}\n\nLeft Click: Place waypoint\nCtrl+Z: Undo last\nEsc or button below: Finish", MessageType.Warning);

            if (GUILayout.Button("✅ Finish Placement", GUILayout.Height(40)))
            {
                FinishPlacement();
            }

            if (GUILayout.Button("❌ Cancel", GUILayout.Height(30)))
            {
                CancelPlacement();
            }
        }

        EditorGUILayout.Space(10);

        // Quick generation tools
        EditorGUILayout.LabelField("Quick Generation", EditorStyles.boldLabel);

        if (GUILayout.Button("Generate from Selected Mesh"))
        {
            GenerateFromSelectedMesh();
        }

        if (GUILayout.Button("Generate from Path (Selected Objects)"))
        {
            GenerateFromSelectedObjects();
        }

        EditorGUILayout.Space(10);

        // Template rails
        EditorGUILayout.LabelField("Templates", EditorStyles.boldLabel);

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Straight Rail"))
        {
            CreateTemplateRail(RailTemplate.Straight);
        }
        if (GUILayout.Button("Curved Rail"))
        {
            CreateTemplateRail(RailTemplate.Curved);
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("U-Shaped"))
        {
            CreateTemplateRail(RailTemplate.UShaped);
        }
        if (GUILayout.Button("S-Shaped"))
        {
            CreateTemplateRail(RailTemplate.SShaped);
        }
        EditorGUILayout.EndHorizontal();

        if (GUILayout.Button("Spiral Ramp"))
        {
            CreateTemplateRail(RailTemplate.Spiral);
        }
    }

    private void OnSceneGUI(SceneView sceneView)
    {
        if (!placementMode) return;

        // Draw instructions
        Handles.BeginGUI();
        GUILayout.BeginArea(new Rect(10, 10, 300, 100));
        GUILayout.Box("PLACEMENT MODE\nLeft Click: Place waypoint\nCtrl+Z: Undo\nEsc: Finish", GUILayout.Width(280));
        GUILayout.EndArea();
        Handles.EndGUI();

        // Draw temp waypoints
        Handles.color = Color.yellow;
        for (int i = 0; i < tempWaypoints.Count; i++)
        {
            Handles.SphereHandleCap(0, tempWaypoints[i], Quaternion.identity, 0.2f, EventType.Repaint);
            Handles.Label(tempWaypoints[i] + Vector3.up * 0.3f, $"WP {i}");

            if (i > 0)
            {
                Handles.DrawLine(tempWaypoints[i - 1], tempWaypoints[i]);
            }
        }

        // Handle input
        Event e = Event.current;

        if (e.type == EventType.MouseDown && e.button == 0)
        {
            Ray ray = HandleUtility.GUIPointToWorldRay(e.mousePosition);
            Vector3 placePosition;

            if (snapToSurface)
            {
                RaycastHit hit;
                if (Physics.Raycast(ray, out hit, snapDistance))
                {
                    placePosition = hit.point;
                }
                else
                {
                    // Fallback to plane at last waypoint or origin
                    Plane plane = new Plane(Vector3.up, tempWaypoints.Count > 0 ? tempWaypoints[tempWaypoints.Count - 1] : Vector3.zero);
                    float enter;
                    if (plane.Raycast(ray, out enter))
                    {
                        placePosition = ray.GetPoint(enter);
                    }
                    else
                    {
                        return;
                    }
                }
            }
            else
            {
                // Place on horizontal plane
                Plane plane = new Plane(Vector3.up, Vector3.up * placementHeight);
                float enter;
                if (plane.Raycast(ray, out enter))
                {
                    placePosition = ray.GetPoint(enter);
                }
                else
                {
                    return;
                }
            }

            tempWaypoints.Add(placePosition);
            e.Use();
            sceneView.Repaint();
        }
        else if (e.type == EventType.KeyDown && e.keyCode == KeyCode.Escape)
        {
            FinishPlacement();
            e.Use();
        }
        else if (e.type == EventType.KeyDown && e.keyCode == KeyCode.Z && e.control)
        {
            if (tempWaypoints.Count > 0)
            {
                tempWaypoints.RemoveAt(tempWaypoints.Count - 1);
                e.Use();
                sceneView.Repaint();
            }
        }

        // Prevent selection while in placement mode
        HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));
    }

    private void StartPlacementMode()
    {
        if (targetRail == null)
        {
            EditorUtility.DisplayDialog("No Target", "Please assign a GrindRail first!", "OK");
            return;
        }

        placementMode = true;
        tempWaypoints.Clear();
        SceneView.RepaintAll();
    }

    private void FinishPlacement()
    {
        if (tempWaypoints.Count < 2)
        {
            EditorUtility.DisplayDialog("Not Enough Waypoints",
                "You need at least 2 waypoints to create a rail!", "OK");
            CancelPlacement();
            return;
        }

        Undo.RecordObject(targetRail, "Create Rail from Placement");

        // Clear existing waypoints if any
        if (targetRail.waypoints != null)
        {
            foreach (Transform wp in targetRail.waypoints)
            {
                if (wp != null)
                {
                    Undo.DestroyObjectImmediate(wp.gameObject);
                }
            }
        }

        // Create new waypoint objects
        Transform[] newWaypoints = new Transform[tempWaypoints.Count];
        for (int i = 0; i < tempWaypoints.Count; i++)
        {
            GameObject wpObj = new GameObject($"Waypoint_{i}");
            Undo.RegisterCreatedObjectUndo(wpObj, "Create Waypoint");
            wpObj.transform.position = tempWaypoints[i];
            wpObj.transform.SetParent(targetRail.transform);
            newWaypoints[i] = wpObj.transform;
        }

        targetRail.waypoints = newWaypoints;
        EditorUtility.SetDirty(targetRail);

        placementMode = false;
        tempWaypoints.Clear();
        SceneView.RepaintAll();

        Debug.Log($"Created rail with {newWaypoints.Length} waypoints!");
    }

    private void CancelPlacement()
    {
        placementMode = false;
        tempWaypoints.Clear();
        SceneView.RepaintAll();
    }

    private void CreateNewRail()
    {
        GameObject railObj = new GameObject("New Grind Rail");
        targetRail = railObj.AddComponent<GrindRail>();

        // Position at scene view camera
        SceneView sceneView = SceneView.lastActiveSceneView;
        if (sceneView != null)
        {
            railObj.transform.position = sceneView.camera.transform.position + sceneView.camera.transform.forward * 5f;
        }

        Selection.activeGameObject = railObj;
        Undo.RegisterCreatedObjectUndo(railObj, "Create Grind Rail");
    }

    private void GenerateFromSelectedMesh()
    {
        if (Selection.activeGameObject == null)
        {
            EditorUtility.DisplayDialog("No Selection", "Please select a mesh object!", "OK");
            return;
        }

        MeshFilter meshFilter = Selection.activeGameObject.GetComponent<MeshFilter>();
        if (meshFilter == null)
        {
            EditorUtility.DisplayDialog("No Mesh", "Selected object has no MeshFilter!", "OK");
            return;
        }

        if (targetRail == null)
        {
            CreateNewRail();
        }

        // Use the editor script's generation logic
        GrindRailEditor editor = (GrindRailEditor)Editor.CreateEditor(targetRail);
        // Note: This is a simplified version - the actual implementation would call the editor's methods

        Debug.Log("Use the 'Generate from Mesh' button in the GrindRail Inspector instead!");
    }

    private void GenerateFromSelectedObjects()
    {
        if (Selection.gameObjects.Length < 2)
        {
            EditorUtility.DisplayDialog("Not Enough Objects",
                "Please select at least 2 objects to create a path!", "OK");
            return;
        }

        if (targetRail == null)
        {
            CreateNewRail();
        }

        Undo.RecordObject(targetRail, "Generate from Selection");

        // Sort by name or position
        GameObject[] selected = Selection.gameObjects;
        System.Array.Sort(selected, (a, b) => a.name.CompareTo(b.name));

        Transform[] newWaypoints = new Transform[selected.Length];
        for (int i = 0; i < selected.Length; i++)
        {
            GameObject wpObj = new GameObject($"Waypoint_{i}");
            Undo.RegisterCreatedObjectUndo(wpObj, "Create Waypoint");
            wpObj.transform.position = selected[i].transform.position;
            wpObj.transform.SetParent(targetRail.transform);
            newWaypoints[i] = wpObj.transform;
        }

        targetRail.waypoints = newWaypoints;
        EditorUtility.SetDirty(targetRail);

        Debug.Log($"Created rail from {selected.Length} selected objects!");
    }

    private enum RailTemplate
    {
        Straight,
        Curved,
        UShaped,
        SShaped,
        Spiral
    }

    private void CreateTemplateRail(RailTemplate template)
    {
        GameObject railObj = new GameObject($"{template} Rail");
        GrindRail rail = railObj.AddComponent<GrindRail>();

        List<Vector3> positions = new List<Vector3>();
        Vector3 basePos = Vector3.zero;

        SceneView sceneView = SceneView.lastActiveSceneView;
        if (sceneView != null)
        {
            basePos = sceneView.camera.transform.position + sceneView.camera.transform.forward * 5f;
        }

        switch (template)
        {
            case RailTemplate.Straight:
                positions.Add(basePos);
                positions.Add(basePos + Vector3.forward * 10f);
                break;

            case RailTemplate.Curved:
                for (int i = 0; i <= 5; i++)
                {
                    float t = i / 5f;
                    float x = t * 10f;
                    float z = Mathf.Sin(t * Mathf.PI) * 3f;
                    positions.Add(basePos + new Vector3(x, 0, z));
                }
                break;

            case RailTemplate.UShaped:
                positions.Add(basePos);
                positions.Add(basePos + new Vector3(0, 0, 5));
                positions.Add(basePos + new Vector3(5, 0, 5));
                positions.Add(basePos + new Vector3(5, 0, 0));
                break;

            case RailTemplate.SShaped:
                for (int i = 0; i <= 8; i++)
                {
                    float t = i / 8f;
                    float x = t * 10f;
                    float z = Mathf.Sin(t * Mathf.PI * 2f) * 2f;
                    positions.Add(basePos + new Vector3(x, 0, z));
                }
                break;

            case RailTemplate.Spiral:
                for (int i = 0; i <= 20; i++)
                {
                    float t = i / 20f;
                    float angle = t * Mathf.PI * 4f;
                    float radius = 3f;
                    float x = Mathf.Cos(angle) * radius;
                    float z = Mathf.Sin(angle) * radius;
                    float y = t * 5f;
                    positions.Add(basePos + new Vector3(x, y, z));
                }
                break;
        }

        // Create waypoints
        Transform[] waypoints = new Transform[positions.Count];
        for (int i = 0; i < positions.Count; i++)
        {
            GameObject wpObj = new GameObject($"Waypoint_{i}");
            wpObj.transform.position = positions[i];
            wpObj.transform.SetParent(railObj.transform);
            waypoints[i] = wpObj.transform;
        }

        rail.waypoints = waypoints;

        Selection.activeGameObject = railObj;
        Undo.RegisterCreatedObjectUndo(railObj, $"Create {template} Rail");

        Debug.Log($"Created {template} rail template!");
    }
} 