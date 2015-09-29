using Pathfinding;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Debug = UnityEngine.Debug;

public class GridManager : MonoBehaviour
{
    #region Fields

    public static bool isScanning;
    public DebugLevel DebugLvl;
    public List<GridGraph> grid;
    public bool ShowFlockColor;
    public bool ShowGizmos;
    public bool ShowPaths;
    private static GridManager _instance;
    private PathRequest currentPathRequest;
    private queue pathRequests = new queue();

    #endregion Fields

    #region Enums

    public enum DebugLevel
    {
        None = 0,
        Low = 1,
        High = 2,
    }

    #endregion Enums

    #region Properties

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

    #endregion Properties

    #region Methods

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
    /// Request a path from point pathStart to point pathEnd.
    /// </summary>
    /// <param name="pathStart">The starting point of the path</param>
    /// <param name="pathEnd">The ending point of the path</param>
    /// <param name="callback">A function with the parameters (Path)</param>
    /// <param name="Name">A random string, this is used for identifiing a queue in the queuesystem.</param>
    public static void RequestPath(Vector3 pathStart, Vector3 pathEnd, Action<Path> callback, string Name)
    {
        PathRequest newRequest = new PathRequest(pathStart, pathEnd, callback, Name, Grid);
        PathRequest contains = instance.pathRequests.Contains(Name);
        if (contains != null)
        {
            instance.pathRequests.Remove(contains);
            return;
        }
        else
        {
            instance.pathRequests.Enqueue(newRequest);
            instance.Process();
            return;
        }
    }

    /// <summary>
    /// Request a path from point pathStart to point pathEnd, in a specific grid.
    /// </summary>
    /// <param name="pathStart">The starting point of the path</param>
    /// <param name="pathEnd">The ending point of the path</param>
    /// <param name="callback">A function with the parameters (Path)</param>
    /// <param name="Name">A random string, this is used for identifiing a queue in the queuesystem.</param>
    /// <param name="grid">The specific grid you want to find the path on.</param>
    public static void RequestPath(Vector3 pathStart, Vector3 pathEnd, Action<Path> callback, string Name, GridGraph grid)
    {
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
            instance.Process();
            return;
        }
    }

    /// <summary>
    /// Scan the first grid in the grid array
    /// </summary>
    public static void ScanGrid()
    {
        if (instance == null)
        {
            Debug.Log("A GridManager wasn't found in the scene!");
            return;
        }
        instance.StartCoroutine(instance.ScanAGrid(Grid));
    }

    /// <summary>
    /// Scan a specific grid in the grid array.
    /// </summary>
    /// <param name="grid">The grid that is going to be scanned.</param>
    public static void ScanGrid(GridGraph grid)
    {
        if (instance == null)
        {
            Debug.Log("A GridManager wasn't found in the scene!");
            return;
        }
        instance.StartCoroutine(instance.ScanAGrid(grid));
    }

    private IEnumerator FindThePath(Vector3 startPos, Vector3 targetPos, GridGraph grid)
    {
        if (grid.nodes == null || grid.nodes.Length == 0)
        {
            ScanGrid(grid);
        }

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

            while (open.Count > 0)
            {
                Node currentNode = open.RemoveFirst();
                closed.Add(currentNode);

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
                if (open.Count % 100 == 0)
                {
                    yield return null;
                }
            }
        }
        if (pathSuccess)
        {
            p = grid.RetracePath(startNode, targetNode);
        }
        OnProccesingDone(p, pathSuccess);
    }

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
                }
                if (DebugLvl == DebugLevel.High)
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
                if (DebugLvl == DebugLevel.Low)
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
            Gizmos.color = Color.white;
            Gizmos.DrawWireCube((Grid.center + (Grid.Vector2ToVector3(Grid.WorldSize) / 2)), Grid.Vector2ToVector3(Grid.WorldSize));
        }
    }

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
        Process();
    }

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

    #endregion Methods
}