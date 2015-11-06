using OnePathfinding;
using System.Collections;
using UnityEngine;

/// <summary>
/// Used to spawn AIs with ease.
/// </summary>
public class AISpawner : MonoBehaviour
{
    /// <summary>
    /// Maximum number of AIs.
    /// </summary>
    public int MaxAIs = 100;

    /// <summary>
    /// An instance to this object.
    /// </summary>
    private static AISpawner _instance;

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
        GameObject ob = (GameObject)Instantiate(obj, position, Quaternion.identity);

        ob.transform.parent = FindObjectOfType<GridManager>().transform;
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

    private static IEnumerator SpawnObjects(GameObject obj, int Amount, GridGraph grid = null)
    {
        if (grid == null)
        {
            grid = GridManager.Grid;
        }
        if (grid.gridType == GridGraph.GridType.Plane)
        {
            float minX = grid.offset.x;
            float minY = grid.offset.z;

            float maxX = minX + grid.WorldSize.x;
            float maxY = minY + grid.WorldSize.y;

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
        else
        {
            Transform gridTransform = FindObjectOfType<GridManager>().transform;

            int i = 0;
            while (i < Amount && instance.gameObject.transform.childCount < instance.MaxAIs)
            {
                GameObject ob = (GameObject)Instantiate(obj, Random.onUnitSphere * (grid.Radius + 2), Quaternion.identity);
                ob.transform.parent = gridTransform;
                ob.name = obj.name;

                i++;
                yield return null;
            }
        }
    }
}