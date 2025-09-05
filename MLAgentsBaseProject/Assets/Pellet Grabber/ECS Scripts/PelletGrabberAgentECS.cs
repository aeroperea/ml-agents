using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Collections;
using Random = UnityEngine.Random;
using Unity.Burst;
using Unity.Jobs;
using Unity.Transforms;

public class PelletGrabberAgentECS : Agent
{
    private EntityManager entityManager;
    private Entity linkedEntity;
    private Entity linkedPelletEntity;
    bool agentLinked = false;

    public void SetLinkedEntity(Entity entity)
    {
        if (entity == Entity.Null)
        {
            Debug.LogError("Tried to link to a NULL entity!");
            return;
        }

        Debug.Log($"Linked {gameObject.name} to Entity {entity.Index}");

        entityManager = World.DefaultGameObjectInjectionWorld.EntityManager; // Assign EntityManager
        linkedEntity = entity;
        linkedPelletEntity = entityManager.GetComponentData<LinkedPellet>(linkedEntity).PelletEntity;
        agentLinked = true;
    }


    public override void OnEpisodeBegin()
    {
        if (!agentLinked) return;
        if (!entityManager.Exists(linkedEntity) || !entityManager.Exists(linkedPelletEntity)) return;

        var rewardData = entityManager.GetComponentData<PelletGrabber>(linkedEntity);
        var movementData = entityManager.GetComponentData<PelletGrabberMovement>(linkedEntity);

        var localAgentTransform = entityManager.GetComponentData<LocalTransform>(linkedEntity);
        var localPelletTransform = entityManager.GetComponentData<LocalTransform>(linkedPelletEntity);

        uint seed = (uint)UnityEngine.Random.Range(1, int.MaxValue);

        // Create NativeArrays for transforms & reward data
        NativeArray<LocalTransform> transforms = new NativeArray<LocalTransform>(2, Allocator.TempJob);
        transforms[0] = localAgentTransform;
        transforms[1] = localPelletTransform;

        NativeArray<PelletGrabber> rewardDataArray = new NativeArray<PelletGrabber>(1, Allocator.TempJob);
        rewardDataArray[0] = rewardData;

        var job = new ResetAgentJob
        {
            RewardData = rewardDataArray,
            Transforms = transforms,
            RandomSeed = seed
        };

        job.Schedule().Complete();

        // Retrieve updated values
        entityManager.SetComponentData(linkedEntity, rewardDataArray[0]); // Apply updated RewardData
        //print(rewardDataArray[0].relativePelletPos); // Debug print

        entityManager.SetComponentData(linkedEntity, transforms[0]); // Update agent transform
        entityManager.SetComponentData(linkedPelletEntity, transforms[1]); // Update pellet transform

        // Dispose NativeArrays
        transforms.Dispose();
        rewardDataArray.Dispose();

        entityManager.SetComponentEnabled<AgentActiveTag>(linkedEntity, true);
    }



    public override void Heuristic(in ActionBuffers actionsOut)
    {
        if (!agentLinked) return;
        var continuousActions = actionsOut.ContinuousActions;

        // Map movement input to continuous actions (e.g., WASD or arrow keys)
        continuousActions[0] = Input.GetAxis("Horizontal"); // Left (-1) / Right (+1)
        continuousActions[1] = Input.GetAxis("Vertical");   // Down (-1) / Up (+1)
    }


    public override void CollectObservations(VectorSensor sensor)
    {
        if (!agentLinked) return;
        if (!entityManager.Exists(linkedEntity)) return;

        var rewardData = entityManager.GetComponentData<PelletGrabber>(linkedEntity);
        var movementData = entityManager.GetComponentData<PelletGrabberMovement>(linkedEntity);
        //Debug.Log("hello");
        sensor.AddObservation(rewardData.relativeAgentPos);
        sensor.AddObservation(rewardData.timeLeftNormalized);
        sensor.AddObservation(rewardData.toPelletNormalized);
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        if (!agentLinked) return;
        if (!entityManager.Exists(linkedEntity)) return;

        var movementData = entityManager.GetComponentData<PelletGrabberMovement>(linkedEntity);
        movementData.moveInput = new float2(actions.ContinuousActions[0], actions.ContinuousActions[1]);
        entityManager.SetComponentData(linkedEntity, movementData);

        var rewardData = entityManager.GetComponentData<PelletGrabber>(linkedEntity);
        AddReward(rewardData.reward);
        //Debug.Log(rewardData.reward + " time remaining " + rewardData.timeLeftNormalized + " moveinput " + movementData.moveInput + " isSuccess " + rewardData.isSuccess + " isFailed " + rewardData.isFailed);

        // Avoid unnecessary SetReward calls
        if (!rewardData.isSuccess && !rewardData.isFailed)
        {
            return;
        }
        AddReward(rewardData.isSuccess ? 4.5f * (1 + math.square(rewardData.timeLeftNormalized * 1.5f)) : -6f);
        entityManager.SetComponentEnabled<AgentActiveTag>(linkedEntity, false);
        //Debug.Log(this.gameObject.name + " finished an episode with " + rewardData.reward);
        //print($"ended ep {rewardData.isFailed} failed time left: {rewardData.timeLeftNormalized}   {rewardData.isSuccess} success relative pos{rewardData.relativeAgentPos} pelletPos {rewardData.relativePelletPos}");
        EndEpisode();
    }
}
