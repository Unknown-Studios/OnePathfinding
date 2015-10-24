using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// An example on using the AISpawner.
/// </summary>
[RequireComponent(typeof(AISpawner))]
public class Test : MonoBehaviour
{
    /// <summary>
    /// A list of game-objects to spawn.
    /// </summary>
    public List<GameObject> gameobjects;

    /// <summary>
    /// Number of AIs to spawn.
    /// </summary>
    public int NumberOfAIs = 100;

    //Called on the start of this component.
    private void Start()
    {
        foreach (GameObject g in gameobjects)
        {
            AISpawner.Spawn(g, NumberOfAIs / gameobjects.Count);
        }
    }
}