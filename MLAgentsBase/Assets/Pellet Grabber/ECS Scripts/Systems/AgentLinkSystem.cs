//using Unity.Burst;
//using Unity.Collections;
//using Unity.Entities;
//using Unity.Transforms;
//using UnityEngine;

//public partial struct AgentLinkSystem : ISystem
//{
//    public void OnCreate(ref SystemState state)
//    {
//        EntityManager entityManager = state.EntityManager;

//        //// Fetch the prefab entity from ECS singleton
//        //if (!SystemAPI.HasSingleton<AgentSpawnerSingleton>()) return;

//        //var spawnerSingleton = SystemAPI.GetSingleton<AgentSpawnerSingleton>();

//        //if (spawnerSingleton.AgentPrefab == Entity.Null)
//        //{
//        //    Debug.LogError("Agent Prefab Entity is NULL!");
//        //    return;
//        //}

//        // Query all unlinked agents
//        var query = SystemAPI.QueryBuilder().WithAll<ManagedAgentLink>().Build();
//        using var entities = query.ToEntityArray(Allocator.Temp);

//        foreach (var entity in entities)
//        {
//            var managedAgentLink = entityManager.GetComponentObject<ManagedAgentLink>(entity);

//            if (managedAgentLink.AgentReference == null)
//            {
//                // Spawn an ECS entity instead of a GameObject
//                //Entity newAgentEntity = entityManager.Instantiate(spawnerSingleton.AgentPrefab);

//                // If you still need a GameObject (ML-Agents), you can convert it here:
//                UnityEngine.Debug.Log("help");
//                GameObject newAgent = GameObject.Instantiate(Resources.Load<GameObject>("PelletGrabberAgentECS"));
//                PelletGrabberAgentECS agentScript = newAgent.GetComponent<PelletGrabberAgentECS>();

//                managedAgentLink.AgentReference = agentScript;
//                agentScript.SetLinkedEntity(entity);
//            }
//        }
//    }
//}
