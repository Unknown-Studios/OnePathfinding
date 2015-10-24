﻿using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// The main namespace used for the pathfinding.
/// </summary>
namespace OnePathfinding
{
    /// <summary>
    /// Used by heap class.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IHeapItem<T> : IComparable<T>
    {
        /// <summary>
        /// Heap index
        /// </summary>
        int HeapIndex
        {
            get;
            set;
        }
    }

    /// <summary>
    /// A GridGraph is a grid, this is a class that contains all the variables a grid should have.
    /// </summary>
    [Serializable]
    public class GridGraph
    {
        /// <summary>
        /// The distance between each node,
        /// </summary>
        public float _NodeRadius = 2f;

        /// <summary>
        /// The size of the grid in world-space.
        /// </summary>
        public Vector2 _WorldSize = new Vector2(100, 100);

        /// <summary>
        /// The maximum angle at which an AI component can walk at.
        /// </summary>
        public int angleLimit = 45;

        /// <summary>
        /// The name of the grid.
        /// </summary>
        public string name = "New Grid";

        /// <summary>
        /// A list of all nodes in the current grid.
        /// </summary>
        public Node[,] nodes;

        /// <summary>
        /// A offset for this grid, this is an offset from the GameControllers position.
        /// </summary>
        public Vector3 offset;

        /// <summary>
        /// A bool to tell you if it is scanning or not.
        /// </summary>
        public bool Scanning;

        /// <summary>
        /// This is very important, it is the layers at which the AI can walk on.
        /// </summary>
        public LayerMask WalkableMask;

        private Vector2 _Size;

        /// <summary>
        /// The maximum number of nodes the grid can have with the current settings
        /// </summary>
        public int maxSize
        {
            get
            {
                return Mathf.RoundToInt(Size.x * Size.y);
            }
        }

        /// <summary>
        /// The distance between each node.
        /// </summary>
        public float NodeRadius
        {
            get
            {
                return _NodeRadius;
            }
            set
            {
                _NodeRadius = value;
                Update();
            }
        }

        /// <summary>
        /// The current size available with the grids settings.
        /// </summary>
        public Vector2 Size
        {
            get
            {
                return _Size;
            }
        }

        /// <summary>
        /// The world-size of the grid.
        /// </summary>
        public Vector2 WorldSize
        {
            get
            {
                return _WorldSize;
            }
            set
            {
                _WorldSize = value;
                Update();
            }
        }

        /// <summary>
        /// Gets the distance between two nodes. This is used for the pathfinding algorithm and will
        /// not get the actual distance in world-space.
        /// </summary>
        /// <param name="nodeA">The starting node</param>
        /// <param name="nodeB">The comparison node</param>
        /// <returns>A* distance between the two nodes.</returns>
        public int GetDistance(Node nodeA, Node nodeB)
        {
            int dstX = Mathf.Abs(nodeA.x - nodeB.x);
            int dstY = Mathf.Abs(nodeA.y - nodeB.y);

            if (dstX > dstY)
                return 14 * dstY + 10 * (dstX - dstY);
            return 14 * dstX + 10 * (dstY - dstX);
        }

        /// <summary>
        /// Get a list of neighbor nodes for this node.
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        public List<Node> GetNeighbours(Node node)
        {
            List<Node> neighbours = new List<Node>();

            for (int x = -1; x <= 1; x++)
            {
                for (int y = -1; y <= 1; y++)
                {
                    if (x == 0 && y == 0)
                        continue;

                    int checkX = node.x + x;
                    int checkY = node.y + y;

                    if (checkX >= 0 && checkX < Size.x && checkY >= 0 && checkY < Size.y)
                    {
                        neighbours.Add(nodes[checkX, checkY]);
                    }
                }
            }

            return neighbours;
        }

        /// <summary>
        /// Transforms the information given into a possible world size for the grid.
        /// </summary>
        /// <param name="OPPOS"></param>
        /// <returns></returns>
        public Vector3 GridToWorld(Vector3 OPPOS)
        {
            Vector3 World = new Vector3();

            World.x = OPPOS.x * (NodeRadius * 2) - WorldSize.x / 2;
            World.z = OPPOS.z * (NodeRadius * 2) - WorldSize.y / 2;

            return World;
        }

