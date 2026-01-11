using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Zelcam4.MLAgents;

// anchor: packs pelletgrabber -> observationvalue[] only on decision frames
[BurstCompile]
[UpdateInGroup(typeof(ObservationCollectionGroup))]
public partial struct PelletGrabberObservationGatherSystem : ISystem
{
    private EntityQuery q;

    private BufferTypeHandle<ObservationValue> valuesHandle;
    private BufferTypeHandle<ObservationRequest<PelletGrabber>> reqHandle;
    private ComponentTypeHandle<PelletGrabber> srcHandle;

    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        q = new EntityQueryBuilder(Allocator.Temp)
            .WithAll<ObservationValue, ObservationRequest<PelletGrabber>, PelletGrabber, RequestDecisionTag>()
            .Build(ref state);

        valuesHandle = state.GetBufferTypeHandle<ObservationValue>(false);
        reqHandle = state.GetBufferTypeHandle<ObservationRequest<PelletGrabber>>(true);
        srcHandle = state.GetComponentTypeHandle<PelletGrabber>(true);
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        valuesHandle.Update(ref state);
        reqHandle.Update(ref state);
        srcHandle.Update(ref state);

        var job = new GatherJob<PelletGrabber, PelletGrabberExtractor>
        {
            FinalObservationBufferHandle = valuesHandle,
            RequestsHandle = reqHandle,
            SourceComponentHandle = srcHandle,
            Extractor = default
        };

        state.Dependency = job.ScheduleParallel(q, state.Dependency);
    }
}
