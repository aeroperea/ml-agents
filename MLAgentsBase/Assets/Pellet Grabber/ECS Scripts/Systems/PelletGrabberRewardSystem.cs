using System;
using System.Diagnostics;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

partial struct PelletGrabberRewardSystem : ISystem
{
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        PelletGrabberRewardJob pelletGrabberRewardJob = new PelletGrabberRewardJob
        {
            deltaTime = SystemAPI.Time.DeltaTime
        };
        pelletGrabberRewardJob.ScheduleParallel();
    }
}

[BurstCompile(FloatPrecision.Standard, FloatMode.Fast)]
public partial struct PelletGrabberRewardJob : IJobEntity
{
    public float deltaTime;
    //in for RO ref for RW
    public void Execute(in LocalTransform localTransform, ref PelletGrabber pelletGrabber, in PelletGrabberMovement pelletGrabberMovement, in AgentActiveTag activeTag)
    {
        pelletGrabber.reward = 0f;
        pelletGrabber.relativeAgentPos = new float2(localTransform.Position.x, localTransform.Position.z) / pelletGrabber.boundary;
        float2 toPellet = pelletGrabber.relativePelletPos - pelletGrabber.relativeAgentPos;
        float sqDist = math.lengthsq(toPellet);
        float invDist = math.rsqrt(sqDist);
        pelletGrabber.toPelletNormalized = toPellet * invDist;
        float relativeDistance = sqDist * invDist;
        pelletGrabber.timeLeftNormalized -= deltaTime * pelletGrabber.invMaxTime;

        float weightedMovementToDot = math.dot(pelletGrabber.toPelletNormalized, pelletGrabberMovement.moveInput) * 2.1f;
        pelletGrabber.reward += weightedMovementToDot * math.abs(weightedMovementToDot);
        //UnityEngine.Debug.Log($"reward {pelletGrabber.reward}   to pellet {pelletGrabber.toPelletNormalized} move input {pelletGrabberMovement.moveInput} \n " +
        //    $"                 relativeDistance {relativeDistance} toPellet {toPellet}");

        pelletGrabber.reward += (1 - relativeDistance) * 0.33f;
        //pelletGrabber.reward += math.log(1.44f + (1 - relativeDistance) * 9f) * 0.1f;
        pelletGrabber.reward -= 0.05f;

        pelletGrabber.isSuccess = relativeDistance < 0.033f;
        //UnityEngine.Debug.Log(relativeDistance + " " +pelletGrabber.isSuccess);

        pelletGrabber.reward -= math.select(0.44f, 0f, math.any(math.abs(pelletGrabber.relativeAgentPos) > pelletGrabber.boundary));

        pelletGrabber.isFailed = pelletGrabber.timeLeftNormalized < 0f;
        pelletGrabber.episodeReward += pelletGrabber.reward;
    }
}
