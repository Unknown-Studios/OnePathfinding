using System.Collections.Generic;
using UnityEngine;

public class Test : MonoBehaviour
{
    public List<GameObject> gameobjects;
    public int NumberOfAIs = 100;
    public float time = 0.0f;

    private void OnGUI()
    {
        GUILayout.Label(time.ToString());
    }

    private void Start()
    {
        foreach (GameObject g in gameobjects)
        {
            AISpawner.Spawn(g, NumberOfAIs / gameobjects.Count);
        }
    }

    private void Update()
    {
        time += Time.deltaTime;
    }
}