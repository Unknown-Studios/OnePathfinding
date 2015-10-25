using UnityEngine;
using System.Collections;

/// <summary>
/// Used to spawn AIs with ease.
/// </summary>
public class AISpawner : MonoBehaviour
{
    /// <summary>
    /// An instance to this object.
    /// </summary>
    private static AISpawner _instance;

    /// <summary>
    /// Maximum number of AIs.
    /// </summary>
    public int MaxAIs = 100;

    /// <summary>
    /// An instance to this object.
    /// </summary>
    public static AISpawner instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<AISpawner>();
            }
            return _instance;
        }
    }

    /// <summary>
    /// Spawns a object at a position.
    /// </summary>
    /// <param name="obj">The object to spawn</param>
    /// <param name="position">The position on which to spawn the obj.</param>
    public static void Spawn(GameObject obj, Vector3 position)
    {
        Terrain t = Terrain.activeTerrain;
        if (t == null || t.terrainData == null)
        {
            Debug.Log("Cannot spawn objects, when no terrain is existent.");
            return;
        }

        Transform gridTransform = FindObjectOfType<GridManager>().transform;

        GameObject ob = (GameObject)Instantiate(obj, position, Quaternion.identity);

        ob.transform.parent = gridTransform;
        ob.name = obj.name;
    }

    /// <summary>
    /// Spawn the specified amount of obj AIs
    /// </summary>
    /// <param name="obj">The object to spawn</param>
    /// <param name="Amount">The amount to spawn</param>
    public static void Spawn(GameObject obj, int Amount)
    {
        instance.StartCoroutine(SpawnObjects(obj, Amount));
    }

    private static IEnumerator SpawnObjects(GameObject obj, int Amount)
    {
        Terrain t = Terrain.activeTerrain;
        if (t == null || t.terrainData == null)
        {
            Debug.Log("Cannot spawn objects, when no terrain is existent.");
            yield break;
        }

        float minX = t.transform.position.x;
        float minY = t.transform.position.z;

        float maxX = minX + t.terrainData.size.x;
        float maxY = minY + t.terrainData.size.z;

        Transform gridTransform = FindObjectOfType<GridManager>().transform;

        int i = 0;
        while (i < Amount && instance.gameObject.transform.childCount < instance.MaxAIs)
        {
            GameObject ob = (GameObject)Instantiate(obj, new Vector3(Random.Range(minX, maxX), 0, Random.Range(minY, maxY)), Quaternion.identity);
            ob.transform.parent = gridTransform;
            ob.name = obj.name;

            i++;
            yield return null;
        }
    }
}