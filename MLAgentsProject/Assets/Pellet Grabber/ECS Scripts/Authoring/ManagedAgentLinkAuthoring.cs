using Unity.Entities;
using UnityEngine;
using Unity.Mathematics;
using Unity.MLAgents;

class ManagedAgentLinkAuthoring : MonoBehaviour
{

    class Baker : Baker<ManagedAgentLinkAuthoring>
    {
        public override void Bake(ManagedAgentLinkAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.None);
            AddComponentObject(entity, new ManagedAgentLink { AgentReference = null });
        }
    }
}



public class ManagedAgentLink : IComponentData
{
    public Agent AgentReference;
}
