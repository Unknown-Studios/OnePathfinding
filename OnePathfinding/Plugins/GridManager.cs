using Pathfinding;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Debug = UnityEngine.Debug;

public class GridManager : MonoBehaviour
{
    public static bool isScanning;
    public bool _ShowGizmos;
    public DebugLevel DebugLvl;
    public List<GridGraph> grid;
    public bool ShowFlockColor;
    public bool ShowPaths;
    private static GridManager _instance;
    private PathRequest currentPathRequest;
    private queue pathRequests = new queue();

    public enum DebugLevel
    {
        None = 0,
        Low = 1,
        High = 2,
    }

    public static bool ShowGizmos
    {
        get
        {
            return instance._ShowGizmos;
        }
        set
        {
            instance._ShowGizmos = value;
        }
    }

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
                _instance = GameObject.FindObjectOfType<GridManager>();
            }
            return _instance;
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

    public static GridGraph GetGrid(int index)
    {
        if (instance == null)
        {
            return null;
        }
        return instance.grid[index];
    }

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

    public static void ScanGrid()
    {
        if (instance == null)
        {
            Debug.Log("A GridManager wasn't found in the scene!");
            return;
        }
        instance.StartCoroutine(instance.ScanAGrid(Grid));
    }

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
}