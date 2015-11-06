using OnePathfinding;
using System.Collections;

using UnityEngine;
using Random = UnityEngine.Random;

/// <summary>
/// The main example in the OnePathfinding solution.
/// </summary>
[RequireComponent(typeof(AIData))]
public class AdvancedAI : MonoBehaviour
{
    /// <summary>
    /// Current target.
    /// </summary>
    [HideInInspector]
    public GameObject _target;

    /// <summary>
    /// The AIs current state of behavior.
    /// </summary>
    public CurrentAIState AIState;

    /// <summary>
    /// The damage this AI will do if it gets close enough to the target.
    /// </summary>
    public float Damage;

    /// <summary>
    /// The height at which a flying AI will fly at.
    /// </summary>
    public float Flyheight = 50.0f;

    /// <summary>
    /// Whether it is a flying animal or not.
    /// </summary>
    public bool Flying = false;

    /// <summary>
    /// The grids current index.
    /// </summary>
    public int GridIndex;

    /// <summary>
    /// The currently selected grid.
    /// </summary>
    public GridGraph grid;

    /// <summary>
    /// The maximum distance at which this animals noise can be heard.
    /// </summary>
    public float NoiseDistance = 100.0f;

    /// <summary>
    /// A reference to this AIs current path.
    /// </summary>
    public Path path = null;

    /// <summary>
    /// If the agent is requesting or already have a path
    /// </summary>
    public PathType pt;

    /// <summary>
    /// If true it will add a random variable between -1 and 1 to the speed, this is used to add
    /// some variation to the AIs
    /// </summary>
    public bool RandomAddSpeed = true;

    /// <summary>
    /// This animals models area.
    /// </summary>
    public float Size;

    /// <summary>
    /// The speed at which this animal moves at.
    /// </summary>
    public float speed = 6.0F;

    /// <summary>
    /// This animals type, whether it is aggressive, passive(Coming soon) or scared
    /// </summary>
    public AnimalType Type;

    /// <summary>
    /// The distance at which this animal can see at.
    /// </summary>
    public float ViewDistance = 50.0f;

    private bool _pause;

    /// <summary>
    /// The distance from the target before it is a possibility to attack the target.
    /// </summary>
    private float AttackRange = 2.5f;

    /// <summary>
    /// An index to which waypoint in the path.Vector3Path.
    /// </summary>
    private int currentWaypoint = 0;

    /// <summary>
    /// A reference to AIData component.
    /// </summary>
    private AIData Data;

    /// <summary>
    /// The last time this animal attacked, used to offset continuous attacks.
    /// </summary>
    private float LastAttack;

    /// <summary>
    /// The speed at which to add to the current speed.
    /// </summary>
    private float RandomSpeed;

    /// <summary>
    /// The RefreshRate is how long between each check for update.
    /// </summary>
    private float RefreshRate = 2f;

    /// <summary>
    /// This animals type, whether it is aggressive, passive(Coming soon) or scared
    /// </summary>
    public enum AnimalType
    {
        aggressive = 0,
        scared = 1
    }

    /// <summary>
    /// Current state of the AI
    /// </summary>
    public enum CurrentAIState
    {
        Idling = 0,
        GoingHome = 1
    }

    /// <summary>
    /// Current path situation.
    /// </summary>
    public enum PathType
    {
        none = 0,
        HasPath = 1,
        RequestingPath = 2
    }

    /// <summary>
    /// Type of alert
    /// </summary>
    public enum AlertType
    {
        Danger = 0,
        Target = 1
    }

    /// <summary>
    /// Used to pause the current AI from finding a path.
    /// </summary>
    public bool Pause
    {
        get
        {
            return _pause;
        }
        set
        {
            _pause = value;
            if (value)
            {
                EndOfPath();
            }
        }
    }

    /// <summary>
    /// Returns whether or not the AI component has a path.
    /// </summary>
    public bool hasPath
    {
        get
        {
            return path != null;
        }
    }

    /// <summary>
    /// The AI's current target.
    /// </summary>
    public GameObject target
    {
        get
        {
            return _target;
        }
        set
        {
            _target = value;

            Flocking flock = GetComponent<Flocking>();
            if (flock != null && flock.master != null)
            {
                if (flock.master.GetComponent<AdvancedAI>().isDanger(_target))
                {
                    flock.master.GetComponent<AdvancedAI>().Alert(value, AlertType.Target);
                }
                else
                {
                    flock.master.GetComponent<AdvancedAI>().Alert(value, AlertType.Target);
                }
            }
        }
    }

