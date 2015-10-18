using System.Collections.Generic;
using UnityEngine;

public class Test : MonoBehaviour
{
    public List<GameObject> gameobjects;
    public int NumberOfAIs = 100;

    private void Start()
    {
        foreach (GameObject g in gameobjects)
        {
            AISpawner.Spawn(g, NumberOfAIs / gameobjects.Count);
        }
    }
}