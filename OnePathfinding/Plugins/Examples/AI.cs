using Pathfinding;
using System.Collections;
using UnityEngine;
using Random = UnityEngine.Random;

[RequireComponent(typeof(AIData))]
public class AI : MonoBehaviour
{
    [HideInInspector]
    public GameObject _target;

    public CurrentAIState AIState;
    public AudioClip AlertSound;
    public float Damage;
    public bool FlockAnimal;
    public int FlockID;
    public GameObject FlockMember;
    public int FlockSize;
    public bool Flying = false;
    public bool IsMaster;

    [HideInInspector]
    public GameObject master;

    public int maxFlockSize = 3;
    public int minFlockSize = 2;
    public bool RandomAddSpeed = true;

    [HideInInspector]
    public float Size;

    public float SmellDistance = 5f;
    public float speed = 6.0F;
    public float TargetAngle;
    public AnimalType Type;
    public float ViewDistance = 50.0f;
    private bool _LockTarget = false;
    private float AttackRange = 2.5f;
    private GameObject Closest;
    private Vector3 currentWay;
    private int currentWaypoint = 0;
    private AIData Data;
    private GameObject f;
    private Vector3 flyTarget;
    private string ID;
    private float LastCheck = 0.0f;
    private Vector3 MasterPosition;
    private Path path = null;
    private float RandomSpeed;
    private float RefreshRate = 2f;
    private GameObject t;

    public enum AlertType
    {
        Danger = 0,
        Target = 1
    }

    public enum AnimalType
    {
        aggresive = 0,
        scared = 1
    }

    public enum CurrentAIState
    {
        Idling = 0,
        GoingHome = 1,
        FindingFood = 2
    }

    /// <summary>
    /// Returns whether or not the ai component has a path.
    /// </summary>
    public bool hasPath
    {
        get
        {
            if (path != null)
            {
                return true;
            }
            return false;
        }
    }

    public GameObject target
    {
        get
        {
            return _target;
        }
        set
        {
            _target = value;
            if (value != null)
            {
                if (master != null)
                {
                    if (_target.GetComponent<AI>() != null && ((_target.GetComponent<AI>().Size > Size)))
                    {
                        Alert(_target, AlertType.Danger);
                    }
                    else
                    {
                        Alert(_target, AlertType.Target);
                    }
                }

                LockTarget = true;
            }
        }
    }

