using System.Collections;
using UnityEngine;

public class Wind : MonoBehaviour
{
    public static float speed;

    /// <summary>
    /// The rate at which the wind direction updates. (In seconds)
    /// </summary>
    public int RepeatRate;

    public Vector2 _wind;

    private static Wind _instance;

    public static Vector2 wind
    {
        get
        {
            return instance._wind;
        }
        set
        {
            instance._wind = value;
        }
    }

    public static Vector3 windVector3
    {
        get
        {
            return new Vector3(wind.x, 0, wind.y);
        }
    }

    public static Wind instance
    {
        get
        {
            return _instance;
        }
    }

    public void Awake()
    {
        if (FindObjectsOfType<Wind>().Length > 1)
        {
            Destroy(this);
        }
        _instance = FindObjectOfType<Wind>();
    }

    public void Start()
    {
        InvokeRepeating("UW", 0.0f, RepeatRate);
    }

    private void UW()
    {
        StartCoroutine(UpdateWind());
    }

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
}