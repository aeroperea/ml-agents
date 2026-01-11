using Unity.Entities;
using UnityEngine;
using Unity.Mathematics;

public class PelletGrabberAuthoring : MonoBehaviour
{
    // anchor: links
    public Transform pelletTransform;

    // anchor: config
    public float maxTime = 10f;
    public float2 boundary = new float2(14f, 14f);
    public float moveSpeed = 10f;

    class Baker : Baker<PelletGrabberAuthoring>
    {
        public override void Bake(PelletGrabberAuthoring authoring)
        {
            // anchor: agent entity
            Entity e = GetEntity(TransformUsageFlags.Dynamic);

            // anchor: pellet entity link
            Entity pelletE = Entity.Null;
            if (authoring.pelletTransform != null)
                pelletE = GetEntity(authoring.pelletTransform, TransformUsageFlags.Dynamic);

            AddComponent(e, new LinkedPellet { PelletEntity = pelletE });

            AddComponent(e, new PelletGrabber
            {
                invMaxTime = (authoring.maxTime > 0f) ? (1f / authoring.maxTime) : 0f,
                timeLeftNormalized = 1f,
                boundary = authoring.boundary,
                relativePelletPos = float2.zero,
                relativeAgentPos = float2.zero,
                toPelletNormalized = new float2(1f, 0f),
                reward = 0f,
                episodeReward = 0f,
                isFailed = false,
                isSuccess = false,
                needsReset = true
            });

            AddComponent(e, new PelletGrabberMovement
            {
                moveSpeed = authoring.moveSpeed,
                moveInput = float2.zero
            });
        }
    }
}
