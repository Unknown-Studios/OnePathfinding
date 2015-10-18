using OnePathfinding; //Add the custom classes and other stuff, this should be included in every script that is using Pathfinding
using UnityEngine;

public class SimpleAI : MonoBehaviour
{
    //This is a simple AI Controller, use AI if you want to learn the advanced features.

    //The speed at which the AI will move.
    public float speed;

    //The currently targeted gameobject.
    public GameObject target;

    //The current waypoint
    private int currentWay = 0;

    //Distance to waypoint before switching to the next one,
    //Don't make this 0.0 as there might be some offsets.
    private int nextWaypoint;

    //The currently active path
    private Path path;

    private void FixedUpdate()
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
            transform.position = Vector3.MoveTowards(transform.position, path.Vector3Path[currentWay], Time.fixedDeltaTime * speed);
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
    { //When the path proccesing is done
        if (p.Success) //If the path was a succes
        {
            path = p; //Set the path as the current path
            currentWay = 0; //Reset the waypoint counter
        }
    }

    private void Start()
    {
        //GridManager.RequestPath(starting point, ending point, OnPathFound, randomString);
        if (target != null)
        {
            GridManager.RequestPath(transform.position, target.transform.position, OnPathComplete);
        }
    }
}