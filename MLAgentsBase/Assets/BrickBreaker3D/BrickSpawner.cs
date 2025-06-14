using System;
using UnityEngine;

public class BrickSpawner : MonoBehaviour
{

    public GameObject brickPrefab;

    public int rowsSpawned = 10;
    public int columnsSpawned;
    public int depthSpawned;


    public Vector3 brickSpawnOrigin = new Vector3(0, 10, 0);
    private float bricksRowSpacing;
    private float brickColSpacing;
    private float bricksDepthSpacing;
    public Color gmizmoColor = Color.blue;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        bricksRowSpacing = brickPrefab.transform.localScale.y + 0.1f;
        brickColSpacing = brickPrefab.transform.localScale.x + 0.1f;
        bricksDepthSpacing = brickPrefab.transform.localScale.z + 0.1f;

        bricksRowSpacing *= rowsSpawned < 0 ? -1 : 1;
        brickColSpacing *= columnsSpawned < 0 ? -1 : 1;
        bricksDepthSpacing *= depthSpawned < 0 ? -1 : 1;

        rowsSpawned = Math.Abs(rowsSpawned);
        columnsSpawned = Math.Abs(columnsSpawned);
        depthSpawned = Math.Abs(depthSpawned);

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

    private void OnDrawGizmos()
    {
        brickColSpacing = brickPrefab.transform.localScale.x + 0.1f;
        bricksRowSpacing = brickPrefab.transform.localScale.y + 0.1f;
        bricksDepthSpacing = brickPrefab.transform.localScale.z + 0.1f;

        gmizmoColor.a = 0.5f;
        Gizmos.color = gmizmoColor;
        float x = brickColSpacing * columnsSpawned;
        float y = bricksRowSpacing * rowsSpawned;
        float z = bricksDepthSpacing * depthSpawned;
        Vector3 boxSize = new Vector3(x, y, z);
        Gizmos.DrawCube(transform.position, boxSize);
    }
}
