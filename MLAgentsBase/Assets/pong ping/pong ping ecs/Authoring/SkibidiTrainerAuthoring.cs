using Unity.Entities;
using UnityEngine;
using Unity.Mathematics;

class SkibidiTrainerAuthoring : MonoBehaviour
{
    public float myFloat = 3;

    class Baker : Baker<SkibidiTrainerAuthoring>
    {
        public override void Bake(SkibidiTrainerAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.None);
            AddComponent(entity, new SkibidiTrainer 
            {
                myFloat = authoring.myFloat
            });
        }
    }
}

public struct SkibidiTrainer : IComponentData
{
    public float myFloat;
}