    /// <summary>
    /// Find a path to a specific vector3.
    /// </summary>
    /// <param name="end">The Vector3 location.</param>
    public void FindAPath(Vector3 end)
    {
        GridManager.RequestPath(transform.position, end, OnPathComplete, grid);
    }

    /// <summary>
    /// Alerts the master of a nearby enemy.
    /// </summary>
    /// <param name="Target">The target to alert the master of.</param>
    /// <param name="type">If the target is dangerous or not.</param>
    public void Alert(GameObject Target, AlertType type)
    {
        Flocking flock = GetComponent<Flocking>();
        if (!flock)
        {
            return;
        }
        if (target == Target || Target == null || flock.IsFlockMember(Target)) //Already the current target.
        {
            return;
        }
        if (!flock || flock.IsMaster)
        {
            target = Target;
            FindAPath(Target.transform.position);
            return;
        }
        else
        {
            AdvancedAI ai = flock.master.GetComponent<AdvancedAI>();
            ai.target = Target;
            ai.FindAPath(Target.transform.position);
        }
        if (GetComponent<AudioSource>())
        {
            GetComponent<AudioSource>().Play();
        }
    }

    /// <summary>
    /// Deal "dmg" damage to the target.
    /// </summary>
    /// <param name="dmg">Amount of damage to deal.</param>
    private void Attack(float dmg)
    {
        if (target == null)
        {
            return;
        }
        if (target.GetComponent<AdvancedAI>())
        {
            AIData targetData = target.GetComponent<AIData>();
            targetData.Health -= dmg;
            if (targetData.Health == 0)
            {
                Data.Hunger += 10 * target.GetComponent<AdvancedAI>().Size;
            }
        }
        else if (target.tag == "Player")
        {
            Debug.LogError("Functionality for attacking player not added.");
        }
    }

    /// <summary>
    /// Called when the end of the path has been reached.
    /// </summary>
    private void EndOfPath()
    {
        AIState = CurrentAIState.Idling;
        path = null;
        pt = PathType.none;
        currentWaypoint = 0;
    }

    private void FindAirPath()
    {
        path = new Path(grid);
        path.Vector3Path = new Vector3[1];

        Flocking flock = GetComponent<Flocking>();
        if (!flock || !flock.IsMaster)
        {
            path.Vector3Path[0] = transform.position + (grid.Vector2ToVector3(Random.insideUnitCircle * ViewDistance));
        }
        else if (flock.master != null)
        {
            path.Vector3Path[0] = flock.master.transform.position + (grid.Vector2ToVector3(Random.insideUnitCircle * ViewDistance));
        }

        grid.Clamp(path.Vector3Path[0]);
        if (grid.gridType == GridGraph.GridType.Plane)
        {
            Vector3 start = path.Vector3Path[0];
            Vector3 end = start;
            start.y = 0;
            end.y = 500;
            RaycastHit ray;

            if (Physics.Linecast(start, end, out ray))
            {
                path.Vector3Path[0].y = ray.point.y + Flyheight + Random.Range(-5f, 5f);
            }
        }
        else
        {
            Flyheight = Mathf.Clamp(Flyheight, 0, grid.Radius);

            path.Vector3Path[0] = Random.onUnitSphere * Flyheight;
        }
    }

    private void FindAirPath(Vector3 position)
    {
        path = new Path(grid);
        path.Vector3Path = new Vector3[1];

        path.Vector3Path[0] = position;
    }

    /// <summary>
    /// Finds the closest player/AI component which meets the requirements.
    /// </summary>
    /// <returns>Returns the closest GameObject.</returns>
    private GameObject FindClosest()
    {
        GameObject Closest = null;
        int i = 0;
        foreach (Collider collider in Physics.OverlapSphere(transform.position, ViewDistance))
        {
            if (collider.transform != transform && (collider.tag == "Player" || collider.GetComponent<AdvancedAI>()))
            {
                if (GetComponent<Flocking>() && GetComponent<Flocking>().IsFlockMember(collider.gameObject))
                {
                    continue;
                }
                float angle = Mathf.Abs(Vector3.Angle(transform.forward, collider.transform.position - transform.position));
                if (angle <= 35) //Less than field of view.
                {
                    if (Closest == null)
                    {
                        Closest = collider.gameObject;
                    }
                    else if (Vector3.Distance(transform.position, Closest.transform.position) <= Vector3.Distance(transform.position, collider.transform.position))
                    {
                        Closest = collider.gameObject;
                    }
                }
            }
            i++;
        }
        return Closest;
    }

