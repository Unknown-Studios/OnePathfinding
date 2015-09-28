using System.Collections.Generic;
using UnityEngine;

public class Test : MonoBehaviour
{
    #region Fields

    public List<GameObject> gameobjects;
    public int NumberOfAIs = 100;

    #endregion Fields

    #region Methods

    private void Start()
    {
        foreach (GameObject g in gameobjects)
        {
            AISpawner.Spawn(g, NumberOfAIs / gameobjects.Count);
        }
    }

    #endregion Methods
}