using UnityEngine;
using Unity.Entities;
using Unity.Collections;
using System.Collections;
using System.Collections.Generic;

public class AgentSpawner : MonoBehaviour
{
    public GameObject agentPrefab; // Assign in Inspector

    [Header("Batch Settings")]
    public int agentsPerBatch = 800;  // Number of agents per batch
    public int totalBatches = 10;     // How many batches to spawn

    private EntityManager entityManager;
    private List<Entity> unlinkedEntities = new List<Entity>(); // Stores entities that need an agent

    void Start()
    {
        entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
        StartCoroutine(WaitForEntitiesAndSpawn());
    }

    private IEnumerator WaitForEntitiesAndSpawn()
    {
        EntityQuery agentQuery = entityManager.CreateEntityQuery(typeof(ManagedAgentLink));

        while (agentQuery.CalculateEntityCount() == 0) // Wait until entities exist
        {
            Debug.LogWarning("No ManagedAgentLink entities found, waiting...");
            yield return new WaitForSeconds(0.5f); // Wait and check again
        }

        NativeArray<Entity> entitiesArr = agentQuery.ToEntityArray(Allocator.Temp);
        foreach (var ent in entitiesArr)
        {
            var managedAgentLink = entityManager.GetComponentObject<ManagedAgentLink>(ent);
            if (managedAgentLink.AgentReference == null)
            {
                unlinkedEntities.Add(ent);
            }
        }

        // Debug.Log($"Total Unlinked Entities: {unlinkedEntities.Count}");

        entitiesArr.Dispose();
        
        StartCoroutine(SpawnAgentsInBatches());
    }

    private IEnumerator SpawnAgentsInBatches()
    {
        int spawnedCount = 0;
        int totalToSpawn = unlinkedEntities.Count;

        for (int batch = 0; batch < totalBatches; batch++)
        {
            int spawnCount = Mathf.Min(agentsPerBatch, totalToSpawn - spawnedCount);
            Debug.Log($"Spawning batch {batch + 1}/{totalBatches} - {spawnCount} agents");

            for (int i = 0; i < spawnCount; i++)
            {
                if (spawnedCount >= totalToSpawn) break;

                Entity entity = unlinkedEntities[spawnedCount];
                GameObject newAgent = Instantiate(agentPrefab, Vector3.zero, Quaternion.identity);
                PelletGrabberAgentECS agentScript = newAgent.GetComponent<PelletGrabberAgentECS>();

                if (agentScript == null)
                {
                    Debug.LogError("Agent prefab missing PelletGrabberAgentECS script!");
                    continue;
                }

                // Assign references in ECS
                var managedAgentLink = entityManager.GetComponentObject<ManagedAgentLink>(entity);
                managedAgentLink.AgentReference = agentScript;
                agentScript.SetLinkedEntity(entity);

                spawnedCount++;
            }

            Debug.Log($"Spawned {spawnedCount} / {totalToSpawn} agents");

            if (spawnedCount >= totalToSpawn)
                break;

            yield return null;
        }

        // **Final Cleanup Pass:** Spawn any remaining agents after the last batch
        if (spawnedCount < totalToSpawn)
        {
            Debug.Log($"Final cleanup pass - Spawning remaining {totalToSpawn - spawnedCount} agents");
            yield return null;

            while (spawnedCount < totalToSpawn)
            {
                Entity entity = unlinkedEntities[spawnedCount];
                GameObject newAgent = Instantiate(agentPrefab, Vector3.zero, Quaternion.identity);
                PelletGrabberAgentECS agentScript = newAgent.GetComponent<PelletGrabberAgentECS>();

                if (agentScript == null)
                {
                    Debug.LogError("Agent prefab missing PelletGrabberAgentECS script!");
                    continue;
                }

                // Assign references in ECS
                var managedAgentLink = entityManager.GetComponentObject<ManagedAgentLink>(entity);
                managedAgentLink.AgentReference = agentScript;
                agentScript.SetLinkedEntity(entity);

                spawnedCount++;
            }
        }

        Debug.Log("All agents spawned successfully!");
    }
}