    /// <summary>
    /// Find the opposite position compared to the agent.
    /// </summary>
    /// <param name="gobj">The position to find the opposite of.</param>
    /// <returns>The opposite position.</returns>
    private Vector3 FindOpposite(Vector3 gobj)
    {
        Vector3 offset = transform.position - gobj;
        offset.x = Mathf.Clamp(Mathf.Abs(offset.x) / ViewDistance, -1.0f, 1.0f);
        offset.z = Mathf.Clamp(Mathf.Abs(offset.z) / ViewDistance, -1.0f, 1.0f);

        return transform.position - (offset * ViewDistance);
    }

    /// <summary>
    /// This function selects a way of finding the path based on the master distance, the current
    /// state of the AI. This is the main function of this script, it handles the AIs behavior. When
    /// it has found the method it sends it to the GridManager, which then processes it and send it
    /// back to the path variable in this script.
    /// </summary>
    private void FindPath()
    {
        if (Pause)
        {
            return;
        }
        Flocking flock = GetComponent<Flocking>();
        grid = GridManager.Grids[Mathf.Clamp(GridIndex, 0, GridManager.Grids.Count - 1)];
        bool t = false;
        if (flock && !flock.IsMaster && flock.DistanceMaster() > ViewDistance) //If the agent can't see the master.
        {
            target = null;
            t = true;
        }
        if (target == null || Flying)
        {
            if (Flying) //If flying is enabled
            {
                if (target = FindClosest()) //Check if a target is visible
                {
                    if (Type == AnimalType.aggressive) //If the animal is aggressive.
                    {
                        FindAirPath(target.transform.position); //Find a path to the target.
                    }
                    else if (Type == AnimalType.scared) //If the animal is scared.
                    {
                        FindAirPath(FindOpposite(target.transform.position)); //Find a path away from the target.
                    }
                }
                if (path == null) //If there wasn't any target visible.
                {
                    FindAirPath(); //Find an idle path.
                }
                return;
            }

            target = FindClosest(); //See if a target is visible.
            if (target == null) //If not try smelling for something.
            {
                target = Smell();
            }

            if (Type == AnimalType.aggressive) //If the animal is aggressive.
            {
                if (target != null && 100.0f - Data.Hunger <= target.GetComponent<AdvancedAI>().Size) //If hungry
                {
                    FindAPath(target.transform.position); //Find a path to the target.
                    return;
                }
            }
            else
            {
                FindAPath(FindOpposite(target.transform.position)); //If it is a scared animal run away from the danger.
                return;
            }

            if (AIState == CurrentAIState.Idling) //If the AI state is set to idling.
            {
                if (pt == PathType.none || t) //If there weren't any path, or there is a
                {
                    float dist = -ViewDistance / 2;

                    if (!flock || flock.IsMaster) //If I am the master.
                    {
                        FindAPath(transform.position + (grid.Vector2ToVector3(Random.insideUnitCircle * dist))); //Move normally
                    }
                    else if (flock.master != null)
                    {
                        FindAPath(flock.master.transform.position + (grid.Vector2ToVector3(Random.insideUnitCircle * dist))); //Move to the master.
                    }
                }
                return;
            }
            else if (AIState == CurrentAIState.GoingHome) //If the agent is planning on going home.
            {
                if (pt == PathType.none) //If there wasn't any path.
                {
                    FindAPath(Data.Home); //Find the quickest path home.
                }
                return;
            }
        }
        else if (pt == PathType.none) //If there is a target and there isn't any path, check if the target is still valid.
        {
            if (Physics.Linecast(transform.position, target.transform.position)) //If it still is visible.
            {
                FindAPath(target.transform.position); //Find a path to the target.
                return;
            }
            else
            {
                target = null; //Unset the target if it wasn't valid.
            }
        }
    }