        /// <summary>
        /// Gets the nearest walkable grid to the given worldPosition.
        /// </summary>
        /// <param name="worldPosition">The position at which to find the nearest walkable node.</param>
        /// <returns>The nearest walkable node.</returns>
        public Node NearWalkable(Vector3 worldPosition)
        {
            if (nodes == null || nodes.Length == 0)
            {
                return null;
            }
            float percentX = Mathf.Clamp01((worldPosition.x) / WorldSize.x);
            float percentY = Mathf.Clamp01((worldPosition.z) / WorldSize.y);

            int x = Mathf.RoundToInt((Size.x - 1) * percentX);
            int y = Mathf.RoundToInt((Size.y - 1) * percentY);

            int MaxDistance = 50;
            if (x * y < nodes.Length)
            {
                for (int X = 0; X < MaxDistance; X++)
                {
                    for (int Y = 0; Y < MaxDistance; Y++)
                    {
                        int X1 = x + X;
                        int Y1 = y + Y;
                        X1 = Mathf.Clamp(X1, 0, nodes.GetLength(0) - 1);
                        Y1 = Mathf.Clamp(Y1, 0, nodes.GetLength(1) - 1);

                        int X2 = x - X;
                        int Y2 = y - Y;
                        X2 = Mathf.Clamp(X2, 0, nodes.GetLength(0) - 1);
                        Y2 = Mathf.Clamp(Y2, 0, nodes.GetLength(1) - 1);

                        if (nodes[X1, Y1] != null && nodes[X1, Y1].Walkable)
                        {
                            return nodes[X1, Y1];
                        }
                        else if (nodes[X2, Y2] != null && nodes[X2, Y2].Walkable)
                        {
                            return nodes[X2, Y2];
                        }
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Gets the nearest node from world position.
        /// </summary>
        /// <param name="worldPosition"></param>
        /// <returns>The nearest node to this position.</returns>
        public Node NodeFromWorldPos(Vector3 worldPosition)
        {
            if (nodes == null || nodes.Length == 0)
            {
                return null;
            }
            float percentX = Mathf.Clamp01((worldPosition.x) / WorldSize.x);
            float percentY = Mathf.Clamp01((worldPosition.z) / WorldSize.y);

            int x = Mathf.RoundToInt((Size.x - 1) * percentX);
            int y = Mathf.RoundToInt((Size.y - 1) * percentY);
            if (x * y > nodes.Length)
            {
                return nodes[0, 0];
            }
            else
            {
                return nodes[x, y];
            }
        }

        /// <summary>
        /// Gets this nodes world position.
        /// </summary>
        /// <param name="node"></param>
        /// <returns>The nodes position in world-space.</returns>
        public Vector3 NodeToWorldPos(Node node)
        {
            Vector3 worldPos = new Vector3();

            worldPos.x = (node.x * (NodeRadius * 2)) - WorldSize.x / 2;
            worldPos.y = node.height;
            worldPos.z = (node.y * (NodeRadius * 2)) - WorldSize.y / 2;

            return offset + worldPos;
        }

        /// <summary>
        /// Called when scanning the grid is completed.
        /// </summary>
        public void OnScanDone()
        {
            Update();
        }

        /// <summary>
        /// Walks back through the given path, and ends up with a list of nodes.
        /// </summary>
        /// <param name="start">The first used node</param>
        /// <param name="end">The target node</param>
        /// <returns>Returns a path object.</returns>
        public Path RetracePath(Node start, Node end)
        {
            Path P = new Path();
            Node Cur = end;

            while (Cur != start)
            {
                P.movementPath.Add(Cur);
                Cur = Cur.parent;
            }

            P.movementPath.Reverse();

            return P;
        }

        /// <summary>
        /// Scans a specific node in the array.
        /// </summary>
        /// <param name="x">The nodes X position in the array.</param>
        /// <param name="y">The nodes Y position in the array.</param>
        public void ScanNode(int x, int y)
        {
            if (nodes == null || nodes.Length == 0 || nodes.Length != (Mathf.RoundToInt(Size.x) * Mathf.RoundToInt(Size.y)))
            {
                Update();
                nodes = new Node[Mathf.RoundToInt(Size.x), Mathf.RoundToInt(Size.y)];
            }
            bool Walk = false;
            RaycastHit hit = new RaycastHit();

            Vector3 endPos = offset + Vector2ToVector3(_WorldSize) / 2 + GridToWorld(new Vector3(x, 0, y));

            Vector3 startPos = endPos;
            startPos.y = 500;
            endPos.y = 0;
            if (Physics.Linecast(startPos, endPos, out hit))
            {
                endPos.y = hit.point.y;
                if (Vector3.Angle(Vector3.up, hit.normal) < angleLimit)
                {
                    Walk = !(Physics.CheckSphere(endPos, (NodeRadius * 2), WalkableMask));
                }
            }

            nodes[x, y] = new Node(x, y, Walk, endPos);
        }

        /// <summary>
        /// Transforms a vector2 to a vector3, format: Vector3(vec.x, 0,vec.y)
        /// </summary>
        /// <param name="vec">The vector to transform</param>
        /// <returns>The transformed vector3.</returns>
        public Vector3 Vector2ToVector3(Vector2 vec)
        {
            return new Vector3(vec.x, 0, vec.y);
        }

        private void Update()
        {
            _Size.x = Mathf.FloorToInt(WorldSize.x / (NodeRadius * 2));
            _Size.y = Mathf.FloorToInt(WorldSize.y / (NodeRadius * 2));
        }
    }

    /// <summary>
    /// Heap
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class Heap<T> where T : IHeapItem<T>
    {
        /// <summary>
        /// Current length of items array.
        /// </summary>
        private int currentItemCount;

        /// <summary>
        /// Array of items.
        /// </summary>
        private T[] items;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="maxHeapSize"></param>
        public Heap(int maxHeapSize)
        {
            items = new T[maxHeapSize];
        }

        /// <summary>
        /// Length of items array.
        /// </summary>
        public int Count
        {
            get
            {
                return currentItemCount;
            }
        }

        /// <summary>
        /// Used to add an item to the item array.
        /// </summary>
        /// <param name="item"></param>
        public void Add(T item)
        {
            item.HeapIndex = currentItemCount;
            items[currentItemCount] = item;
            SortUp(item);
            currentItemCount++;
        }

        /// <summary>
        /// Check if items array contains item.
        /// </summary>
        /// <param name="item">Item to check for.</param>
        /// <returns></returns>
        public bool Contains(T item)
        {
            return Equals(items[item.HeapIndex], item);
        }

        /// <summary>
        /// Remove the first object in the items array.
        /// </summary>
        /// <returns></returns>
        public T RemoveFirst()
        {
            T firstItem = items[0];
            currentItemCount--;
            items[0] = items[currentItemCount];
            items[0].HeapIndex = 0;
            SortDown(items[0]);
            return firstItem;
        }

        /// <summary>
        /// Start sorting of an object.
        /// </summary>
        /// <param name="item">The item to sort.</param>
        public void UpdateItem(T item)
        {
            SortUp(item);
        }

        /// <summary>
        /// Sort down.
        /// </summary>
        /// <param name="item"></param>
        private void SortDown(T item)
        {
            while (true)
            {
                int childIndexLeft = item.HeapIndex * 2 + 1;
                int childIndexRight = item.HeapIndex * 2 + 2;
                int swapIndex = 0;

                if (childIndexLeft < currentItemCount)
                {
                    swapIndex = childIndexLeft;

                    if (childIndexRight < currentItemCount)
                    {
                        if (items[childIndexLeft].CompareTo(items[childIndexRight]) < 0)
                        {
                            swapIndex = childIndexRight;
                        }
                    }

                    if (item.CompareTo(items[swapIndex]) < 0)
                    {
                        Swap(item, items[swapIndex]);
                    }
                    else
                    {
                        return;
                    }
                }
                else
                {
                    return;
                }
            }
        }

        /// <summary>
        /// Sort up.
        /// </summary>
        /// <param name="item"></param>
        private void SortUp(T item)
        {
            int parentIndex = (item.HeapIndex - 1) / 2;

            while (true)
            {
                T parentItem = items[parentIndex];
                if (item.CompareTo(parentItem) > 0)
                {
                    Swap(item, parentItem);
                }
                else
                {
                    break;
                }

                parentIndex = (item.HeapIndex - 1) / 2;
            }
        }

        /// <summary>
        /// Swap two item's place in the items array.
        /// </summary>
        /// <param name="itemA">1. Item to swap.</param>
        /// <param name="itemB">2. Item to swap.</param>
        private void Swap(T itemA, T itemB)
        {
            items[itemA.HeapIndex] = itemB;
            items[itemB.HeapIndex] = itemA;
            int itemAIndex = itemA.HeapIndex;
            itemA.HeapIndex = itemB.HeapIndex;
            itemB.HeapIndex = itemAIndex;
        }
    }

    /// <summary>
    /// A node is a position in the world that needs to be validated for whether or not the AI agent
    /// can walk on it.
    /// </summary>
    public class Node : IHeapItem<Node>
    {
        /// <summary>
        /// The nodes current gCost.
        /// </summary>
        public int gCost;

        /// <summary>
        /// The nodes current hCost.
        /// </summary>
        public int hCost;

        /// <summary>
        /// This nodes height in world-space.
        /// </summary>
        public float height;

        /// <summary>
        /// The parent node, this is used to retrace the path, when it is found.
        /// </summary>
        public Node parent;

        /// <summary>
        /// Whether or not the node is walkable.
        /// </summary>
        public bool Walkable = false;

        /// <summary>
        /// The nodes position in world-space.
        /// </summary>
        public Vector3 WorldPosition = Vector3.zero;

        /// <summary>
        /// The nodes X value in the grid's node array.
        /// </summary>
        public int x;

        /// <summary>
        /// The nodes Y value in the grid's node array.
        /// </summary>
        public int y;

        /// <summary>
        /// This nodes heap index
        /// </summary>
        private int heapIndex;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="_x">X index in the nodes array.</param>
        /// <param name="_y">Y index in the nodes array.</param>
        /// <param name="_Walk">Whether it is walkable or not.</param>
        /// <param name="_WorldPos">The world position of the node.</param>
        public Node(int _x, int _y, bool _Walk, Vector3 _WorldPos)
        {
            x = _x;
            y = _y;
            Walkable = _Walk;
            WorldPosition = _WorldPos;
            height = _WorldPos.y;
        }

        /// <summary>
        /// This returns the fCost, it is the gCost and the hCost combined.
        /// </summary>
        public int fCost
        {
            get
            {
                return gCost + hCost;
            }
        }

        /// <summary>
        /// This nodes headIndex
        /// </summary>
        public int HeapIndex
        {
            get
            {
                return heapIndex;
            }
            set
            {
                heapIndex = value;
            }
        }

        /// <summary>
        /// Compares this node's fCost the node1's fCost.
        /// </summary>
        /// <param name="node1">The node to compare to.</param>
        /// <returns>The difference between the fCosts.</returns>
        public int CompareTo(Node node1)
        {
            int compare = fCost.CompareTo(node1.fCost);
            if (compare == 0)
            {
                compare = hCost.CompareTo(node1.hCost);
            }
            return -compare;
        }
    }

    /// <summary>
    /// A path is a list of waypoints that can be used to move the AI agent.
    /// </summary>
    public class Path
    {
        /// <summary>
        /// The list of nodes that was found during the path retracing.
        /// </summary>
        public List<Node> movementPath = new List<Node>();

        /// <summary>
        /// If the path generation was a success or not.
        /// </summary>
        public bool Success = true;

        /// <summary>
        /// A list of the worldSpace positions for the nodes.
        /// </summary>
        public Vector3[] Vector3Path = new Vector3[0];

        /// <summary>
        /// The AIs current destination.
        /// </summary>
        public Vector3 Destination
        {
            get
            {
                return Vector3Path[Vector3Path.Length - 1];
            }
        }

        /// <summary>
        /// Called when the path has been generated.
        /// </summary>
        public void Update()
        {
            Vector3Path = SmoothPath(movementPath);
        }

        /// <summary>
        /// Smooths the path so it uses less nodes to get around the map.
        /// </summary>
        /// <param name="path">The path to smooth.</param>
        /// <returns>The smoothened path.</returns>
        private Vector3[] SmoothPath(List<Node> path)
        {
            List<Vector3> waypoints = new List<Vector3>();
            if (path.Count != 0)
            {
                waypoints.Add(path[0].WorldPosition);
                for (int i = 0; i < path.Count; i++)
                {
                    for (int c = path.Count - 1; c > 1; c--)
                    {
                        if (c <= i)
                        {
                            waypoints.Add(path[i].WorldPosition);
                            break;
                        }
                        else if (!Physics.Linecast(path[i].WorldPosition, path[c].WorldPosition)) //If there isn't something in the way
                        {
                            i = c;
                            waypoints.Add(path[c].WorldPosition);
                            break;
                        }
                    }
                }
            }
            return waypoints.ToArray();
        }
    }

    /// <summary>
    /// A PathRequest is a request to the GridManager for a path, this is holding all the required
    /// data to get a path.
    /// </summary>
    public class PathRequest
    {
        /// <summary>
        /// The function to call when the path has been found.
        /// </summary>
        public Action<Path> callback;

        /// <summary>
        /// The grid to find the path on.
        /// </summary>
        public GridGraph grid;

        /// <summary>
        /// The name of the current path, this is used to identify the requested path.
        /// </summary>
        public string name;

        /// <summary>
        /// The target position of the path.
        /// </summary>
        public Vector3 pathEnd;

        /// <summary>
        /// The start position of the path.
        /// </summary>
        public Vector3 pathStart;

        /// <summary>
        /// Constructor for PathRequest without specific grid.
        /// </summary>
        /// <param name="_start">The start position for the path</param>
        /// <param name="_end">The target position for the path</param>
        /// <param name="_callback">The function to call when processing is done.</param>
        /// <param name="Name">The name of the PathRequest.</param>
        public PathRequest(Vector3 _start, Vector3 _end, Action<Path> _callback, string Name)
        {
            pathStart = _start;
            pathEnd = _end;
            callback = _callback;
            name = Name;
        }

        /// <summary>
        /// Constructor for PathRequest with specific grid.
        /// </summary>
        /// <param name="_start">The start position for the path</param>
        /// <param name="_end">The target position for the path</param>
        /// <param name="_callback">The function to call when processing is done.</param>
        /// <param name="Name">The name of the PathRequest.</param>
        /// <param name="_grid">The specific grid to find the path on.</param>
        public PathRequest(Vector3 _start, Vector3 _end, Action<Path> _callback, string Name, GridGraph _grid)
        {
            pathStart = _start;
            pathEnd = _end;
            callback = _callback;
            name = Name;
            grid = _grid;
        }
    }

    /// <summary>
    /// A queue is a list of the PathRequests used for queuing the processing of the paths.
    /// </summary>
    [Serializable]
    public class queue
    {
        /// <summary>
        /// The list used to hold the PathRequest.
        /// </summary>
        public List<PathRequest> list = new List<PathRequest>();

        /// <summary>
        /// The length of the PathRequest list.
        /// </summary>
        public int Count
        {
            get
            {
                return list.Count;
            }
        }

        /// <summary>
        /// Checks if the list contains the specific PathRequest.
        /// </summary>
        /// <param name="name">The list to check in.</param>
        /// <returns>MULL if it wasn't found, returns the PathRequest if it was found.</returns>
        public PathRequest Contains(string name)
        {
            foreach (PathRequest p in list)
            {
                if (p.name == name)
                {
                    return p;
                }
            }
            return null;
        }

        /// <summary>
        /// Remove the oldest PathRequest in the queue.
        /// </summary>
        /// <returns></returns>
        public PathRequest Dequeue()
        {
            return Remove(0);
        }

        /// <summary>
        /// Adds a new PathRequest to the list.
        /// </summary>
        /// <param name="PR">The PathRequest to add.</param>
        public void Enqueue(PathRequest PR)
        {
            list.Add(PR);
        }

        /// <summary>
        /// Remove a specific PathRequest from the list.
        /// </summary>
        /// <param name="index">The index of the PathRequest.</param>
        /// <returns>The removed PathRequest.</returns>
        public PathRequest Remove(int index)
        {
            PathRequest Item = list[index];
            list.RemoveAt(index);
            return Item;
        }

        /// <summary>
        /// Remove a specific PathRequest from the list.
        /// </summary>
        /// <param name="PR">The PathRequest to remove.</param>
        /// <returns>The removed PathRequest.</returns>
        public PathRequest Remove(PathRequest PR)
        {
            list.Remove(PR);
            return PR;
        }
    }
}