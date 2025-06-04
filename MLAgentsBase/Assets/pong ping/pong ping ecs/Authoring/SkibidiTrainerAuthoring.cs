using Unity.Entities;
using UnityEngine;
using Unity.Mathematics;

class SkibidiTrainerAuthoring : MonoBehaviour
{
    public GameObject leftAgentGo;
    public GameObject rightAgentGo;
    public GameObject ballGo;


    class Baker : Baker<SkibidiTrainerAuthoring>
    {
        public override void Bake(SkibidiTrainerAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.None);
            AddComponent(entity, new TrainerLink 
            {
                leftAgent = GetEntity(authoring.leftAgentGo, TransformUsageFlags.Dynamic),
                rightAgent = GetEntity(authoring.rightAgentGo, TransformUsageFlags.Dynamic),
                ball = GetEntity(authoring.ballGo, TransformUsageFlags.Dynamic),
            });
        }
    }
}

public struct TrainerLink : IComponentData
{
    public Entity leftAgent;
    public Entity rightAgent;
    public Entity ball;
}
