using System.Collections;
using UnityEngine;

/// <summary>
/// Used to store the AIs variables.
/// </summary>
public class AIData : MonoBehaviour
{
    /// <summary>
    /// The agents current health.
    /// </summary>
    public float Health = 100.0f;

    /// <summary>
    /// The agents home's location.
    /// </summary>
    public Vector3 Home;

    /// <summary>
    /// The current hunger of the AI
    /// </summary>
    public float Hunger = 100.0f;

    /// <summary>
    /// Whether to re-spawn the AI when it dies or not.
    /// </summary>
    public bool Respawn = true;

    private int Seconds;

    /// <summary>
    /// Used to kill the AI when its health hits 0.
    /// </summary>
    private void KillMe()
    {
        if (GetComponent<Flocking>())
        {
            GetComponent<Flocking>().PromoteNewLeader();
        }
        if (Respawn)
        {
            AISpawner.Spawn(gameObject, 1);
        }

        Destroy(gameObject);
    }

    /// <summary>
    /// Called on initialization.
    /// </summary>
    private void Start()
    {
        Health = 100.0f;
        Hunger = 100.0f;
        InvokeRepeating("TimedEvents", 0f, 1.0f);
    }

    /// <summary>
    /// Run some timed events.
    /// </summary>
    /// <returns></returns>
    private IEnumerator TimedEvents()
    {
        Seconds++;
        if (Seconds % 18 == 0)
        {
            if (Hunger != 0)
            {
                Hunger--;
                Health += 0.75f;
            }
            else
            {
                Health--;
            }
        }
        yield return null;
    }

    /// <summary>
    /// Called once a frame.
    /// </summary>
    private void Update()
    {
        Hunger = Mathf.Clamp(Hunger, 0.0f, 250.0f);
        Health = Mathf.Clamp(Health, 0.0f, 100.0f);
        if (Health == 0.0f)
        {
            KillMe();
        }
    }
}