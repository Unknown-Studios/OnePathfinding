using UnityEngine;

/// <summary>
/// The smelling component is used by the AdvancedAI component to smell after agents.
/// </summary>
[RequireComponent(typeof(AdvancedAI))]
public class Smelling : MonoBehaviour
{
    /// <summary>
    /// The smelling distance of this agent.
    /// </summary>
    [HideInInspector]
    public float SmellDistance;

    /// <summary>
    /// A reference to the AdvancedAI component.
    /// </summary>
    private AdvancedAI main;

    /// <summary>
    /// Smells after nearby agents.
    /// </summary>
    /// <returns>Returns the GameObject of the found agent.</returns>
    public GameObject Smell()
    {
        AdvancedAI main = GetComponent<AdvancedAI>();
        Collider[] col = Physics.OverlapSphere(transform.position - (Wind.windVector3 * SmellDistance), SmellDistance);
        foreach (Collider collider in col)
        {
            if (collider.GetComponent<AdvancedAI>() || collider.tag == "Player")
            {
                if (main.IsFlockMember(collider.gameObject))
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

    private void OnDrawGizmos()
    {
        if (GridManager.ShowGizmo)
        {
            if (!main.Flying)
            {
                if (FindObjectOfType<GridManager>().ShowFlockColor && main.FlockAnimal)
                {
                    int se = Random.seed;
                    Random.seed = main.FlockID;
                    Gizmos.color = new Color(Random.Range(0f, 1f), Random.Range(0f, 1f), Random.Range(0f, 1f));
                    Random.seed = se;
                }
                if (Smell() != null)
                {
                    Gizmos.color = Color.red;
                }
                Gizmos.DrawWireSphere(transform.position - (Wind.windVector3 * SmellDistance), SmellDistance);
            }
        }
    }

    private void Start()
    {
        main = GetComponent<AdvancedAI>();
    }
}