    private bool LockTarget
    {
        get
        {
            return _LockTarget;
        }
        set
        {
            _LockTarget = value;
            if (!value)
            {
                target = null;
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
        if (master == null)
        {
            return;
        }
        if (AlertSound != null && GetComponent<AudioSource>() != null)
        {
            GetComponent<AudioSource>().PlayOneShot(AlertSound);
        }

        AI ai = master.GetComponent<AI>();
        ai.target = Target;
        ai.FindAPath(Target.transform.position);
        LockTarget = true;
    }

    /// <summary>
    /// Used to determine how far away the master is.
    /// </summary>
    /// <returns>The distance to the master. Returns -1 if no master is found.</returns>
    public float DistanceMaster()
    {
        if (master == null || !FlockAnimal)
        {
            return -1f;
        }
        if (master != null && path != null)
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
        GridManager.RequestPath(transform.position, end, OnPathComplete, gameObject.GetHashCode().ToString());
    }

    /// <summary>
    /// Find a path to a specific transform.
    /// </summary>
    /// <param name="end">The transform of the location.</param>
    public void FindAPath(Transform end)
    {
        GridManager.RequestPath(transform.position, end.position, OnPathComplete, gameObject.GetHashCode().ToString());
    }

    /// <summary>
    /// Checks if target is still in-range.
    /// </summary>
    /// <param name="obj">GameObject used for checking.</param>
    /// <returns></returns>
    public bool IsClose(GameObject obj)
    {
        if (obj == null)
        {
            return false;
        }
        if (Vector3.Distance(transform.position, obj.transform.position) < ViewDistance)
        {
            if (Physics.Raycast(transform.position, obj.transform.position, ViewDistance))
            {
                //I can see the target
                return true;
            }
            else if (Smell() != null)
            {
                //I can smell the target
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// Checks if this gameobject and the subject is from the same flock.
    /// </summary>
    /// <param name="subject">The gameobject to test against.</param>
    /// <returns>Bool, telling whether or not they are from the same flock.</returns>
    public bool IsFlockMember(GameObject subject)
    {
        if (subject.tag == "AI" && subject.GetComponent<AI>().FlockID == FlockID)
        {
            return true;
        }
        return false;
    }

    public bool PromoteNewLeader()
    {
        if (!FlockAnimal)
        {
            return false;
        }

        bool found = false;

        foreach (AI ai in FindObjectsOfType<AI>())
        {
            if (ai.FlockID == FlockID)
            {
                if (!found)
                {
                    ai.IsMaster = true;
                    f = ai.gameObject;
                    found = true;
                }
                else if (f != null)
                {
                    ai.master = f;
                    f = null;
                }
            }
        }

        return found;
    }

    public GameObject Smell()
    {
        Collider[] col = Physics.OverlapSphere(transform.position - (Wind.windVector3 * SmellDistance), SmellDistance);
        foreach (Collider collider in col)
        {
            if (collider.tag == "AI" || collider.tag == "Player")
            {
                if (IsFlockMember(collider.gameObject))
                {
                    continue;
                }
                if (collider.transform != transform)
                {
                    transform.LookAt(collider.transform.position);
                    return collider.gameObject;
                }
            }
        }
        return null;
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
        if (target.tag == "AI")
        {
            target.GetComponent<AIData>().Health -= dmg;
            if (target.GetComponent<AIData>().Health == 0)
            {
                Data.Hunger += 10 * target.GetComponent<AI>().Size;
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
        if (Type == AnimalType.aggresive && target != null && Vector3.Distance(transform.position, target.transform.position) <= AttackRange)
        {
            Attack(Damage);
        }

        AIState = CurrentAIState.Idling;
        LockTarget = false;
        path = null;
        currentWaypoint = 0;
    }

    private Vector3 FindAirPath()
    {
        Vector3 go = new Vector3(Random.Range(-ViewDistance, ViewDistance), 0, Random.Range(-ViewDistance, ViewDistance)) + transform.position;
        Vector3 size = Terrain.activeTerrain.terrainData.size;
        go.y = Terrain.activeTerrain.terrainData.GetInterpolatedHeight(go.x / size.x, go.z / size.z);
        go.y += 100;
        return go;
    }

    /// <summary>
    /// Finds the closest player/AI component which meets the requirements.
    /// </summary>
    /// <returns>Returns the closest gameobject.</returns>
    private GameObject FindClosest()
    {
        Closest = null;
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, ViewDistance);
        int i = 0;
        foreach (Collider collider in hitColliders)
        {
            if (collider.transform != transform && (collider.tag == "Player" || collider.tag == "AI"))
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

    private void FindPath()
    {
        if (Terrain.activeTerrain != null)
        {
            bool t = false;
            if (DistanceMaster() > ViewDistance)
            {
                LockTarget = false;
                t = true;
            }
            if (Flying)
            {
                flyTarget = FindAirPath();
                return;
            }
            else if (!LockTarget)
            {
                target = FindClosest();

                if (target != null)
                {
                    FindAPath(target.transform.position);
                    return;
                }
                target = Smell();
                if (target != null)
                {
                    FindAPath(target.transform.position);
                    return;
                }

                if (AIState == CurrentAIState.Idling)
                {
                    if (path == null || t)
                    {
                        if (target == null)
                        {
                            float min = -ViewDistance / 2;
                            float max = ViewDistance / 2;
                            Vector3 GridPos = new Vector3(Random.Range(min, max), 0, Random.Range(min, max));

                            if (IsMaster || !FlockAnimal)
                            {
                                FindAPath(transform.position + GridPos);
                            }
                            else
                            {
                                FindAPath(master.transform.position + GridPos);
                            }
                        }
                    }
                    return;
                }
                else if (AIState == CurrentAIState.GoingHome)
                {
                    if (path == null)
                    {
                        FindAPath(Data.Home);
                    }
                    return;
                }
                else if (AIState == CurrentAIState.FindingFood)
                {
                    target = FindClosest();
                    if (target != null)
                    {
                        FindAPath(target.transform.position);
                    }
                    return;
                }
            }
            else
            {
                if (target != null)
                {
                    transform.LookAt(target.transform.position);
                    target = FindClosest();
                    if (target != null)
                    {
                        FindAPath(target.transform.position);
                        return;
                    }
                }
                LockTarget = false;
            }
        }
    }

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

        if (GridManager.ShowGizmos)
        {
            Color c = Color.green;

            if (FindObjectOfType<GridManager>().ShowFlockColor && FlockAnimal)
            {
                int se = Random.seed;
                Random.seed = FlockID;
                c = new Color(Random.Range(0f, 1f), Random.Range(0f, 1f), Random.Range(0f, 1f));
                Random.seed = se;
            }

            //Smelling
            if (Smell() != null)
            {
                Gizmos.color = Color.red;
            }
            else
            {
                Gizmos.color = c;
            }
            Gizmos.DrawWireSphere(transform.position - (Wind.windVector3 * SmellDistance), SmellDistance);

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

    private void OnPathComplete(Path p)
    {
        if (p.Success)
        {
            path = p;
            currentWaypoint = 0;
        }
    }

    private void Start()
    {
        RandomSpeed = Random.Range(-2.0f, 2.0f);

        Data = GetComponent<AIData>();
        Data.Home = transform.position;

        tag = "AI";

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

        InvokeRepeating("FindPath", Random.Range(0.0f, RefreshRate), RefreshRate);
        StartCoroutine(UpdateState()); //Start state updater

        if (master == null && FlockAnimal)
        {
            IsMaster = true;
        }
    }

    private void Update()
    {
        if (Flying)
        {
            if (flyTarget != Vector3.zero)
            {
                transform.position = Vector3.MoveTowards(transform.position, flyTarget, Time.deltaTime * speed);
                if (Vector3.Distance(transform.position, flyTarget) < 1.0f)
                {
                    flyTarget = Vector3.zero;
                }
            }
            return;
        }
        if (path == null)
        {
            return;
        }

        if (Vector3.Distance(transform.position, currentWay) < 1.0f)
        {
            currentWaypoint++;
            if (currentWaypoint >= path.Vector3Path.Length)
            {
                EndOfPath();
                return;
            }
        }
        currentWay = path.Vector3Path[currentWaypoint];

        Vector3 rot = currentWay;

        rot.y = transform.position.y;

        transform.LookAt(rot);
        if (RandomAddSpeed)
        {
            transform.position = Vector3.MoveTowards(transform.position, currentWay, Time.deltaTime * (speed + RandomSpeed));
        }
        else
        {
            transform.position = Vector3.MoveTowards(transform.position, currentWay, Time.deltaTime * speed);
        }
    }

    private IEnumerator UpdateState()
    {
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
                else if (Data.Hunger < 20)
                {
                    AIState = CurrentAIState.FindingFood;
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