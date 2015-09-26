using UnityEngine;

public class Test : MonoBehaviour
{
    public GameObject AIPrefab;

    private void Start()
    {
        AISpawner.Spawn(AIPrefab, 10);
    }
}