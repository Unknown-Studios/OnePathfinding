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
    /// If the animal is a flock animal or not.
    /// </summary>
    public bool FlockAnimal;

    /// <summary>
    /// An ID used to identify the flock leader and it's members.
    /// </summary>
    public int FlockID;

    /// <summary>
    /// If you want a custom GameObject to be the child in a flock, if you say have a alpha wolf
    /// with a different model than the rest of the, you would then set this to child's model.
    /// </summary>
    public GameObject FlockMember;

    /// <summary>
    /// The height at which a flying AI will fly at.
    /// </summary>
    public float Flyheight = 50.0f;

    /// <summary>
    /// Whether it is a flying animal or not.
    /// </summary>
    public bool Flying = false;

    /// <summary>
    /// This is used to easily identify if this AI is the master of it's flock.
    /// </summary>
    public bool IsMaster;

    /// <summary>
    /// A reference to the master GameObject. NULL if not in a flock or if this is the leader.
    /// </summary>
    public GameObject master;

    /// <summary>
    /// The minimum size this AI's flock can have.
    /// </summary>
    public int maxFlockSize = 3;

    /// <summary>
    /// The maximum size this AI's flock can have.
    /// </summary>
    public int minFlockSize = 2;

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
    /// The distance at which this animal can smell at.
    /// </summary>
    public float SmellDistance = 5f;

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
    /// Type of alert
    /// </summary>
    public enum AlertType
    {
        Danger = 0,
        Target = 1
    }

    /// <summary>
    /// This animals type, whether it is aggressive, passive(Coming soon) or scared
    /// </summary>
    public enum AnimalType
    {
        aggresive = 0,
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

            if (master != null)
            {
                if (master.GetComponent<AdvancedAI>().isDanger(_target))
                {
                    master.GetComponent<AdvancedAI>().Alert(value, AlertType.Target);
                }
                else
                {
                    master.GetComponent<AdvancedAI>().Alert(value, AlertType.Target);
                }
            }
        }
    }

    /// <summary>
    /// Alerts the master of a nearby enemy.
    /// </summary>
    /// <param name="Target">The target to alert the master of.</param>
    /// <param name="type">If the target is dangerous or not.</param>
    public void Alert(GameObject Target, AlertType type)
    {
        if (target == Target || Target == null || IsFlockMember(Target)) //Already the current target.
        {
            return;
        }
        if (IsMaster)
        {
            target = Target;
            FindAPath(Target.transform.position);
            return;
        }
        else
        {
            AdvancedAI ai = master.GetComponent<AdvancedAI>();
            ai.target = Target;
            ai.FindAPath(Target.transform.position);
        }
        GetComponent<AudioSource>().Play();
    }

    /// <summary>
    /// Used to determine how far away the master is.
    /// </summary>
    /// <returns>The distance to the master. Returns -1 if no master is found.</returns>
    public float DistanceMaster()
    {
        if (master != null && path != null && FlockAnimal)
        {
            return Vector3.Distance(master.transform.position, path.Destination);
        }
        return -1f;
    }

    /// <summary>
    /// Find a path to a specific vector3.
    /// </summary>
    /// <param name="end">The Vector3 location.</param>
    public void FindAPath(Vector3 end)
    {
        GridManager.RequestPath(transform.position, end, OnPathComplete);
        pt = PathType.RequestingPath;
    }

    /// <summary>
    /// Checks if this GameObject and the subject is from the same flock.
    /// </summary>
    /// <param name="subject">The GameObject to test against.</param>
    /// <returns>Bool, telling whether or not they are from the same flock.</returns>
    public bool IsFlockMember(GameObject subject)
    {
        if (subject.GetComponent<AdvancedAI>())
        {
            if (gameObject.transform == subject.transform || subject.GetComponent<AdvancedAI>().FlockID == FlockID)
            {
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// Promote a new leader of the flock, this would happen if the leader gets killed.
    /// </summary>
    public void PromoteNewLeader()
    {
        GameObject f = null;

        foreach (AdvancedAI ai in FindObjectsOfType<AdvancedAI>())
        {
            if (IsFlockMember(ai.gameObject))
            {
                if (f == null)
                {
                    ai.IsMaster = true;
                    f = ai.gameObject;
                }
                else
                {
                    ai.master = f;
                }
            }
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

    private void Awake()
    {
        FlockID = Mathf.RoundToInt(Random.Range(0, 99999999));
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
        TerrainData td = Terrain.activeTerrain.terrainData;
        path = new Path();
        path.Vector3Path = new Vector3[1];
        float min = -ViewDistance;
        float max = ViewDistance;
        Vector3 GridPos = new Vector3(Random.Range(min, max), 0, Random.Range(min, max));

        if (IsMaster || !FlockAnimal)
        {
            path.Vector3Path[0] = transform.position + GridPos;
        }
        else if (master != null)
        {
            path.Vector3Path[0] = master.transform.position + GridPos;
        }

        path.Vector3Path[0].x = Mathf.Clamp(path.Vector3Path[0].x, 0, td.size.x);
        path.Vector3Path[0].z = Mathf.Clamp(path.Vector3Path[0].z, 0, td.size.z);

        float normX = path.Vector3Path[0].x / td.size.x;
        float normY = path.Vector3Path[0].z / td.size.z;

        path.Vector3Path[0].y = td.GetInterpolatedHeight(normX, normY) + Flyheight + Random.Range(-5f, 0f);
    }

    private void FindAirPath(Vector3 position)
    {
        path = new Path();
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
                if (IsFlockMember(collider.gameObject))
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
        if (Terrain.activeTerrain != null)
        {
            bool t = false;
            if (!IsMaster && DistanceMaster() > ViewDistance) //If the agent can't see the master.
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
                        if (Type == AnimalType.aggresive) //If the animal is aggressive.
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

                if (Type == AnimalType.aggresive) //If the animal is aggressive.
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
                        Vector3 GridPos = new Vector3(Random.Range(-dist, dist), 0, Random.Range(-dist, dist));

                        if (IsMaster) //If I am the master.
                        {
                            FindAPath(transform.position + GridPos); //Move normally
                        }
                        else
                        {
                            FindAPath(master.transform.position + GridPos); //Move to the master.
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
    }

    /// <summary>
    /// FixedUpdate
    /// </summary>
    private void FixedUpdate()
    {
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
        else //Else just look at the next waypoint.
        {
            Vector3 rot = path.Vector3Path[currentWaypoint];
            rot.y = transform.position.y;
            transform.LookAt(rot);
        }

        if (RandomAddSpeed) //Add random variable between -1 and 1 to make some variation in the movement.
        {
            transform.position = Vector3.MoveTowards(transform.position, path.Vector3Path[currentWaypoint], Time.fixedDeltaTime * (speed + RandomSpeed));
        }
        else //Else move normally.
        {
            transform.position = Vector3.MoveTowards(transform.position, path.Vector3Path[currentWaypoint], Time.fixedDeltaTime * speed);
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
            if (Size < targetAI.Size && targetAI.Type == AnimalType.aggresive) //Be scared if the animal is bigger and aggressive
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

            if (FindObjectOfType<GridManager>().ShowFlockColor && FlockAnimal)
            {
                int se = Random.seed;
                Random.seed = FlockID;
                c = new Color(Random.Range(0f, 1f), Random.Range(0f, 1f), Random.Range(0f, 1f));
                Random.seed = se;
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

    /// <summary>
    /// Spawn new member in this flock.
    /// </summary>
    private void SpawnFlockMember()
    {
        if (FlockAnimal)
        {
            int FlockSize = Random.Range(minFlockSize, maxFlockSize);

            int o = 0;
            while (o < FlockSize)
            {
                Vector3 r = new Vector3(Random.Range(-10f, 10f), 0, Random.Range(-10f, 10f));
                GameObject bo = null;
                if (FlockMember != null)
                {
                    bo = (GameObject)Instantiate(FlockMember, transform.position + r, Quaternion.identity);
                }
                else
                {
                    bo = (GameObject)Instantiate(gameObject, transform.position + r, Quaternion.identity);
                }
                bo.GetComponent<AdvancedAI>().master = gameObject;
                bo.GetComponent<AdvancedAI>().FlockID = FlockID;
                bo.name = bo.name.Replace("(Clone)", "");

                bo.transform.SetParent(transform.parent);
                o++;
            }
        }
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

        if (master == null)
        {
            IsMaster = true;
            SpawnFlockMember();
        }
    }

    private void Update()
    {
        if (Type == AnimalType.aggresive && target != null && Vector3.Distance(transform.position, target.transform.position) <= AttackRange)
        {
            if (Time.realtimeSinceStartup - LastAttack > 1.0f)
            {
                LastAttack = Time.realtimeSinceStartup;
                Attack(Damage);
            }
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