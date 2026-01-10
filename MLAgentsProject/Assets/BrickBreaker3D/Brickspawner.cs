using System;
using UnityEngine;

public class Brickspawner : MonoBehaviour
{

    public GameObject brickPrefab;

    public int rowsSpawned = 10;
    public int columnsSpawned;
    public int depthSpawned;
    private float bricksRowSpacing = 1.1f;
    private float brickColSpacing = 2.3f;
    private float brickDepthSpacing = 1.1f;
    [SerializeField] private Color gizmoColor = Color.blue;




    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        OnValidate();
      

        bricksRowSpacing *= rowsSpawned < 0 ? -1 : 1;
        brickColSpacing *= columnsSpawned < 0 ? -1 : 1;
        brickDepthSpacing *= depthSpawned < 0 ? -1 : 1;

        rowsSpawned = Math.Abs(rowsSpawned);
        columnsSpawned = Math.Abs(columnsSpawned);
        depthSpawned = Math.Abs(depthSpawned);

        SpawnedBricks(); 
    }

    private void OnValidate()
    {
        bricksRowSpacing = brickPrefab.transform.localScale.y + 0.1f;
        brickColSpacing = brickPrefab.transform.localScale.x + 0.1f;
        brickDepthSpacing = brickPrefab.transform.localScale.z + 0.1f;
    }
    private void SpawnedBricks()
    {
        for (int z = 0; z < depthSpawned; z++)
        {
            for (int x = 0; x < columnsSpawned; x++)
            {
                for(int y = 0; y < rowsSpawned; y++)
                {
                    Vector3 spawnPosition = transform.position + new Vector3(brickColSpacing * x,bricksRowSpacing * y,brickDepthSpacing * z);
                    GameObject spawnedbrick = Instantiate(brickPrefab, spawnPosition, Quaternion.identity);
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
        bricksRowSpacing = brickPrefab.transform.localScale.y + 0.1f;
        brickColSpacing = brickPrefab.transform.localScale.x + 0.1f;
        brickDepthSpacing = brickPrefab.transform.localScale.z + 0.1f;

        gizmoColor.a = 0.5f;
        Gizmos.color = gizmoColor;
        float x = brickColSpacing * columnsSpawned;
        float y = bricksRowSpacing * rowsSpawned;
        float z = brickDepthSpacing * depthSpawned;
        Vector3 boxSize = new Vector3(x,y,z);
        Gizmos.DrawCube(transform.position + boxSize * 0.5f, boxSize);
    }
}
