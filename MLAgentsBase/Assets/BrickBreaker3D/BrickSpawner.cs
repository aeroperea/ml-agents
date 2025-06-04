using System;
using UnityEngine;

public class BrickSpawner : MonoBehaviour
{

    public GameObject brickPrefab;

    public int rowsSpawned = 10;
    public int columnsSpawned;
    public int depthSpawned;

    public float bricksRowSpacing = 1.1f;
    public float brickColSpacing = 2.3f;
    public float bricksDepthSpacing = 1.1f;
    [SerializeField] private Color gizmoColor = Color.blue  ;

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
                    Vector3 spawnPosition = transform.position + new Vector3(brickColSpacing * x, bricksRowSpacing * y, bricksDepthSpacing * z);
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
        Gizmos.color = gizmoColor;
        float x = brickColSpacing * columnsSpawned;
        float y = bricksRowSpacing * rowsSpawned;
        float z = bricksDepthSpacing * depthSpawned;
        Vector3 boxSize = new Vector3(x,y,z);
        Gizmos.DrawWireCube(transform.position + boxSize * 0.5f, boxSize);
    }
}
