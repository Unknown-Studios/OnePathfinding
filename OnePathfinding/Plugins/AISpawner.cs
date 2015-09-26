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

        GameObject ob = (GameObject)GameObject.Instantiate(obj, position, Quaternion.identity);

        AI ai = ob.GetComponent<AI>();
        ob.transform.parent = gridTransform;

        if (ai != null)
        {
            ob.name = ob.name.Replace("(Clone)", "");
            if (ai.FlockAnimal)
            {
                int flockSize = Random.Range(ai.minFlockSize, ai.maxFlockSize);
                ai.FlockSize = flockSize;

                int o = 0;
                while (o < flockSize)
                {
                    GameObject bo;
                    if (ai.FlockMember != null)
                    {
                        bo = (GameObject)Instantiate(ai.FlockMember, ob.transform.position, Quaternion.identity);
                    }
                    else
                    {
                        bo = (GameObject)Instantiate(obj, ob.transform.position, Quaternion.identity);
                    }
                    bo.transform.parent = gridTransform;
                    bo.GetComponent<AI>().master = ob;
                    bo.GetComponent<AI>().FlockID = ai.FlockID;
                    bo.name = bo.name.Replace("(Clone)", "");

                    o++;
                }
            }
        }
        else
        {
            Debug.LogError("Can't spawn AIs without AI component.");
            return;
        }
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
            GameObject ob = (GameObject)GameObject.Instantiate(obj, new Vector3(Random.Range(minX, maxX), 0, Random.Range(minY, maxY)), Quaternion.identity);

            i++;
            AI ai = ob.GetComponent<AI>();
            ob.transform.parent = gridTransform;

            if (ai != null)
            {
                ob.name = ob.name.Replace("(Clone)", "");
                if (ai.FlockAnimal)
                {
                    int flockSize = Random.Range(ai.minFlockSize, ai.maxFlockSize);
                    ai.FlockSize = flockSize;

                    int o = 0;
                    while (o < flockSize && i < Amount)
                    {
                        GameObject bo = (GameObject)GameObject.Instantiate(obj, ob.transform.position, Quaternion.identity);
                        bo.transform.parent = gridTransform;
                        bo.GetComponent<AI>().master = ob;
                        bo.GetComponent<AI>().FlockID = ai.FlockID;
                        bo.name = bo.name.Replace("(Clone)", "");

                        i++;
                        o++;
                    }
                }
            }
            else
            {
                Debug.LogError("Can't spawn AIs without AI component.");
                return;
            }
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