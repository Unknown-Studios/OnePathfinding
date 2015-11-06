using System;
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
        /// The shape of the grid.
        /// </summary>
        public GridType _gridType = GridType.Plane;

        /// <summary>
        /// The distance between each node,
        /// </summary>
        public float _NodeRadius = 2f;

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
        public Node[,,] nodes;

        /// <summary>
        /// A offset for this grid, this is an offset from the GameControllers position.
        /// </summary>
        public Vector3 offset;

        /// <summary>
        /// The radius of the Sphere (Sphere grid only)
        /// </summary>
        public int Radius;

        /// <summary>
        /// A bool to tell you if it is scanning or not.
        /// </summary>
        public bool Scanning;

        /// <summary>
        /// Whether or not to scan the terrain when loading the scene.
        /// </summary>
        public bool ScanOnLoad = true;

        /// <summary>
        /// This is very important, it is the layers at which the AI can walk on.
        /// </summary>
        public LayerMask UnWalkableMask;

        /// <summary>
        /// The size of the grid in world-space.
        /// </summary>
        public Vector2 WorldSize = new Vector2(100, 100);

        private Vector2 _Size;

        /// <summary>
        /// The shape of the grid.
        /// </summary>
        public enum GridType
        {
            Plane = 0,
            Sphere = 1,
        }

        /// <summary>
        /// private Set and Get, public Get
        /// </summary>
        public GridType gridType
        {
            get
            {
                return _gridType;
            }
        }

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
            }
        }

        /// <summary>
        /// The current size available with the grids settings.
        /// </summary>
        public Vector2 Size
        {
            get
            {
                Update();
                return _Size;
            }
        }

        /// <summary>
        /// Clamps the given Vector3 to the grid.
        /// </summary>
        /// <param name="v3">The Vector3 to clamp</param>
        /// <returns>Clamped Vector3</returns>
        public Vector3 Clamp(Vector3 v3)
        {
            v3.x = Mathf.Clamp(v3.x, offset.x, offset.x + WorldSize.x);
            v3.z = Mathf.Clamp(v3.z, offset.z, offset.z + WorldSize.y);
            return v3;
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

            if (Scanning || nodes == null || nodes.Length == 0)
            {
                return null;
            }

            if (gridType == GridType.Plane)
            {
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
                            neighbours.Add(nodes[checkX, checkY, 0]);
                        }
                    }
                }
            }
            else if (gridType == GridType.Sphere)
            {
                for (int x = -1; x <= 1; x++)
                {
                    for (int y = -1; y <= 1; y++)
                    {
                        if (x == 0 && y == 0)
                            continue;

                        //Get the X and Y position for the node to check after.
                        int checkX = node.x + x;
                        int checkY = node.y + y;

                        //Clamp the X and Y to fit in the sphere.
                        if (checkX < 0)
                        {
                            checkX = Mathf.RoundToInt(Size.x - 1);
                        }
                        else if (checkX >= Size.x)
                        {
                            checkX = 0;
                        }
                        if (checkY < 0)
                        {
                            checkY = Mathf.RoundToInt(Size.x - 1);
                        }
                        else if (checkY >= Size.x)
                        {
                            checkY = 0;
                        }

                        //Check for candidates:
                        Node last = null;
                        for (int c = 0; c < 6; c++)
                        {
                            if (nodes[checkX, checkY, c] != null)
                            {
                                if (last == null || Vector3.Distance(node.WorldPosition, nodes[checkX, checkY, c].WorldPosition) < Vector3.Distance(node.WorldPosition, last.WorldPosition)) //All grids for the best candidate to be the neighbour.
                                {
                                    last = nodes[checkX, checkY, c];
                                }
                            }
                        }
                        //Add to neighbor list.
                        neighbours.Add(last);
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
            if (gridType == GridType.Plane)
            {
                float percentX = Mathf.Clamp01(worldPosition.x / WorldSize.x);
                float percentY = Mathf.Clamp01(worldPosition.z / WorldSize.y);

                int x = (int)(Mathf.Clamp((Size.x - 1) * percentX, 0, Size.x - 1));
                int y = (int)(Mathf.Clamp((Size.y - 1) * percentY, 0, Size.y - 1));

                return nodes[x, y, 0];
            }
            else if (gridType == GridType.Sphere)
            {
                Node cur = null;
                for (int x = 0; x < nodes.GetLength(0); x++)
                {
                    for (int y = 0; y < nodes.GetLength(1); y++)
                    {
                        for (int i = 0; i < nodes.GetLength(2); i++)
                        {
                            if (cur == null || Vector3.Distance(worldPosition, nodes[x, y, i].WorldPosition) < Vector3.Distance(worldPosition, cur.WorldPosition))
                            {
                                cur = nodes[x, y, i];
                            }
                        }
                    }
                }
                return cur;
            }
            return null;
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
        /// <param name="Grid">The grid to retrace the path on.</param>
        /// <returns>Returns a path object.</returns>
        public Path RetracePath(Node start, Node end, GridGraph Grid)
        {
            Path P = new Path(Grid);
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
        /// Used to scan the node on a sphere
        /// </summary>
        /// <param name="x">The X value of the point</param>
        /// <param name="y">The Y value of the point</param>
        /// <param name="z">The Z value of the point</param>
        /// <param name="gridIndex">Which side of the sphere to scan.</param>
        public void ScanNode(int x, int y, int z, int gridIndex)
        {
            if (gridType != GridType.Sphere)
            {
                Debug.LogError("You are using the wrong ScanNode function. Use ScanNode(x,y) instead.");
                return;
            }
            if (nodes == null || nodes.GetLength(0) != Size.x + 1 || nodes.GetLength(1) != Size.x + 1)
            {
                nodes = new Node[Mathf.RoundToInt(Size.x + 1), Mathf.RoundToInt(Size.x + 1), 6]; //6 because there is 6 sides in a cube.
            }

            bool Walk = false;
            RaycastHit hit = new RaycastHit();
            Vector3 endPos = Vector3.zero;

            Vector3 v = new Vector3(x, y, z) * 2f / (Size.x + 1) - Vector3.one;
            Vector3 startPos = v.normalized * Radius;

            if (Physics.Linecast(startPos, offset, out hit))
            {
                endPos = hit.point;
                if (Vector3.Angle(v.normalized, hit.normal) < angleLimit)
                {
                    if (!Physics.CheckSphere(endPos, (NodeRadius * 2), UnWalkableMask))
                    {
                        Walk = true;
                    }
                }
            }

            switch (gridIndex)
            {
                case 0: //-Z
                    nodes[x, y, gridIndex] = new Node(x, y, Walk, endPos);
                    break;

                case 1: //+X
                    nodes[y, z, gridIndex] = new Node(y, z, Walk, endPos);
                    break;

                case 2: //+Z
                    nodes[x, y, gridIndex] = new Node(x, y, Walk, endPos);
                    break;

                case 3: //-X
                    nodes[y, z, gridIndex] = new Node(y, z, Walk, endPos);
                    break;

                case 4: //+Y
                    nodes[x, z, gridIndex] = new Node(x, z, Walk, endPos);
                    break;

                case 5: //-Y
                    nodes[x, z, gridIndex] = new Node(x, z, Walk, endPos);
                    break;

                default:
                    Debug.LogError("GridIndex out of range");
                    break;
            }
        }

        /// <summary>
        /// Scans a specific node in the array.
        /// </summary>
        /// <param name="x">The nodes X position in the array.</param>
        /// <param name="y">The nodes Y position in the array.</param>
        public void ScanNode(int x, int y)
        {
            bool Walk = false;
            RaycastHit hit = new RaycastHit();
            Vector3 endPos = Vector3.zero;

            if (gridType != GridType.Plane)
            {
                return;
            }
            if (nodes == null || nodes.Length != (Mathf.RoundToInt(Size.x) * Mathf.RoundToInt(Size.y)))
            {
                nodes = new Node[Mathf.RoundToInt(Size.x), Mathf.RoundToInt(Size.y), 1];
            }
            endPos = offset + Vector2ToVector3(WorldSize) / 2 + GridToWorld(new Vector3(x, 0, y));

            Vector3 startPos = endPos;
            startPos.y = 500;
            endPos.y = 0;
            if (Physics.Linecast(startPos, endPos, out hit))
            {
                endPos.y = hit.point.y;
                if (Vector3.Angle(Vector3.up, hit.normal) < angleLimit)
                {
                    if (!Physics.CheckSphere(endPos, (NodeRadius * 2), UnWalkableMask))
                    {
                        Walk = true;
                    }
                }
            }
            nodes[x, y, 0] = new Node(x, y, Walk, endPos);
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
            if (gridType == GridType.Plane)
            {
                _Size.x = Mathf.FloorToInt(WorldSize.x / (NodeRadius * 2));
                _Size.y = Mathf.FloorToInt(WorldSize.y / (NodeRadius * 2));
            }
            else if (gridType == GridType.Sphere)
            {
                _Size.x = WorldSize.x;
            }
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
        /// Array of type T.
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
            if (currentItemCount >= items.Length)
            {
                Debug.LogError("An error happened");
            }
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
        /// The grid that this path was made on.
        /// </summary>
        public GridGraph grid;

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
        /// Constructor
        /// </summary>
        /// <param name="Grid">The grid that this path should be generated on.</param>
        public Path(GridGraph Grid)
        {
            grid = Grid;
        }

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
                if (grid.gridType == GridGraph.GridType.Plane)
                {
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
                else if (grid.gridType == GridGraph.GridType.Sphere)
                {
                    for (int i = 0; i < path.Count; i++)
                    {
                        waypoints.Add(path[i].WorldPosition);
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