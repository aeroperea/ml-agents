using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Zelcam4.MLAgents;

// anchor: action -> moveinput (replaces onactionreceived)
[BurstCompile]
[UpdateInGroup(typeof(ActionSystemGroup))]
public partial struct PelletGrabberActionInputSystem : ISystem
{
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        state.Dependency = new ActionJob().ScheduleParallel(state.Dependency);
    }

    [BurstCompile]
    public partial struct ActionJob : IJobEntity
    {
        private void Execute(ref PelletGrabberMovement move, in AgentAction action, in AgentEcs agent, in RequestActionTag _)
        {
            if (agent.StartingEpisode)
            {
                move.moveInput = float2.zero;
                return;
            }

            float x = (action.ContinuousActions.Length > 0) ? action.ContinuousActions[0] : 0f;
            float y = (action.ContinuousActions.Length > 1) ? action.ContinuousActions[1] : 0f;

            move.moveInput = math.clamp(new float2(x, y), new float2(-1f), new float2(1f));
        }
    }
}
