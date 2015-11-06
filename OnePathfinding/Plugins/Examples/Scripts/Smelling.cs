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
    public float SmellDistance = 15.0f;

    /// <summary>
    /// Smells after nearby agents.
    /// </summary>
    /// <returns>Returns the GameObject of the found agent.</returns>
    public GameObject Smell()
    {
        Collider[] col = Physics.OverlapSphere(transform.position - (Wind.windVector3 * SmellDistance), SmellDistance);
        foreach (Collider collider in col)
        {
            if (collider.GetComponent<AdvancedAI>() || collider.tag == "Player")
            {
                if (GetComponent<Flocking>() && GetComponent<Flocking>().IsFlockMember(collider.gameObject))
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
}