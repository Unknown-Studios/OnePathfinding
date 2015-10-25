using OnePathfinding;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Debug = UnityEngine.Debug;

/// <summary>
/// Used to manage the grids. (To create them, delete them and set them up)
/// </summary>
[RequireComponent(typeof(Wind))]
public class GridManager : MonoBehaviour
{
    /// <summary>
    /// If the GridManager is scanning a grid.
    /// </summary>
    public static bool isScanning;

    /// <summary>
    /// The level of debugging.
    /// </summary>
    public DebugLevel DebugLvl;

    /// <summary>
    /// The list of grids in this scene.
    /// </summary>
    public List<GridGraph> grid;

    /// <summary>
    /// Whether to show the color of each flock or not.
    /// </summary>
    public bool ShowFlockColor;

    /// <summary>
    /// Whether to show the gizmo's or not (Does not include Paths)
    /// </summary>
    public bool ShowGizmos;

    /// <summary>
    /// Whether to show paths or not.
    /// </summary>
    public bool ShowPaths;

    /// <summary>
    /// Instance to this object.
    /// </summary>
    private static GridManager _instance;

    /// <summary>
    /// The currently processed pathRequest.
    /// </summary>
    private PathRequest currentPathRequest;
    
    /// <summary>
    /// The current queue of PathRequests.
    /// </summary>
    public queue pathRequests = new queue();

    /// <summary>
    /// The level at which debugging will happen.
    /// </summary>
    public enum DebugLevel
    {
        None = 0,
        Low = 1,
        High = 2,
    }

    /// <summary>
    /// Gets the first grid in the grid-array.
    /// </summary>
    public static GridGraph Grid
    {
        get
        {
            if (instance == null)
            {
                return null;
            }
            return instance.grid[0];
        }
    }

    /// <summary>
    /// Gets the full list of grids in the grid-array.
    /// </summary>
    public static List<GridGraph> Grids
    {
        get
        {
            if (instance == null)
            {
                return null;
            }
            return instance.grid;
        }
    }

