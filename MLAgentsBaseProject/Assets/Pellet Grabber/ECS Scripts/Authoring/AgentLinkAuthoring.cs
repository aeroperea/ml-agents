using Unity.Entities;
using UnityEngine;

class AgentLinkAuthoring : MonoBehaviour
{

    class Baker : Baker<AgentLinkAuthoring>
    {
        public override void Bake(AgentLinkAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.None);
            AddComponent(entity, new AgentLink 
            {
                Entity = entity,
                AgentReference = authoring.GetComponent<PelletGrabberAgentECS>()
            });
        }
    }
}

public struct AgentLink : IComponentData
{
    public Entity Entity;
    public UnityObjectRef<PelletGrabberAgentECS> AgentReference;
}
