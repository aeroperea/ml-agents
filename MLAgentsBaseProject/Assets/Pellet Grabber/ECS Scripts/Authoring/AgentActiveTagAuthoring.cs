using Unity.Entities;
using UnityEngine;

class AgentActiveTagAuthoring : MonoBehaviour
{

    class Baker : Baker<AgentActiveTagAuthoring>
    {
        public override void Bake(AgentActiveTagAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.None);
            AddComponent(entity, new AgentActiveTag());
            SetComponentEnabled<AgentActiveTag>(entity, false);
        }
    }
}

public struct AgentActiveTag : IComponentData, IEnableableComponent
{
    
}
