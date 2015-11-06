using OnePathfinding;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

/// <summary>
/// A custom editor for the GridManager
/// </summary>
[CustomEditor(typeof(GridManager))]
public class GMEditor : Editor
{
    /// <summary>
    /// Last time the layer mask was updated.
    /// </summary>
    public static long lastUpdateTick;

    /// <summary>
    /// The list of names for the layers.
    /// </summary>
    public static string[] layerNames;

    /// <summary>
    /// The number for each layer.
    /// </summary>
    public static List<int> layerNumbers;

    /// <summary>
    /// The name for each layer.
    /// </summary>
    public static List<string> layers;

    /// <summary>
    /// Bool array toggling whether to show grid settings or not.
    /// </summary>
    public bool[] current;

    /// <summary>
    /// An instance to this object.
    /// </summary>
    private GridManager GM;

    /// <summary>
    /// Used as a custom editor field for layer masks.
    /// </summary>
    /// <param name="label"></param>
    /// <param name="selected"></param>
    /// <param name="showSpecial"></param>
    /// <returns></returns>
    public LayerMask LayerMaskField(string label, LayerMask selected, bool showSpecial)
    {
        //Unity 3.5 and up

        if (layers == null || (System.DateTime.Now.Ticks - lastUpdateTick > 10000000L && Event.current.type == EventType.Layout))
        {
            lastUpdateTick = System.DateTime.Now.Ticks;
            if (layers == null)
            {
                layers = new List<string>();
                layerNumbers = new List<int>();
                layerNames = new string[4];
            }
            else
            {
                layers.Clear();
                layerNumbers.Clear();
            }

            int emptyLayers = 0;
            for (int i = 0; i < 32; i++)
            {
                string layerName = LayerMask.LayerToName(i);

                if (layerName != "")
                {
                    for (; emptyLayers > 0; emptyLayers--)
                        layers.Add("Layer " + (i - emptyLayers));
                    layerNumbers.Add(i);
                    layers.Add(layerName);
                }
                else
                {
                    emptyLayers++;
                }
            }

            if (layerNames.Length != layers.Count)
            {
                layerNames = new string[layers.Count];
            }
            for (int i = 0; i < layerNames.Length; i++) layerNames[i] = layers[i];
        }

        selected.value = EditorGUILayout.MaskField(label, selected.value, layerNames);

        return selected;
    }

    /// <summary>
    /// Used to draw the inspectorGUI.
    /// </summary>
    public override void OnInspectorGUI()
    {
        GM = (GridManager)target;
        EditorUtility.SetDirty(target);
        if (GM == null)
        {
            return;
        }
        else if (GM.grid == null)
        {
            GM.grid = new List<GridGraph>();
        }

        if (current == null || current.Length != GM.grid.Count)
        {
            current = new bool[GM.grid.Count];
        }
        EditorGUILayout.LabelField("Information:", EditorStyles.boldLabel);
        EditorGUILayout.LabelField(new GUIContent("Queue length: " + GM.queueLength, "This shows the current length of the queue."));
        EditorGUILayout.LabelField(new GUIContent("Currently active AIs: " + FindObjectsOfType<AdvancedAI>().Length.ToString(), "This label shows how many AI components is found in the scene"));
        GUILayout.Space(10f);
        EditorGUILayout.LabelField("Settings:", EditorStyles.boldLabel);
        GM.DebugLvl = (GridManager.DebugLevel)EditorGUILayout.EnumPopup("Debug Level: ", GM.DebugLvl);
        GM.ShowGizmos = EditorGUILayout.Toggle("Show Gizmo's: ", GM.ShowGizmos);
        if (GM.ShowGizmos)
        {
            GM.ShowFlockColor = EditorGUILayout.Toggle(new GUIContent("Show Flock Color", "Each flock has a color for their gizmo's, making it easier to see who's who."), GM.ShowFlockColor);
        }
        GM.ShowPaths = EditorGUILayout.Toggle(new GUIContent("Show Paths", "Show each AIs current path"), GM.ShowPaths);

        GUILayout.Space(10f);
        EditorGUILayout.LabelField("Grids:", EditorStyles.boldLabel);
        for (int i = 0; i < GM.grid.Count; i++)
        {
            current[i] = EditorGUILayout.Foldout(current[i], GM.grid[i].name);
            if (current[i])
            {
                GM.grid[i].name = EditorGUILayout.TextField("Name: ", GM.grid[i].name);
                GM.grid[i].ScanOnLoad = EditorGUILayout.Toggle("Scan On Load: ", GM.grid[i].ScanOnLoad);
                GM.grid[i].offset = EditorGUILayout.Vector3Field("Offset: ", GM.grid[i].offset);
                GM.grid[i]._gridType = (GridGraph.GridType)EditorGUILayout.EnumPopup("Grid Type: ", GM.grid[i]._gridType);
                if (GM.grid[i].gridType == GridGraph.GridType.Plane)
                {
                    GM.grid[i].WorldSize = EditorGUILayout.Vector2Field("World Size: ", GM.grid[i].WorldSize);
                    GM.grid[i].NodeRadius = EditorGUILayout.FloatField("Node Radius: ", GM.grid[i].NodeRadius);
                }
                else if (GM.grid[i].gridType == GridGraph.GridType.Sphere)
                {
                    float x = 1f * EditorGUILayout.IntField(new GUIContent("Node Width: ", "The number of nodes per side of the sphere"), Mathf.RoundToInt(GM.grid[i].WorldSize.x));
                    GM.grid[i].WorldSize = new Vector3(x, 0, 0);
                    GM.grid[i].Radius = EditorGUILayout.IntField("Radius: ", GM.grid[i].Radius);
                }
                GM.grid[i].angleLimit = EditorGUILayout.IntSlider("Max Angle: ", GM.grid[i].angleLimit, 0, 90);
                GM.grid[i].UnWalkableMask = LayerMaskField("Unwalkable Layer(s):", GM.grid[i].UnWalkableMask, true);
                GUILayout.Space(10);
                if (GUILayout.Button("Scan Grid"))
                {
                    GridManager.ScanGrid(GM.grid[i]);
                }
                GUI.backgroundColor = Color.red;
                if (GUILayout.Button("Delete grid"))
                {
                    GM.grid.RemoveAt(i);
                }
                GUI.backgroundColor = Color.white;
                GUILayout.Space(10);
            }
        }
        if (GUILayout.Button("Add Grid"))
        {
            GM.grid.Add(new GridGraph());
            GridGraph g = GM.grid[GM.grid.Count - 1];
            if (g == null)
            {
                Debug.LogError("An error happened, please contact the developer, CODE: G" + (GM.grid.Count - 1));
                return;
            }
            g.name = "Grid #" + GM.grid.Count;
        }
        GUILayout.Space(20);
        if (GUILayout.Button("Scan Grids"))
        {
            foreach (GridGraph grid in GM.grid)
            {
                GridManager.ScanGrid(grid);
            }
        }
    }

    /// <summary>
    /// Update the inspector.
    /// </summary>
    public void OnInspectorUpdate()
    {
        // This will only get called 10 times per second.
        Repaint();
    }
}