using System.Collections;
using UnityEngine;

public class AIData : MonoBehaviour
{
    #region Fields

    public float Health = 100.0f;

    public Vector3 Home;

    public float Hunger = 100.0f;
    public bool Respawn = true;

    private int Seconds;

    #endregion Fields

    #region Methods

    private void KillMe()
    {
        GetComponent<AI>().PromoteNewLeader();
        if (Respawn)
        {
            AISpawner.Spawn(gameObject, 1);
        }

        Destroy(gameObject);
    }

    private void Start()
    {
        Health = 100.0f;
        Hunger = 100.0f;
        InvokeRepeating("TimedEvents", 0f, 1.0f);
    }

    private IEnumerator TimedEvents()
    {
        Seconds++;
        if (Seconds % 18 == 0)
        {
            if (Hunger != 0)
            {
                Health += 0.75f;
            }
            if (Hunger == 0.0f)
            {
                Health--;
            }
            else
            {
                Hunger--;
            }
        }
        yield return null;
    }

    private void Update()
    {
        Hunger = Mathf.Clamp(Hunger, 0.0f, 100.0f);
        Health = Mathf.Clamp(Health, 0.0f, 100.0f);
        if (Health == 0.0f)
        {
            KillMe();
        }
    }

    #endregion Methods
}