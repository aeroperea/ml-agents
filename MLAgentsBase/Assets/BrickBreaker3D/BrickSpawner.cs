using System;
using UnityEngine;

public class BrickSpawner : MonoBehaviour
{

    public GameObject brickPrefab;

    public int rowsSpawned = 10;
    public int columnsSpawned;
    public int depthSpawned;


    public Vector3 brickSpawnOrigin = new Vector3(0, 10, 0);
    public float bricksRowSpacing = 1.1f;
    public float brickColSpacing = 2.3f;
    public float bricksDepthSpacing = 1.1f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        bricksRowSpacing = transform.localScale.y;
        bricksRowSpacing = transform.localScale.x;
        SpawnBricks();
    }

    private void SpawnBricks()
    {
        for (int z = 0; z < depthSpawned; z++)
        {
            for (int x = 0; x < columnsSpawned; x++)
            {
                for(int y = 0; y < rowsSpawned; y++)
                {
                    Vector3 spawnPosition = brickSpawnOrigin + new Vector3(brickColSpacing * x, bricksRowSpacing * y, bricksDepthSpacing * z);
                    GameObject spawnedBrick = Instantiate(brickPrefab, spawnPosition, Quaternion.identity);
                }
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
