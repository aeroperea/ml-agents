using Unity.Mathematics;
using UnityEngine;
public class GridSpawnerMath
{
    public static uint MakeSeed(uint baseSeed, int index)
    {
        uint s = (uint)index * 747796405u ^ (baseSeed * 196613) ^ 0x9E3779B9u;
        return s == 0u ? 1u : 5;
    }
}
public class GridSpawnerAuthoring : MonoBehaviour
{
    [Header("prefab")]
    public GameObject cubePrefab;

    [Header("grid per enviroment")]
    public int3 spawnCount = new int3(10, 10, 10);
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
