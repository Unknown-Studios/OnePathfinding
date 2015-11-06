using System.Collections;
using UnityEngine;

/// <summary>
/// This script is used to have an impact on the smelling.
/// </summary>
public class Wind : MonoBehaviour
{
    /// <summary>
    /// The speed at which the wind moves.
    /// </summary>
    public static float speed = 1.0f;

    /// <summary>
    /// The rate at which the wind direction updates. (In seconds)
    /// </summary>
    public int RepeatRate = 300;

    /// <summary>
    /// A instance to this component.
    /// </summary>
    private static Wind _instance;

    /// <summary>
    /// The current wind direction.
    /// </summary>
    private Vector2 _wind;

    /// <summary>
    /// Instance of this component.
    /// </summary>
    public static Wind instance
    {
        get
        {
            return _instance;
        }
    }

    /// <summary>
    /// Static reference to the wind.
    /// </summary>
    public static Vector2 wind
    {
        get
        {
            if (instance == null)
            {
                return Vector2.zero;
            }
            return instance._wind;
        }
        set
        {
            if (instance != null)
            {
                instance._wind = value;
            }
        }
    }

    /// <summary>
    /// Wind reference as a vector3.
    /// </summary>
    public static Vector3 windVector3
    {
        get
        {
            return new Vector3(wind.x, 0, wind.y);
        }
    }

    /// <summary>
    /// Called on initialization of this component.
    /// </summary>
    public void Awake()
    {
        if (FindObjectsOfType<Wind>().Length > 1)
        {
            Debug.LogError("There can never be 2 Wind components in the same scene.");
            Destroy(this);
        }
        _instance = FindObjectOfType<Wind>();
    }

    /// <summary>
    /// Called on initialization of this component.
    /// </summary>
    public void Start()
    {
        InvokeRepeating("UW", 0.0f, RepeatRate);
    }

    /// <summary>
    /// Updates the winds direction.
    /// </summary>
    /// <returns></returns>
    private IEnumerator UpdateWind()
    {
        Vector2 NewWind = new Vector2((Random.value * 2.0f) - 1.0f, (Random.value * 2.0f) - 1.0f);
        while (wind != NewWind)
        {
            speed = Mathf.Sin(Time.time);
            wind = Vector2.MoveTowards(wind, NewWind, Time.deltaTime / 2.0f);
            wind *= speed;
            yield return null;
        }
    }

    /// <summary>
    /// Used to call the UpdateWind coroutine.
    /// </summary>
    private void UW()
    {
        StartCoroutine(UpdateWind());
    }
}