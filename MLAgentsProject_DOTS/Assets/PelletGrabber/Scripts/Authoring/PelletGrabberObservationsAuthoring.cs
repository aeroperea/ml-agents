using Unity.Entities;
using UnityEngine;
using Zelcam4.MLAgents;

// anchor: creates observation request list + allocates final observation buffer
public class PelletGrabberObservationsAuthoring : MonoBehaviour
{
    class Baker : Baker<PelletGrabberObservationsAuthoring>
    {
        public override void Bake(PelletGrabberObservationsAuthoring authoring)
        {
            var e = GetEntity(TransformUsageFlags.Dynamic);

            var req = AddBuffer<ObservationRequest<PelletGrabber>>(e);

            req.Add(new ObservationRequest<PelletGrabber> { SourceSubId = (byte)PelletGrabberObsId.rel_agent_x, TargetIndex = 0 });
            req.Add(new ObservationRequest<PelletGrabber> { SourceSubId = (byte)PelletGrabberObsId.rel_agent_y, TargetIndex = 1 });
            req.Add(new ObservationRequest<PelletGrabber> { SourceSubId = (byte)PelletGrabberObsId.time_left,   TargetIndex = 2 });
            req.Add(new ObservationRequest<PelletGrabber> { SourceSubId = (byte)PelletGrabberObsId.to_pellet_x, TargetIndex = 3 });
            req.Add(new ObservationRequest<PelletGrabber> { SourceSubId = (byte)PelletGrabberObsId.to_pellet_y, TargetIndex = 4 });

            var values = AddBuffer<ObservationValue>(e);
            values.ResizeUninitialized(5);
        }
    }
}
