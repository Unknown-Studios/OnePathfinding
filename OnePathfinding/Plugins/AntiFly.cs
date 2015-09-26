﻿using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class AntiFly : MonoBehaviour
{
    private Terrain t;
    private TerrainData td;

    private void OnEnable()
    {
        t = Terrain.activeTerrain;
        td = t.terrainData;
    }

    private void Start()
    {
        if (GetComponent<Rigidbody>())
        {
            GetComponent<Rigidbody>().freezeRotation = true;
        }
    }

    private void Update()
    {
        if (transform.position.y <= 0f)
        {
            Vector3 pos = transform.position - t.transform.position;
            Vector2 Normal = new Vector2(pos.x / td.size.x, pos.z / td.size.z);

            Vector3 CurPos = transform.position;
            CurPos.y = td.GetInterpolatedHeight(Normal.x, Normal.y);
            CurPos.y += 2;
            transform.position = CurPos;
        }
    }
}