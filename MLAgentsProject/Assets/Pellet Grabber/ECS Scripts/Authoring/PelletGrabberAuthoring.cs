using Unity.Entities;
using UnityEngine;
using Unity.Mathematics;

class PelletGrabberAuthoring : MonoBehaviour
{
    public float maxTime;
    public float2 boundary = new float2(14,14);
    public float moveSpeed = 10f;
    class Baker : Baker<PelletGrabberAuthoring>
    {
        public override void Bake(PelletGrabberAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic );
            AddComponent(entity, new PelletGrabber
            {
                invMaxTime = 1 / authoring.maxTime,
                timeLeftNormalized = 1,
                boundary = authoring.boundary,
            });

            AddComponent(entity, new PelletGrabberMovement 
            {
                moveSpeed = authoring.moveSpeed
            });
        }
    }
}

public struct PelletGrabber : IComponentData
{
    public float2 relativePelletPos;
    public float2 relativeAgentPos;
    public float2 toPelletNormalized;
    public float invMaxTime;
    public float timeLeftNormalized;

    public float2 boundary;

    public float reward;
    public float episodeReward;

    public bool isFailed;
    public bool isSuccess;
}

public struct PelletGrabberMovement : IComponentData
{
    public float moveSpeed;
    public float2 moveInput;
}