    /// <summary>
    /// Reference of this object.
    /// </summary>
    public static GridManager instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<GridManager>();
            }
            return _instance;
        }
    }

    /// <summary>
    /// Whether or not to show the gizmo's in the editor.
    /// </summary>
    public static bool ShowGizmo
    {
        get
        {
            return instance.ShowGizmos;
        }
        set
        {
            instance.ShowGizmos = value;
        }
    }

    /// <summary>
    /// Whether or not to show the paths in the editor.
    /// </summary>
    public static bool ShowPath
    {
        get
        {
            if (instance == null)
            {
                return false;
            }
            return instance.ShowPaths;
        }
    }

    /// <summary>
    /// The current length of the queue.
    /// </summary>
    public int queueLength
    {
        get
        {
            return pathRequests.Count;
        }
    }

    /// <summary>
    /// Get the grid with the given index in the grid list
    /// </summary>
    /// <param name="index">The grids index</param>
    /// <returns>Grid</returns>
    public static GridGraph GetGrid(int index)
    {
        if (instance == null)
        {
            return null;
        }
        return instance.grid[index];
    }

    /// <summary>
    /// Request a path from point pathStart to point pathEnd, in a specific grid.
    /// </summary>
    /// <param name="pathStart">The starting point of the path</param>
    /// <param name="pathEnd">The ending point of the path</param>
    /// <param name="callback">A function with the parameters (Path)</param>
    /// <param name="grid">The specific grid you want to find the path on.</param>
    public static void RequestPath(Vector3 pathStart, Vector3 pathEnd, Action<Path> callback, GridGraph grid = null)
    {
        if (grid == null)
        {
            grid = Grid;
        }
        string Name = callback.GetHashCode().ToString();
        PathRequest newRequest = new PathRequest(pathStart, pathEnd, callback, Name, grid);
        PathRequest contains = instance.pathRequests.Contains(Name);
        if (contains != null)
        {
            instance.pathRequests.Remove(contains);
            return;
        }
        else
        {
            instance.pathRequests.Enqueue(newRequest);
            return;
        }
    }

    /// <summary>
    /// Scan a specific grid in the grid array.
    /// </summary>
    /// <param name="grid">The grid that is going to be scanned.</param>
    public static void ScanGrid(GridGraph grid = null)
    {
        if (instance == null)
        {
            Debug.Log("A GridManager wasn't found in the scene!");
            return;
        }
        if (grid == null)
        {
            grid = Grid;
        }
        instance.StartCoroutine(instance.ScanAGrid(grid));
    }

    /// <summary>
    /// Find the quickest path from point 1 to point 2
    /// </summary>
    /// <param name="startPos">The start-position of the path</param>
    /// <param name="targetPos">The target-position for the path</param>
    /// <param name="grid">The grid that it should perform the search on.</param>
    /// <returns></returns>
    private IEnumerator FindThePath(Vector3 startPos, Vector3 targetPos, GridGraph grid)
    {
        if (grid.nodes == null || grid.nodes.Length == 0)
        {
            ScanGrid(grid);
        }

        float max = 300; //Maximum number of nodes, before canceling the path. (So if there isn't a way between 2 points it won't search forever)

        Path p = new Path();
        bool pathSuccess = false;

        Node startNode = grid.NearWalkable(startPos);
        Node targetNode = grid.NearWalkable(targetPos);

        if (startNode == null || targetNode == null)
        {
            OnProccesingDone(p, false);
            yield break;
        }

        if (startNode.Walkable && targetNode.Walkable)
        {
            Heap<Node> open = new Heap<Node>(grid.maxSize);
            HashSet<Node> closed = new HashSet<Node>();
            open.Add(startNode);
            int cur = 0;

            while (open.Count > 0)
            {
                Node currentNode = open.RemoveFirst();
                closed.Add(currentNode);
                cur++;

                if (currentNode == targetNode)
                {
                    pathSuccess = true;
                    break;
                }

                foreach (Node neighbour in grid.GetNeighbours(currentNode))
                {
                    if (neighbour == null || !neighbour.Walkable || closed.Contains(neighbour))
                    {
                        continue;
                    }

                    int newMovementCostToNeighbour = currentNode.gCost + grid.GetDistance(currentNode, neighbour);
                    if (newMovementCostToNeighbour < neighbour.gCost || !open.Contains(neighbour))
                    {
                        neighbour.gCost = newMovementCostToNeighbour;
                        neighbour.hCost = grid.GetDistance(neighbour, targetNode);
                        neighbour.parent = currentNode;

                        if (!open.Contains(neighbour))
                            open.Add(neighbour);
                    }
                }
                if (cur > max)
                {
                    break;
                }
            }
        }
        if (pathSuccess)
        {
            p = grid.RetracePath(startNode, targetNode);
        }
        OnProccesingDone(p, pathSuccess);
    }

    /// <summary>
    /// Used to draw the grids gizmo's.
    /// </summary>
    private void OnDrawGizmos()
    {
        if (grid == null)
        {
            return;
        }
        foreach (GridGraph Grid in grid)
        {
            if (!Grid.Scanning)
            {
                if (Grid.nodes == null)
                {
                    ScanGrid(Grid);
                    return;
                }
                if (DebugLvl == DebugLevel.High)
                {
                    if (Grid.nodes != null)
                    {
                        foreach (Node n in Grid.nodes)
                        {
                            if (n == null)
                            {
                                continue;
                            }
                            if (!n.Walkable)
                            {
                                Gizmos.color = Color.red;
                                Gizmos.DrawCube(n.WorldPosition, Vector3.one * (Grid.NodeRadius * 2f));
                            }
                            else
                            {
                                Gizmos.color = Color.blue;
                                Gizmos.DrawCube(n.WorldPosition, Vector3.one * (Grid.NodeRadius * 2f));
                            }
                        }
                    }
                }
                if (DebugLvl == DebugLevel.Low)
                {
                    if (Grid.nodes != null)
                    {
                        foreach (Node n in Grid.nodes)
                        {
                            if (n == null)
                            {
                                continue;
                            }
                            if (!n.Walkable)
                            {
                                Gizmos.color = Color.red;
                                Gizmos.DrawCube(n.WorldPosition, Vector3.one * (Grid.NodeRadius * 2f));
                            }
                        }
                    }
                }
            }
            Gizmos.color = Color.white;
            Gizmos.DrawWireCube((Grid.offset + (Grid.Vector2ToVector3(Grid.WorldSize) / 2)), Grid.Vector2ToVector3(Grid.WorldSize));
        }
    }

    /// <summary>
    /// Called when the processing of the current path is done.
    /// </summary>
    /// <param name="p">The path that has been worked on.</param>
    /// <param name="Success">Whether it was a success or not.</param>
    private void OnProccesingDone(Path p, bool Success)
    {
        p.Success = Success;
        p.Update();
        if (p.Vector3Path.Length == 0)
        {
            p.Success = false;
        }

        currentPathRequest.callback(p);
        currentPathRequest = null;
    }

    /// <summary>
    /// Called once every frame.
    /// </summary>
    void Update()
    {
        Process();
    }

    /// <summary>
    /// Process the next pathRequest in-line.
    /// </summary>
    private void Process()
    {
        if (currentPathRequest == null && pathRequests.Count > 0)
        {
            currentPathRequest = pathRequests.Dequeue();
            StartCoroutine(FindThePath(currentPathRequest.pathStart, currentPathRequest.pathEnd, currentPathRequest.grid));
        }
    }

    private IEnumerator ScanAGrid(GridGraph grid)
    {
        isScanning = true;
        for (int x = 0; x < grid.Size.x; x++)
        {
            for (int y = 0; y < grid.Size.y; y++)
            {
                grid.ScanNode(x, y);
            }
            if (x % 25 == 0)
            {
                yield return null;
            }
        }
        grid.OnScanDone();
        isScanning = false;
    }
}