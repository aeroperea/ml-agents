using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using Zelcam4.MLAgents;

// anchor: updates cached observation fields right before gather/send
[BurstCompile]
[UpdateAfter(typeof(IncrementStepSystem))]
[UpdateBefore(typeof(ObservationCollectionGroup))]
public partial struct PelletGrabberObservationStateSystem : ISystem
{
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var pelletLookup = SystemAPI.GetComponentLookup<LocalTransform>(true);

        var job = new ObsStateJob
        {
            pelletTransformLookup = pelletLookup
        };

        state.Dependency = job.ScheduleParallel(state.Dependency);
    }
}

[BurstCompile]
[WithAll(typeof(RequestDecisionTag))]
public partial struct ObsStateJob : IJobEntity
{
    [ReadOnly] public ComponentLookup<LocalTransform> pelletTransformLookup;

    private void Execute(ref PelletGrabber g, in LocalTransform self, in LinkedPellet linkedPellet, in AgentEcs agent)
    {
        if (agent.StartingEpisode)
            return;

        if (linkedPellet.PelletEntity != Entity.Null && pelletTransformLookup.HasComponent(linkedPellet.PelletEntity))
        {
            var pelletTr = pelletTransformLookup[linkedPellet.PelletEntity];
            g.relativePelletPos = new float2(pelletTr.Position.x, pelletTr.Position.z) / g.boundary;
        }

        g.relativeAgentPos = new float2(self.Position.x, self.Position.z) / g.boundary;

        float2 toPellet = g.relativePelletPos - g.relativeAgentPos;
        float invDist = math.rsqrt(math.max(math.lengthsq(toPellet), 1e-8f));
        g.toPelletNormalized = toPellet * invDist;
    }
}
