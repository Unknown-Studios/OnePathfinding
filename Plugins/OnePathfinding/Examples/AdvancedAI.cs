using OnePathfinding;
using System.Collections;
using UnityEngine;
using Random = UnityEngine.Random;

[RequireComponent(typeof(AIData))]
[RequireComponent(typeof(AudioSource))]
public class AdvancedAI : MonoBehaviour
{
    [HideInInspector]
    public GameObject _target;

    public CurrentAIState AIState;
    public AudioClip AlertSound;
    public bool automatedNoise;
    public float Damage;
    public bool FlockAnimal;
    public int FlockID;
    public GameObject FlockMember;
    public float Flyheight = 50.0f;
    public bool Flying = false;
    public bool IsMaster;

    public GameObject master;

    public int maxFlockSize = 3;
    public int minFlockSize = 2;
    public float NoiseDistance = 100.0f;
    public Path path = null;
    public bool RandomAddSpeed = true;

    public float Size;

    public float SmellDistance = 5f;
    public float speed = 6.0F;
    public float TillNoise;
    public AnimalType Type;
    public float ViewDistance = 50.0f;
    private bool _LockTarget = false;
    private float AttackRange = 2.5f;
    private new AudioSource audio;
    private float audioValue;
    private int currentWaypoint = 0;
    private AIData Data;

    private float LastAttack;
    private float RandomSpeed;
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
    /// What type of animal it is, whether it is agressive or not.
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
            if (value != null)
            {
                if (master != null)
                {
                    if (_target.GetComponent<AdvancedAI>() != null && ((_target.GetComponent<AdvancedAI>().Size > Size)))
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
        if (target == Target)
        {
            return;
        }
        if (master == null)
        {
            LockTarget = false;
            target = Target;
            FindAPath(Target.transform.position);
            LockTarget = true;
            return;
        }
        if (AlertSound != null && audio != null)
        {
            audio.PlayOneShot(AlertSound);
        }

        AdvancedAI ai = master.GetComponent<AdvancedAI>();
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
    }

    /// <summary>
    /// Checks if this gameobject and the subject is from the same flock.
    /// </summary>
    /// <param name="subject">The gameobject to test against.</param>
    /// <returns>Bool, telling whether or not they are from the same flock.</returns>
    public bool IsFlockMember(GameObject subject)
    {
        if (subject.tag == "AI")
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
    /// Alert all the surounding objects, when the object makes a noise.
    /// </summary>
    private void AlertObjects()
    {
        if (Type == AnimalType.aggresive)
        {
            return;
        }
        Collider[] colliders = Physics.OverlapSphere(transform.position, audioValue * audio.maxDistance);
        foreach (Collider col in colliders)
        {
            if (!IsFlockMember(col.gameObject))
            {
                continue;
            }
            if (col.tag == "AI" && col.GetComponent<AdvancedAI>().Size < Size)
            {
                col.GetComponent<AdvancedAI>().Alert(col.gameObject, AlertType.Danger);
            }
        }
    }

    private float AnalyzeSound()
    {
        float max = 0;
        float[] samples = new float[1024];
        audio.GetOutputData(samples, 0);
        for (int i = 0; i < samples.Length; i++)
        {
            if (samples[i] > max)
            {
                max = samples[i];
            }
        }
        return max;
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
                GetComponent<AIData>().Hunger += 10 * target.GetComponent<AdvancedAI>().Size;
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
        LockTarget = false;
        path = null;
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
    /// <returns>Returns the closest gameobject.</returns>
    private GameObject FindClosest()
    {
        GameObject Closest = null;
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

    private Vector3 FindOpposite(Vector3 gobj)
    {
        Vector3 offset = transform.position - gobj;
        offset.x = Mathf.Clamp(Mathf.Abs(offset.x) * 100f, -1.0f, 1.0f);
        offset.z = Mathf.Clamp(Mathf.Abs(offset.z) * 100f, -1.0f, 1.0f);

        return transform.position - (offset * 50f);
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
            else if (!LockTarget)
            {
                if (Flying)
                {
                    if (target = FindClosest())
                    {
                        FindAirPath(target.transform.position);
                    }
                    if (path == null)
                    {
                        FindAirPath();
                    }
                    return;
                }

                target = FindClosest();
                if (target != null)
                {
                    target = Smell();
                }

                if (target != null && 100.0f - Data.Hunger <= target.GetComponent<AdvancedAI>().Size)
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
                            else if (master != null)
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
            }
            else
            {
                if (target != null)
                {
                    //Check if target is still valid.
                    transform.LookAt(target.transform.position);
                    if (Flying)
                    {
                        Vector3 rot = transform.rotation.eulerAngles;
                        rot.x = 45;
                        transform.rotation = Quaternion.Euler(rot);
                    }

                    target = FindClosest();
                    if (target != null)
                    {
                        if (Type == AnimalType.scared || (target.tag == "AI" && target.GetComponent<AdvancedAI>().Size > Size))
                        {
                            Vector3 loc = FindOpposite(target.transform.position);
                            FindAPath(loc);
                        }
                        else
                        {
                            FindAPath(target.transform.position);
                        }
                        return;
                    }
                }
                LockTarget = false;
            }
        }
    }

    private IEnumerator NoiseMaker()
    {
        if (automatedNoise)
        {
            float LastNoise = 0f;
            float ran = Random.Range(0.0f, 100.0f);
            while (true)
            {
                TillNoise = (Time.realtimeSinceStartup - ran) - LastNoise;
                if (Time.realtimeSinceStartup - LastNoise > ran)
                {
                    audio.clip = AlertSound;
                    audio.Play();
                    LastNoise = Time.realtimeSinceStartup;
                    ran = Random.Range(0.0f, 600.0f);
                }
                yield return null;
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

        if (GridManager.ShowGizmo)
        {
            if (audio != null && audioValue != 0f)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawWireSphere(transform.position, audioValue * audio.maxDistance);
            }
            Color c = Color.green;

            if (FindObjectOfType<GridManager>().ShowFlockColor && FlockAnimal)
            {
                int se = Random.seed;
                Random.seed = FlockID;
                c = new Color(Random.Range(0f, 1f), Random.Range(0f, 1f), Random.Range(0f, 1f));
                Random.seed = se;
            }

            if (!Flying)
            {
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

    private void OnPathComplete(Path p)
    {
        if (p.Success)
        {
            path = p;
            currentWaypoint = 0;
            if (Flying)
            {
                FindAirPath();
            }
        }
    }

    private GameObject Smell()
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
                    return collider.gameObject;
                }
            }
        }
        return null;
    }

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

                o++;
            }
        }
    }

