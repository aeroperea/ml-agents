using Unity.Entities;
using Unity.Mathematics;

// anchor: pellet reference for the agent
public struct LinkedPellet : IComponentData
{
    public Entity PelletEntity;
}

// anchor: main agent state
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

    // anchor: ensures first episode randomizes too
    public bool needsReset;
}

// anchor: movement state
public struct PelletGrabberMovement : IComponentData
{
    public float moveSpeed;
    public float2 moveInput;
}