    /// <summary>
    /// FixedUpdate
    /// </summary>
    private void FixedUpdate()
    {
        if (grid.gridType == GridGraph.GridType.Plane)
        {
            RaycastHit hit;
            if (Physics.Linecast(transform.position, transform.position + new Vector3(0, 500, 0), out hit))
            {
                hit.point += new Vector3(0, 5, 0);
                transform.position = hit.point;
            }
        }
        if (path == null) //If the path doesn't exist.
        {
            return;
        }

        currentWaypoint = Mathf.Clamp(currentWaypoint, 0, path.Vector3Path.Length - 1); //Clamp the current waypoint to prevent errors.

        if (Vector3.Distance(transform.position, path.Vector3Path[currentWaypoint]) < AttackRange) //If the target is closer than attack range update the current waypoint.
        {
            currentWaypoint++; //Increment the currentWaypoint variable.
            if (currentWaypoint >= path.Vector3Path.Length) //If the currentWaypoint is equal to the path length end the pathfinding.
            {
                EndOfPath(); //Call the end of path function.
                return;
            }
        }

        if (Flying) //If flying look at 45 degree angle downwards.
        {
            transform.LookAt(path.Vector3Path[currentWaypoint]);
            Vector3 roto = transform.rotation.eulerAngles;
            roto.x = 45;
            transform.rotation = Quaternion.Euler(roto);
        }
        else if (!GetComponent<GridAlign>()) //Else just look at the next waypoint.
        {
            Vector3 rot = path.Vector3Path[currentWaypoint];
            if (grid.gridType == GridGraph.GridType.Plane)
            {
                rot.y = transform.position.y;
            }
            transform.LookAt(rot);
        }

        if (!Pause)
        {
            if (RandomAddSpeed) //Add random variable between -1 and 1 to make some variation in the movement.
            {
                transform.position = Vector3.MoveTowards(transform.position, path.Vector3Path[currentWaypoint], Time.fixedDeltaTime * (speed + RandomSpeed));
            }
            else //Else move normally.
            {
                transform.position = Vector3.MoveTowards(transform.position, path.Vector3Path[currentWaypoint], Time.fixedDeltaTime * speed);
            }
        }
    }

    /// <summary>
    /// If target is a danger to this animal.
    /// </summary>
    /// <param name="target"></param>
    /// <returns>True or false depending on whether or not it is a danger.</returns>
    private bool isDanger(GameObject target)
    {
        if (target != null)
        {
            AdvancedAI targetAI = target.GetComponent<AdvancedAI>(); //Reference to the targets AdvancedAI component.
            if (targetAI == null)
            {
                return true; //It is a danger.
            }
            if (Size < targetAI.Size && targetAI.Type == AnimalType.aggressive) //Be scared if the animal is bigger and aggressive
            {
                return true; //It is a danger.
            }
            else if (Size < targetAI.Size + 20 && targetAI.Type == AnimalType.scared) //Be scared if much bigger than me.
            {
                return true; //It is a danger.
            }
        }
        return false; //It is not a danger.
    }

    /// <summary>
    /// Used to draw the path gizmo's.
    /// </summary>
    private void OnDrawGizmos()
    {
        //Path
        if (GridManager.ShowPath && path != null)
        {
            for (int i = currentWaypoint; i < path.Vector3Path.Length; i++)
            {
                Gizmos.color = Color.cyan;
                Gizmos.DrawCube(path.Vector3Path[i], Vector3.one);

                if (i == currentWaypoint)
                {
                    Gizmos.DrawLine(transform.position, path.Vector3Path[i]);
                }
                else
                {
                    Gizmos.DrawLine(path.Vector3Path[i - 1], path.Vector3Path[i]);
                }
            }
        }

        if (GridManager.ShowGizmo)
        {
            Color c = Color.green;

            if (GetComponent<Flocking>() && FindObjectOfType<GridManager>().ShowFlockColor)
            {
                int se = Random.seed;
                Random.seed = GetComponent<Flocking>().FlockID;
                c = new Color(Random.Range(0f, 1f), Random.Range(0f, 1f), Random.Range(0f, 1f));
                Random.seed = se;
            }
            if (GetComponent<Smelling>() && GetComponent<Smelling>().Smell() != null)
            {
                Gizmos.color = Color.red;
            }
            else
            {
                Gizmos.color = c;
            }
            Gizmos.DrawWireSphere(transform.position - (Wind.windVector3 * GetComponent<Smelling>().SmellDistance), GetComponent<Smelling>().SmellDistance);

            if (GetComponent<Listener>() && GetComponent<Listener>().audioValue != 0.0f)
            {
                Gizmos.DrawWireSphere(transform.position, GetComponent<Listener>().audioValue * GetComponent<AudioSource>().maxDistance);
            }

            Gizmos.matrix = Matrix4x4.TRS(transform.position, transform.rotation, Vector3.one);

            //Looking
            if (FindClosest() != null)
            {
                Gizmos.color = Color.red;
            }
            else
            {
                Gizmos.color = c;
            }
            Gizmos.DrawFrustum(Vector3.zero, 35.0f, ViewDistance, 0.0f, 2.5f);
        }
    }

