using UnityEngine;

/// <summary>
/// Used to prevent the AI to go off the terrain.
/// </summary>
[RequireComponent(typeof(Rigidbody))]
public class AntiFly : MonoBehaviour
{
    /// <summary>
    /// Instance to the terrain.
    /// </summary>
    private Terrain t;

    private TerrainData td;

    /// <summary>
    /// Called on initialization.
    /// </summary>
    private void OnEnable()
    {
        t = Terrain.activeTerrain;
        td = t.terrainData;
    }

    /// <summary>
    /// Called on initialization.
    /// </summary>
    private void Start()
    {
            GetComponent<Rigidbody>().useGravity = true;
            GetComponent<Rigidbody>().freezeRotation = true;
        
    }

    /// <summary>
    /// Called once a frame.
    /// </summary>
    private void Update()
    {
        if (transform.position.y <= 0f)
        {
            Vector3 pos = transform.position - t.transform.position;
            Vector2 Normal = new Vector2(pos.x / td.size.x, pos.z / td.size.z);

            Vector3 CurPos = transform.position;
            CurPos.x = Mathf.Clamp(CurPos.x, t.transform.position.x + 0.1f, t.transform.position.x + t.terrainData.size.x - 0.1f);
            CurPos.z = Mathf.Clamp(CurPos.z, t.transform.position.z + 0.1f, t.transform.position.z + t.terrainData.size.z - 0.1f);
            if (Normal.x > 0 && Normal.x < 1 && Normal.y > 0 && Normal.y < 1)
            {
                CurPos.y = td.GetInterpolatedHeight(Normal.x, Normal.y);
                CurPos.y += 5;
            }
            transform.position = CurPos;
        }
    }
}