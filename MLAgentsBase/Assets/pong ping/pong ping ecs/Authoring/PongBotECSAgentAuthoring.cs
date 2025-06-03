using Unity.Entities;
using UnityEngine;
using Unity.Mathematics;

class PongBotECSAgentAuthoring : MonoBehaviour
{
    public float myFloat = 3;

    class Baker : Baker<PongBotECSAgentAuthoring>
    {
        public override void Bake(PongBotECSAgentAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new PongBotECSAgent 
            {
                myFloat = authoring.myFloat
            });
        }
    }
}

public struct PongBotECSAgent : IComponentData
{
    public float myFloat;
}
