using UnityEngine;
using Unity.Entities;
using Unity.Collections;
using System.Collections;
using System.Collections.Generic;

public class AgentLinkManager : MonoBehaviour
{
    public GameObject agentPrefab; // Assigned in Inspector
    public Transform agentsParent; // Parent of all agents in the scene

    [Header("Batch Settings")]
    public int agentsPerBatch = 800;
    public float batchInterval = 4f;

    private EntityManager entityManager;
    private List<Entity> unlinkedEntities = new List<Entity>(); // Entities that need an agent

    // 🚨 Static list to store agents in Edit Mode (not serialized in Inspector)
    [SerializeField] private List<GameObject> preloadedAgents = new List<GameObject>();

    void Awake()
    {
        entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
        //agentsParent.gameObject.SetActive(false);
    }

    void Start()
    {
        Debug.Log($"Loaded {preloadedAgents.Count} preloaded agents.");

        // 🔹 Query unlinked ECS entities
        EntityQuery agentQuery = entityManager.CreateEntityQuery(typeof(ManagedAgentLink));
        using NativeArray<Entity> entities = agentQuery.ToEntityArray(Allocator.TempJob);

        foreach (var entity in entities)
        {
            var managedAgentLink = entityManager.GetComponentObject<ManagedAgentLink>(entity);
            if (managedAgentLink.AgentReference == null)
            {
                unlinkedEntities.Add(entity);
            }
        }
        entities.Dispose();

        Debug.Log($"Unlinked Entities: {unlinkedEntities.Count}");

        // 🔹 Start linking in batches
        StartCoroutine(LinkAgentsInBatches());
    }

    private IEnumerator LinkAgentsInBatches()
    {
        int linkedCount = 0;
        int totalToLink = unlinkedEntities.Count;

        for (int batch = 0; batch < Mathf.CeilToInt((float)totalToLink / agentsPerBatch); batch++)
        {
            int linkCount = Mathf.Min(agentsPerBatch, totalToLink - linkedCount);
            Debug.Log($"Linking batch {batch + 1} - {linkCount} agents");

            for (int i = 0; i < linkCount; i++)
            {
                if (linkedCount >= totalToLink) break;

                Entity entity = unlinkedEntities[linkedCount];
                GameObject agent = GetAvailableAgent();
                agent.SetActive(true);

                if (agent == null)
                {
                    Debug.LogError("Failed to find or spawn an agent!");
                    continue;
                }

                PelletGrabberAgentECS agentScript = agent.GetComponent<PelletGrabberAgentECS>();
                var managedAgentLink = entityManager.GetComponentObject<ManagedAgentLink>(entity);
                managedAgentLink.AgentReference = agentScript;
                agentScript.SetLinkedEntity(entity);

                linkedCount++;
            }

            Debug.Log($"Linked {linkedCount} / {totalToLink} agents");

            if (linkedCount >= totalToLink)
                break;

            //yield return new WaitForSeconds(batchInterval);
            yield return null;
        }

        Debug.Log("All agents linked successfully!");

        // 🚨 Clear the preloaded agents list to free memory
        preloadedAgents.Clear();

        //agentsParent.gameObject.SetActive(true);
    }

    private GameObject GetAvailableAgent()
    {
        if (preloadedAgents.Count > 0)
        {
            GameObject agent = preloadedAgents[0];
            preloadedAgents.RemoveAt(0);
            return agent;
        }
        else
        {
            return Instantiate(agentPrefab, Vector3.zero, Quaternion.identity, agentsParent);
        }
    }

    // 🚨 Called by the Editor script to populate the list before runtime
    public void PreloadAgents()
    {
        if (agentsParent == null || agentPrefab == null)
        {
            Debug.LogError("Agents Parent or Agent Prefab is not set!");
            return;
        }

        // 🔹 Load existing agents
        preloadedAgents.Clear();
        foreach (var agent in agentsParent.GetComponentsInChildren<PelletGrabberAgentECS>())
        {
            preloadedAgents.Add(agent.gameObject);
        }

        int currentCount = preloadedAgents.Count;
        Debug.Log($"Existing agents: {currentCount}");

        // 🔹 Spawn missing agents if needed
        while (preloadedAgents.Count < 1000) // Default to 1000 agents
        {
            GameObject newAgent = Instantiate(agentPrefab, Vector3.zero, Quaternion.identity, agentsParent);
            preloadedAgents.Add(newAgent);
        }

        Debug.Log($"Preloaded {preloadedAgents.Count} agents.");
    }
}
