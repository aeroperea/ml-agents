using Unity.Entities;
using UnityEngine;
using Unity.Mathematics;

class PelletGrabberMovementAuthoring : MonoBehaviour
{
    public float moveSpeed = 10f;

    class Baker : Baker<PelletGrabberMovementAuthoring>
    {
        public override void Bake(PelletGrabberMovementAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new PelletGrabberMovement 
            {
                moveSpeed = authoring.moveSpeed
            });
        }
    }
}

public struct PelletGrabberMovement : IComponentData
{
    public float moveSpeed;
    public float2 moveInput;
}
