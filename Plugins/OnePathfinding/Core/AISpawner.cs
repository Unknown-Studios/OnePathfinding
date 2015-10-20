using UnityEngine;

public class AISpawner : MonoBehaviour
{
    public static AISpawner instance;

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
        ob.name = ob.name.Replace("(Clone)", "");
    }

    public static void Spawn(GameObject obj, int Amount)
    {
        Terrain t = Terrain.activeTerrain;
        if (t == null || t.terrainData == null)
        {
            Debug.Log("Cannot spawn objects, when no terrain is existent.");
            return;
        }

        float minX = t.transform.position.x;
        float minY = t.transform.position.z;

        float maxX = minX + t.terrainData.size.x;
        float maxY = minY + t.terrainData.size.z;

        Transform gridTransform = FindObjectOfType<GridManager>().transform;

        int i = 0;
        while (i < Amount)
        {
            GameObject ob = (GameObject)Instantiate(obj, new Vector3(Random.Range(minX, maxX), 0, Random.Range(minY, maxY)), Quaternion.identity);
            ob.transform.parent = gridTransform;

            i++;
        }
    }

    private void OnDisable()
    {
        instance = null;
    }

    private void OnEnable()
    {
        instance = this;
    }
}