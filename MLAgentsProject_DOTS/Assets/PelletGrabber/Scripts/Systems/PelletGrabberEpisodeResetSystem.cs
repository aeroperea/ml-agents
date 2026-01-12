using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using Zelcam4.MLAgents;

// anchor: pelletgrabber-episode-reset-system
// runs after mlagents-dots has marked agents as StartingEpisode
[BurstCompile]
[UpdateAfter(typeof(AgentResetSystem))]
public partial struct PelletGrabberEpisodeResetSystem : ISystem
{
    private ComponentLookup<LocalTransform> _transformLookup;

    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        _transformLookup = state.GetComponentLookup<LocalTransform>(false);

        state.RequireForUpdate<AgentEcs>();
        state.RequireForUpdate<PelletGrabber>();
        state.RequireForUpdate<LinkedPellet>();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        _transformLookup.Update(ref state);

        var job = new ResetJob
        {
            transformLookup = _transformLookup
        };

        state.Dependency = job.ScheduleParallel(state.Dependency);
    }
}

// anchor: pelletgrabber-episode-reset-job
[BurstCompile]
public partial struct ResetJob : IJobEntity
{
    [NativeDisableParallelForRestriction]
    public ComponentLookup<LocalTransform> transformLookup;

    private void Execute(
        Entity entity,
        ref AgentEcs agent,
        ref PelletGrabber g,
        ref PelletGrabberMovement move,
        in LinkedPellet linkedPellet)
    {
        if (!agent.StartingEpisode && !g.needsReset)
            return;

        float2 boundarySafe = math.max(g.boundary, new float2(1e-6f, 1e-6f));

        uint seed = (uint)(agent.EpisodeId * 1000003) ^ (uint)(agent.CompletedEpisodes * 7919) ^ 0x9e3779b9u;
        if (seed == 0u) seed = 1u;
        var rng = Unity.Mathematics.Random.CreateFromIndex(seed);

        float ax = rng.NextFloat(-boundarySafe.x, boundarySafe.x);
        float az = rng.NextFloat(-boundarySafe.y, boundarySafe.y);

        float px = rng.NextFloat(-boundarySafe.x, boundarySafe.x);
        float pz = rng.NextFloat(-boundarySafe.y, boundarySafe.y);

        if (transformLookup.HasComponent(entity))
        {
            var selfTr = transformLookup[entity];
            selfTr.Position = new float3(ax, selfTr.Position.y, az);
            transformLookup[entity] = selfTr;
        }

        if (linkedPellet.PelletEntity != Entity.Null && transformLookup.HasComponent(linkedPellet.PelletEntity))
        {
            var pelletTr = transformLookup[linkedPellet.PelletEntity];
            pelletTr.Position = new float3(px, pelletTr.Position.y, pz);
            transformLookup[linkedPellet.PelletEntity] = pelletTr;
        }

        move.moveInput = float2.zero;

        g.timeLeftNormalized = 1f;
        g.reward = 0f;
        g.episodeReward = 0f;
        g.isFailed = false;
        g.isSuccess = false;

        g.relativeAgentPos = new float2(ax, az) / boundarySafe;
        g.relativePelletPos = new float2(px, pz) / boundarySafe;

        float2 toPellet = g.relativePelletPos - g.relativeAgentPos;
        float invLen = math.rsqrt(math.max(math.lengthsq(toPellet), 1e-8f));
        g.toPelletNormalized = toPellet * invLen;

        agent.StartingEpisode = false;
        g.needsReset = false;
    }
}
