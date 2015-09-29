using Pathfinding;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(GridManager))]
public class GMEditor : Editor
{
    #region Fields

    public static long lastUpdateTick;
    public static string[] layerNames;
    public static List<int> layerNumbers;
    public static List<string> layers;
    public bool[] current;
    private GridManager GM;

    #endregion Fields

    #region Methods

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

    public override void OnInspectorGUI()
    {
        GM = (GridManager)target;
        if (current == null)
        {
            current = new bool[GM.grid.Count];
        }

        EditorGUILayout.LabelField(new GUIContent("Currently active AIs: " + FindObjectsOfType<AdvancedAI>().Length.ToString(), "This label shows how many AI components is found in the scene"));
        EditorGUILayout.LabelField("Scanning: " + GridManager.isScanning);
        GM.DebugLvl = (GridManager.DebugLevel)EditorGUILayout.EnumPopup("Debug Level: ", GM.DebugLvl);
        GM.ShowGizmos = EditorGUILayout.Toggle("Show Gizmos: ", GM.ShowGizmos);
        GM.ShowFlockColor = EditorGUILayout.Toggle(new GUIContent("Show Flock Color", "Each flock has a color for their gizmos, making it easier to see who's who."), GM.ShowFlockColor);
        GM.ShowPaths = EditorGUILayout.Toggle(new GUIContent("Show Paths", "Show each AIs current path"), GM.ShowPaths);

        GUILayout.Space(10f);
        EditorGUILayout.LabelField("Grids:", EditorStyles.boldLabel);
        if (current.Length != GM.grid.Count)
        {
            current = new bool[GM.grid.Count];
        }

        for (int i = 0; i < GM.grid.Count; i++)
        {
            current[i] = EditorGUILayout.Foldout(current[i], GM.grid[i].name);
            if (current[i])
            {
                GM.grid[i].name = EditorGUILayout.TextField("Name: ", GM.grid[i].name);
                GM.grid[i].center = EditorGUILayout.Vector3Field("Center", GM.grid[i].center);
                GM.grid[i].WorldSize = EditorGUILayout.Vector2Field("World Size", GM.grid[i].WorldSize);
                GM.grid[i].NodeRadius = EditorGUILayout.FloatField("Node Radius", GM.grid[i].NodeRadius);
                GM.grid[i].angleLimit = EditorGUILayout.IntSlider("Max Angle", GM.grid[i].angleLimit, 0, 90);
                GM.grid[i].WalkableMask = LayerMaskField("Walkable Layer(s):", GM.grid[i].WalkableMask, true);
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
                Debug.Log(GM.grid.Count - 1);
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

    private void OnInspectorUpdate()
    {
        Repaint();
    }

    #endregion Methods
}