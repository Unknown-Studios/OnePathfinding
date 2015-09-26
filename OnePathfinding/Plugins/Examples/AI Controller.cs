using Pathfinding; //Add the custom classes and other stuff, this should be included in every script that is using Pathfinding
using UnityEngine;

public class AIController : MonoBehaviour
{
    //This is a simple AI Controller, use AI if you want to learn the advanced features.

    public int nextWaypoint;
    public GameObject target;

    private int currentWay = 0;
    private Path path;

    private void OnDrawGizmos()
    {
        /*If the grid isn't set, this mostly happens
		 * if you don't have a GridManager component
		 * in the scene
		*/
        if (GridManager.Grid == null)
        {
            return;
        }
        /* If the terrain nodes doesn't exist,
		 * Scan the terrain, to get a new set of nodes.
		 */
        if (GridManager.Grid.nodes == null || GridManager.Grid.nodes.Length == 0)
        {
            GridManager.ScanGrid();
        }

        if (path != null)
        {
            for (int i = currentWay; i < path.Vector3Path.Length; i++)
            {
                Gizmos.color = Color.cyan;

                Gizmos.DrawCube(path.Vector3Path[i], Vector3.one);
                Gizmos.DrawLine(path.Vector3Path[i - 1], path.Vector3Path[i]);
            }
        }
    }

    private void OnEndReached()
    {
        Debug.Log("The end has been reached");
        //Make sure to set the path to null here, if you don't do this you
        //will be getting some out of range exceptions
        path = null;
    }

    private void OnPathComplete(Path p)
    { //When that path proccesing is done
        if (p.Success)
        { //If the path was a succes
            path = p; //Set the path as the current path
            currentWay = 0; //Reset the waypoint counter
        }
    }

    private void Start()
    {
        //GridManager.RequestPath(Vector3, Vector3,Callback,string);
        GridManager.RequestPath(transform.position, target.transform.position, OnPathComplete, gameObject.GetHashCode().ToString());
    }

    private void Update()
    {
        if (path != null)
        { //If the path exists
            if (Vector3.Distance(transform.position, path.Vector3Path[currentWay]) < nextWaypoint)
            { //If it is close enough to the waypoint
                currentWay++; //Proceed to the next waypoint
                if (currentWay >= path.Vector3Path.Length)
                { //If the last waypoint has been reached call EndOfPath
                    OnEndReached();
                    return;
                }
            }
            //Move towards the current waypoint
            transform.position = Vector3.MoveTowards(transform.position, path.Vector3Path[currentWay], Time.deltaTime * 20);
        }
    }
}