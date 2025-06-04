using Unity.Entities;
using UnityEngine;
using Unity.Mathematics;

class PongBallAuthoring : MonoBehaviour
{
    public GameObject linkedEnv;

    class Baker : Baker<PongBallAuthoring>
    {
        public override void Bake(PongBallAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new PongBallResetFlag());
            SetComponentEnabled<PongBallResetFlag>(entity, false);
        }
    }
}

public struct PongBall : IComponentData
{
    public float2 relativePos;
}
public struct PongBallResetFlag : IComponentData, IEnableableComponent { }
