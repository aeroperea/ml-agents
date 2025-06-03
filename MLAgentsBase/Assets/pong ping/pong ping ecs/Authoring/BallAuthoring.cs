using Unity.Entities;
using UnityEngine;
using Unity.Mathematics;

class BallAuthoring : MonoBehaviour
{
    public float myFloat = 3;

    class Baker : Baker<BallAuthoring>
    {
        public override void Bake(BallAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new Ball 
            {
                myFloat = authoring.myFloat
            });
        }
    }
}

public struct Ball : IComponentData
{
    public float myFloat;
}
