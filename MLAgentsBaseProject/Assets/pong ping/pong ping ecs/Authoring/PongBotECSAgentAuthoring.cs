using Unity.Entities;
using UnityEngine;
using Unity.Mathematics;

class PongBotECSAgentAuthoring : MonoBehaviour
{
    public GameObject linkedTrainerGO;
    public GameObject linkedBallGO;

    class Baker : Baker<PongBotECSAgentAuthoring>
    {
        public override void Bake(PongBotECSAgentAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new PongBotECSAgent
            {
                width = (half)authoring.transform.lossyScale.x,
                halfWidth = (half)(authoring.transform.lossyScale.x * 0.5f),
                negativeSide = authoring.transform.localPosition.z < 0,
                envPos = authoring.linkedTrainerGO.transform.position
            });
            AddComponent(entity, new PongBotRelativePosX());
            AddComponent(entity, new PongBotRewardData());
            AddComponent(entity, new PongAgentLink
            {
                trainer = GetEntity(authoring.linkedTrainerGO, TransformUsageFlags.Dynamic),
                ball = GetEntity(authoring.linkedBallGO, TransformUsageFlags.Dynamic),
            });
        }
    }
}

public struct PongBotECSAgent : IComponentData
{
    public half width;
    public half halfWidth;
    public bool negativeSide;
    public float3 envPos;
}

public struct PongBotObservationData : IComponentData
{
    public half2 relativeBallDir;
    public half2 normalizedBallPos;
}

public struct PongBotRelativePosX : IComponentData
{
    half value;
}

public struct PongBotRewardData : IComponentData
{
    public float reward;
    public byte resetFlags; // first byte = win, second byte = lose, third byte = tie
}

public struct PongAgentLink : IComponentData
{
    public Entity trainer;
    public Entity ball;
}
