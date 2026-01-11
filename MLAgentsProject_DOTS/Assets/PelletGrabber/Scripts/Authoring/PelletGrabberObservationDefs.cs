using Unity.Entities;
using Zelcam4.MLAgents;

// anchor: generic registration required by this package
[assembly: RegisterGenericComponentType(typeof(ObservationRequest<PelletGrabber>))]

public enum PelletGrabberObsId : byte
{
    rel_agent_x = 0,
    rel_agent_y = 1,
    time_left = 2,
    to_pellet_x = 3,
    to_pellet_y = 4
}

public struct PelletGrabberExtractor : IObservationExtractor<PelletGrabber>
{
    public float Extract(in PelletGrabber g, byte sourceSubId)
    {
        switch ((PelletGrabberObsId)sourceSubId)
        {
            case PelletGrabberObsId.rel_agent_x: return g.relativeAgentPos.x;
            case PelletGrabberObsId.rel_agent_y: return g.relativeAgentPos.y;
            case PelletGrabberObsId.time_left:   return g.timeLeftNormalized;
            case PelletGrabberObsId.to_pellet_x: return g.toPelletNormalized.x;
            case PelletGrabberObsId.to_pellet_y: return g.toPelletNormalized.y;
            default: return 0f;
        }
    }
}