    private void Start()
    {
        audio = GetComponent<AudioSource>();
        audio.maxDistance = NoiseDistance;
        audio.minDistance = 0f;
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
        if (Flying)
        {
            InvokeRepeating("FindPath", Random.Range(0.0f, RefreshRate), RefreshRate / 2);
        }
        else
        {
            InvokeRepeating("FindPath", Random.Range(0.0f, RefreshRate), RefreshRate);
        }
        StartCoroutine(UpdateState()); //Start state updater
        StartCoroutine(NoiseMaker());

        if (master == null && FlockAnimal)
        {
            IsMaster = true;
        }
    }

    private void Update()
    {
        if (audio != null)
        {
            audioValue = AnalyzeSound();
            if (audioValue != 0.0f)
            {
                AlertObjects();
            }
        }
        if (Type == AnimalType.aggresive && target != null && Vector3.Distance(transform.position, target.transform.position) <= AttackRange)
        {
            if (Time.realtimeSinceStartup - LastAttack > 1.0f)
            {
                LastAttack = Time.realtimeSinceStartup;
                Attack(Damage);
            }
        }
        if (path == null)
        {
            return;
        }

        if (Vector3.Distance(transform.position, path.Vector3Path[currentWaypoint]) < 1.0f)
        {
            currentWaypoint++;
            if (currentWaypoint >= path.Vector3Path.Length)
            {
                EndOfPath();
                return;
            }
        }

        Vector3 rot = path.Vector3Path[currentWaypoint];
        rot.y = transform.position.y;

        transform.LookAt(rot);

        if (Flying)
        {
            Vector3 roto = transform.rotation.eulerAngles;
            roto.x = 45;
            transform.rotation = Quaternion.Euler(roto);
        }

        if (RandomAddSpeed)
        {
            transform.position = Vector3.MoveTowards(transform.position, path.Vector3Path[currentWaypoint], Time.deltaTime * (speed + RandomSpeed));
        }
        else
        {
            transform.position = Vector3.MoveTowards(transform.position, path.Vector3Path[currentWaypoint], Time.deltaTime * speed);
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