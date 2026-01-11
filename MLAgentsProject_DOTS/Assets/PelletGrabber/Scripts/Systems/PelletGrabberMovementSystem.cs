using Unity.Burst;
using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;
using Zelcam4.MLAgents;

// anchor: apply moveinput only on action steps
[BurstCompile]
[UpdateInGroup(typeof(ActionSystemGroup))]
[UpdateAfter(typeof(PelletGrabberActionInputSystem))]
public partial struct PelletGrabberMovementSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<PelletGrabberMovement>();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var job = new PelletGrabberMovementJob
        {
            deltaTime = SystemAPI.Time.DeltaTime
        };

        state.Dependency = job.ScheduleParallel(state.Dependency);
    }
}

[BurstCompile]
[WithAll(typeof(RequestActionTag))]
[WithNone(typeof(EndEpisodeTag))]
public partial struct PelletGrabberMovementJob : IJobEntity
{
    public float deltaTime;

    private void Execute(ref LocalTransform tr, in PelletGrabberMovement move, in AgentEcs agent)
    {
        if (agent.StartingEpisode)
            return;

        tr.Position += new float3(move.moveInput.x, 0f, move.moveInput.y) * move.moveSpeed * deltaTime;
    }
}
