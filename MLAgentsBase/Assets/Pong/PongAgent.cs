using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using Random = UnityEngine.Random;

public class PongAgent : Agent
{
    //variables
    float stepReward = 0; //reward for the current step
    float accumulatedReward = 0; //reward for episode
    public override void OnEpisodeBegin()
    {
        
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        
    }

    public override void OnActionReceived(ActionBuffers actionBuffers)
    {
        
    }

    void GiveReward(float reward)
    {
        AddReward(reward);
        stepReward += reward;
        accumulatedReward += reward;
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        
    }
}