    /// <summary>
    /// Called when the path has been created.
    /// </summary>
    /// <param name="p">The returned path.</param>
    private void OnPathComplete(Path p)
    {
        if (p.Success)
        {
            pt = PathType.HasPath;
            path = p;
            currentWaypoint = 0;
        }
        else
        {
            pt = PathType.none;
        }
    }

    /// <summary>
    /// Used to determine whether or not an animal is within smelling range.
    /// </summary>
    /// <returns></returns>
    private GameObject Smell()
    {
        if (GetComponent<Smelling>())
        {
            return GetComponent<Smelling>().Smell();
        }
        return null;
    }

    private void Start()
    {
        RandomSpeed = Random.Range(-2.0f, 2.0f);

        Data = GetComponent<AIData>();
        Data.Home = transform.position;

        Mesh mesh;
        if (FindObjectOfType<SkinnedMeshRenderer>())
        {
            mesh = FindObjectOfType<SkinnedMeshRenderer>().sharedMesh;
            Size = mesh.bounds.size.x + mesh.bounds.size.y + mesh.bounds.size.z;
        }
        else if (FindObjectOfType<MeshFilter>())
        {
            mesh = FindObjectOfType<MeshFilter>().mesh;
            Size = mesh.bounds.size.x + mesh.bounds.size.y + mesh.bounds.size.z;
        }
        else
        {
            Size = 3;
        }
        if (Flying)
        {
            InvokeRepeating("FindPath", Random.Range(0.0f, RefreshRate), RefreshRate / 2);
        }
        else
        {
            InvokeRepeating("FindPath", Random.Range(0.0f, RefreshRate), RefreshRate);
        }
        StartCoroutine(UpdateState()); //Start state updater
    }

    private void Update()
    {
        if (!grid.Scanning && (grid.nodes != null && grid.nodes.Length != 0)) //If not scanning
        {
            if (grid.gridType == GridGraph.GridType.Plane) //If grid is a plane.
            {
                transform.position = grid.Clamp(transform.position); //Clamp the position to the grid.
            }
            else if (Flying) //If sphere and flying
            {
                transform.position = (transform.position - grid.offset).normalized * Flyheight;
            }
        }
        if (Type == AnimalType.aggressive && target != null && Vector3.Distance(transform.position, target.transform.position) <= AttackRange)
        {
            if (Time.realtimeSinceStartup - LastAttack > 1.0f)
            {
                LastAttack = Time.realtimeSinceStartup;
                Attack(Damage);
            }
        }
        if (GridManager.Contains(OnPathComplete))
        {
            pt = PathType.RequestingPath;
        }
        else if (path == null)
        {
            pt = PathType.none;
        }
        else
        {
            pt = PathType.HasPath;
        }
    }

    private IEnumerator UpdateState()
    {
        float LastCheck = 0.0f;
        while (true)
        {
            if (Time.realtimeSinceStartup - LastCheck >= 30.0f)
            {
                LastCheck = Time.realtimeSinceStartup;
                float chan = Random.Range(0.0f, 100.0f);
                if (AIState == CurrentAIState.GoingHome && Vector3.Distance(transform.position, Data.Home) < AttackRange)
                {
                    AIState = CurrentAIState.Idling;
                }
                else if (AIState == CurrentAIState.Idling)
                {
                    if (chan < 5f)
                    {
                        AIState = CurrentAIState.GoingHome;
                    }
                    else
                    {
                        AIState = CurrentAIState.Idling;
                    }
                }
            }
            yield return null;
        }
    }
}