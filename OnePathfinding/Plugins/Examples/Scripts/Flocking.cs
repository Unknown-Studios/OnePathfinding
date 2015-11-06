using UnityEngine;

/// <summary>
/// Component used by AdvancedAI.cs for flocking support.
/// </summary>
[RequireComponent(typeof(AdvancedAI))]
public class Flocking : MonoBehaviour
{
    /// <summary>
    /// An ID used to identify the current object.
    /// </summary>
    [HideInInspector]
    public int FlockID;

    /// <summary>
    /// If you want a custom GameObject to be the child in a flock, if you say have a alpha wolf
    /// with a different model than the rest of the, you would then set this to child's model.
    /// </summary>
    [HideInInspector]
    public GameObject FlockMember;

    /// <summary>
    /// This is used to easily identify if this AI is the master of it's flock.
    /// </summary>
    [HideInInspector]
    public bool IsMaster;

    /// <summary>
    /// A reference to the master GameObject. NULL if not in a flock or if this is the leader.
    /// </summary>
    [HideInInspector]
    public GameObject master;

    /// <summary>
    /// The maximum size this AI's flock can have.
    /// </summary>
    [HideInInspector]
    public int maxFlockSize = 3;

    /// <summary>
    /// The minimum size this AI's flock can have.
    /// </summary>
    [HideInInspector]
    public int minFlockSize = 2;

    /// <summary>
    /// A reference to the AdvancedAI object.
    /// </summary>
    [HideInInspector]
    public AdvancedAI main;

    /// <summary>
    /// Used to determine how far away the master is.
    /// </summary>
    /// <returns>The distance to the master. Returns -1 if no master is found.</returns>
    public float DistanceMaster()
    {
        if (master != null && main.path != null)
        {
            return Vector3.Distance(master.transform.position, main.path.Destination);
        }
        return -1f;
    }

    /// <summary>
    /// Checks if this GameObject and the subject is from the same flock.
    /// </summary>
    /// <param name="subject">The GameObject to test against.</param>
    /// <returns>Bool, telling whether or not they are from the same flock.</returns>
    public bool IsFlockMember(GameObject subject)
    {
        if (subject.GetComponent<Flocking>())
        {
            if (gameObject.transform == subject.transform || subject.GetComponent<Flocking>().FlockID == FlockID)
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

        foreach (Flocking ai in FindObjectsOfType<Flocking>())
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

    private void Awake()
    {
        FlockID = Mathf.RoundToInt(Random.Range(0, 99999999));
        main = GetComponent<AdvancedAI>();
    }

    private void Start()
    {
        if (master == null)
        {
            SpawnFlockMember();
        }
    }

    private void Update()
    {
        if (master == null && !IsMaster)
        {
            IsMaster = true;
        }
    }

    /// <summary>
    /// Spawn new member in this flock.
    /// </summary>
    private void SpawnFlockMember()
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
            bo.GetComponent<Flocking>().master = gameObject;
            bo.GetComponent<Flocking>().FlockID = FlockID;
            bo.name = bo.name.Replace("(Clone)", "");

            bo.transform.SetParent(transform.parent);
            o++;
        }
    }
}