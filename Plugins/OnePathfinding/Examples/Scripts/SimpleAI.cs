using OnePathfinding; //Add the custom classes and other stuff, this should be included in every script that is using Pathfinding
using UnityEngine;

/// <summary>
/// SimpleAI is mainly for learning the basics of this component.
/// </summary>
public class SimpleAI : MonoBehaviour
{
    //This is a simple AI Controller, use AI if you want to learn the advanced features.

    /// <summary>
    /// Distance to waypoint before switching to the next one, Don't make this 0.0 as there might be
    /// some offsets.
    /// </summary>
    public float nextWaypoint = 2f;

    ///The speed at which the AI will move.
    public float speed;

    ///The currently targeted GameObject.
    public GameObject target;

    ///The current waypoint
    private int currentWay = 0;

    ///The currently active path
    private Path path;

    /// <summary>
    /// FixedUpdate
    /// </summary>
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

    /// <summary>
    /// Called when the end of the path has been reached.
    /// </summary>
    private void OnEndReached()
    {
        Debug.Log("The end has been reached");
        //Make sure to set the path to null here, if you don't do this you
        //will be getting some out of range exceptions
        path = null;
    }

    /// <summary>
    /// Called when the path has been generated.
    /// </summary>
    /// <param name="p">Returns the generated path.</param>
    private void OnPathComplete(Path p)
    { //When the path processing is done
        if (p.Success) //If the path was a success
        {
            path = p; //Set the path as the current path
            currentWay = 0; //Reset the waypoint counter
        }
    }

    /// <summary>
    /// Called on initialization.
    /// </summary>
    private void Start()
    {
        //GridManager.RequestPath(starting point, ending point, OnPathFound, randomString);
        if (target != null)
        {
            GridManager.RequestPath(transform.position, target.transform.position, OnPathComplete);
        }
    }
}