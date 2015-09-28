using System.Collections;
using UnityEngine;

public class Wind : MonoBehaviour
{
    #region Fields

    public static float speed;

    public Vector2 _wind;

    /// <summary>
    /// The rate at which the wind direction updates. (In seconds)
    /// </summary>
    public int RepeatRate;

    private static Wind _instance;

    #endregion Fields

    #region Properties

    public static Wind instance
    {
        get
        {
            return _instance;
        }
    }

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

    #endregion Properties

    #region Methods

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

    private void UW()
    {
        StartCoroutine(UpdateWind());
    }

    #endregion Methods
}