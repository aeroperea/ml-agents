using Unity.Burst;
using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;

partial struct PelletGrabberMovementSystem : ISystem
{

    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<PelletGrabberMovement>();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        float deltaTime = SystemAPI.Time.DeltaTime;
        PelletGrabberMovementJob pelletGrabberMovementJob = new PelletGrabberMovementJob {
                                                           deltaTime = deltaTime,
                                                        };
        pelletGrabberMovementJob.ScheduleParallel();
    }
}

[BurstCompile]
public partial struct PelletGrabberMovementJob : IJobEntity
{

    public float deltaTime;
    //in for RO ref for RW
    public void Execute(ref LocalTransform localTransform, in PelletGrabberMovement pelletGrabberMovement, in AgentActiveTag isActive)
    {
        localTransform.Position += new float3(pelletGrabberMovement.moveInput.x, 0, pelletGrabberMovement.moveInput.y) * pelletGrabberMovement.moveSpeed * deltaTime;
    }
}
