using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;

[BurstCompile]
public struct ResetAgentJob : IJob
{
    public NativeArray<LocalTransform> Transforms; // Store Agent & Pellet Transform
    public NativeArray<PelletGrabber> RewardData; // Store RewardData as NativeArray
    public uint RandomSeed;

    public void Execute()
    {
        Random random = new Random(RandomSeed);

        // Modify Pellet Position (Index 1 in NativeArray)
        Transforms[1] = new LocalTransform
        {
            Position = new float3(random.NextFloat(-RewardData[0].boundary.x, RewardData[0].boundary.x), 0, random.NextFloat(-RewardData[0].boundary.y, RewardData[0].boundary.y)),
            Rotation = Transforms[1].Rotation,
            Scale = Transforms[1].Scale
        };

        int attempts = 0;

        // Modify Agent Position (Index 0 in NativeArray), ensuring a minimum distance
        do
        {
            Transforms[0] = new LocalTransform
            {
                Position = new float3(random.NextFloat(-RewardData[0].boundary.x, RewardData[0].boundary.x), 0, random.NextFloat(-RewardData[0].boundary.y, RewardData[0].boundary.y)),
                Rotation = Transforms[0].Rotation,
                Scale = Transforms[0].Scale
            };
            attempts++;
        }
        while (attempts < 20 && math.lengthsq(Transforms[0].Position - Transforms[1].Position) < (3f * 3f));

        // Force default positions if too many attempts
        if (attempts == 20)
        {
            Transforms[0] = new LocalTransform { Position = new float3(-7, 0, -7), Rotation = quaternion.identity, Scale = 1f };
            Transforms[1] = new LocalTransform { Position = new float3(7, 0, 7), Rotation = quaternion.identity, Scale = 1f };
        }

        // Store relative positions
        PelletGrabber updatedRewardData = RewardData[0]; // Get the first (and only) item in NativeArray
        updatedRewardData.relativePelletPos = new float2(Transforms[1].Position.x, Transforms[1].Position.z) / updatedRewardData.boundary;
        updatedRewardData.timeLeftNormalized = 1f;
        updatedRewardData.reward = 0f;
        updatedRewardData.isSuccess = false;
        updatedRewardData.isFailed = false;
        updatedRewardData.episodeReward = 0f;

        RewardData[0] = updatedRewardData; // Write back updated values
    }
}
