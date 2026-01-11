using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using Zelcam4.MLAgents;

// anchor: reward + termination (replaces addreward + endepisode)
[BurstCompile]
[UpdateInGroup(typeof(RewardGroup))]
public partial struct PelletGrabberRewardSystem : ISystem
{
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var pelletLookup = SystemAPI.GetComponentLookup<LocalTransform>(true);

        var ecb = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>()
            .CreateCommandBuffer(state.WorldUnmanaged)
            .AsParallelWriter();

        var job = new PelletGrabberRewardJob
        {
            deltaTime = SystemAPI.Time.DeltaTime,
            pelletTransformLookup = pelletLookup,
            ecb = ecb
        };

        state.Dependency = job.ScheduleParallel(state.Dependency);
    }
}

[BurstCompile(FloatPrecision.Standard, FloatMode.Fast)]
[WithAll(typeof(RequestActionTag))]
[WithNone(typeof(EndEpisodeTag))]
public partial struct PelletGrabberRewardJob : IJobEntity
{
    public float deltaTime;

    [ReadOnly] public ComponentLookup<LocalTransform> pelletTransformLookup;
    public EntityCommandBuffer.ParallelWriter ecb;

    private void Execute(
        [ChunkIndexInQuery] int chunkIndex,
        Entity entity,
        ref AgentEcs agent,
        ref PelletGrabber g,
        in PelletGrabberMovement m,
        in LocalTransform self,
        in LinkedPellet linkedPellet)
    {
        if (agent.StartingEpisode)
            return;

        float2 pelletPosNorm = g.relativePelletPos;

        if (linkedPellet.PelletEntity != Entity.Null && pelletTransformLookup.HasComponent(linkedPellet.PelletEntity))
        {
            var pelletTr = pelletTransformLookup[linkedPellet.PelletEntity];
            pelletPosNorm = new float2(pelletTr.Position.x, pelletTr.Position.z) / g.boundary;
        }

        float2 agentPosNorm = new float2(self.Position.x, self.Position.z) / g.boundary;

        float2 toPellet = pelletPosNorm - agentPosNorm;
        float sqDist = math.lengthsq(toPellet);
        float invDist = math.rsqrt(math.max(sqDist, 1e-8f));
        float2 dir = toPellet * invDist;
        float dist = sqDist * invDist;

        g.reward = 0f;

        float dot = math.dot(dir, m.moveInput) * 2.1f;
        g.reward += dot * math.abs(dot);

        g.reward += (1f - dist) * 0.33f;
        g.reward -= 0.05f;

        bool oob = math.any(math.abs(agentPosNorm) > 1f);
        if (oob) g.reward -= 0.44f;

        g.timeLeftNormalized -= deltaTime * g.invMaxTime;

        g.isSuccess = dist < 0.033f;
        g.isFailed = g.timeLeftNormalized < 0f;

        g.episodeReward += g.reward;
        agent.Reward += g.reward;
        agent.CumulativeReward += g.reward;

        if (g.isSuccess || g.isFailed)
        {
            float t = g.timeLeftNormalized * 1.5f;
            float terminal = g.isSuccess ? (4.5f * (1f + (t * t))) : -6f;

            g.reward += terminal;
            g.episodeReward += terminal;

            agent.Reward += terminal;
            agent.CumulativeReward += terminal;

            ecb.AddComponent<EndEpisodeTag>(chunkIndex, entity);
        }
    }
}
