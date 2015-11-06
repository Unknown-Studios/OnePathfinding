using OnePathfinding;
using UnityEngine;

/// <summary>
/// This component is used to make the AI align to the currently active grid.
/// </summary>
[RequireComponent(typeof(AdvancedAI))]
public class GridAlign : MonoBehaviour
{
    private Rigidbody r;

    private void Start()
    {
        r = GetComponent<Rigidbody>();
        if (r)
        {
            r.WakeUp();
            r.useGravity = false;
            r.freezeRotation = true;
            r.isKinematic = false;
        }
    }

    //FixedUpdate
    private void FixedUpdate()
    {
        if (GetComponent<AdvancedAI>() && GetComponent<AdvancedAI>().grid.gridType == GridGraph.GridType.Sphere)
        {
            //Position:
            Vector3 gravityUp = transform.up;

            // Accelerate the body along its up vector
            if (GetComponent<Rigidbody>())
            {
                if (!GetComponent<AdvancedAI>().Flying)
                {
                    r.AddForce(gravityUp * -9.81f, ForceMode.Impulse);
                }
                else
                {
                    r.constraints = RigidbodyConstraints.FreezeAll;
                }
            }

            //Align:
            GridGraph grid = GetComponent<AdvancedAI>().grid;
            Vector3 checkPos = (transform.position - grid.offset).normalized * grid.Radius;
            RaycastHit hit;
            int Layer = gameObject.layer;

            gameObject.layer = 1 << 1;
            if (Physics.Linecast(checkPos, grid.offset, out hit))
            {
                if (transform != hit.transform)
                {
                    Vector3 fwd = transform.forward;
                    fwd = fwd - Vector3.Project(fwd, hit.normal);
                    if (fwd != Vector3.zero)
                        transform.rotation = Quaternion.LookRotation(fwd, hit.normal);
                }
            }
            gameObject.layer = Layer;
        }
    }
}