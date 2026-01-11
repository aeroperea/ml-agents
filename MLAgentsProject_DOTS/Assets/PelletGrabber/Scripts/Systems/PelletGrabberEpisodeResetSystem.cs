using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using Zelcam4.MLAgents;

// anchor: environment reset (replaces onepisodebegin + initial random spawn)
[BurstCompile]
[UpdateBefore(typeof(RequesterSystem))]
public partial struct PelletGrabberEpisodeResetSystem : ISystem
{
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var pelletLookup = SystemAPI.GetComponentLookup<LocalTransform>(false);

        var job = new ResetJob
        {
            pelletTransformLookup = pelletLookup
        };

        state.Dependency = job.ScheduleParallel(state.Dependency);
    }
}

[BurstCompile]
public partial struct ResetJob : IJobEntity
{
    [NativeDisableParallelForRestriction]
    public ComponentLookup<LocalTransform> pelletTransformLookup;

    private void Execute(
        ref AgentEcs agent,
        ref PelletGrabber g,
        ref PelletGrabberMovement move,
        ref LocalTransform self,
        in LinkedPellet linkedPellet)
    {
        if (!agent.StartingEpisode && !g.needsReset)
            return;

        uint seed = (uint)(agent.EpisodeId * 1000003) ^ (uint)(agent.CompletedEpisodes * 7919) ^ 0x9e3779b9u;
        if (seed == 0u) seed = 1u;
        var rng = Unity.Mathematics.Random.CreateFromIndex(seed);

        float ax = rng.NextFloat(-g.boundary.x, g.boundary.x);
        float az = rng.NextFloat(-g.boundary.y, g.boundary.y);

        float px = rng.NextFloat(-g.boundary.x, g.boundary.x);
        float pz = rng.NextFloat(-g.boundary.y, g.boundary.y);

        self.Position = new float3(ax, self.Position.y, az);

        if (linkedPellet.PelletEntity != Entity.Null && pelletTransformLookup.HasComponent(linkedPellet.PelletEntity))
        {
            var pelletTr = pelletTransformLookup[linkedPellet.PelletEntity];
            pelletTr.Position = new float3(px, pelletTr.Position.y, pz);
            pelletTransformLookup[linkedPellet.PelletEntity] = pelletTr;

            g.relativePelletPos = new float2(px, pz) / g.boundary;
        }

        move.moveInput = float2.zero;

        g.timeLeftNormalized = 1f;
        g.reward = 0f;
        g.episodeReward = 0f;
        g.isFailed = false;
        g.isSuccess = false;

        g.relativeAgentPos = new float2(ax, az) / g.boundary;

        float2 toPellet = g.relativePelletPos - g.relativeAgentPos;
        float invDist = math.rsqrt(math.max(math.lengthsq(toPellet), 1e-8f));
        g.toPelletNormalized = toPellet * invDist;

        agent.StartingEpisode = false;
        g.needsReset = false;
    }
}
