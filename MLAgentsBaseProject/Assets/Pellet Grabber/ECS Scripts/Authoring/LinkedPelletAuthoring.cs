using Unity.Entities;
using UnityEngine;

class LinkedPelletAuthoring : MonoBehaviour
{
    public GameObject linkedPelletGO;

    class Baker : Baker<LinkedPelletAuthoring>
    {
        public override void Bake(LinkedPelletAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new LinkedPellet
            {
                PelletEntity = GetEntity(authoring.linkedPelletGO, TransformUsageFlags.Dynamic),
            });
        }
    }
}

public struct LinkedPellet : IComponentData
{
    public Entity PelletEntity;
}

