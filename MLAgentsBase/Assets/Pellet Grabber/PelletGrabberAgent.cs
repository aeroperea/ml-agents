using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using Random = UnityEngine.Random;

public class PelletGrabberAgent : Agent
{
    // [SerializeField] Transform pelletT;
    // private float moveSpeed = 10f;
    // private float hitPelletReward = 200f;
    // private float hitWallPenalty = -75f;
    // private float maxTimePenalty = -50f;
    // private float directionReward = 3f;
    // // private float penaltyPerStep = -1f;
    // private float speedRewardModifier = 2f;

    // private float maxTime = 10f;
    // private Vector3 toTarget;
    // private float startTime;
    // private float accumulatedReward = 0f;
    // private float stepReward = 0f;

    // [SerializeField] public Vector3 startRange = new Vector3(14, 0, 14);
    // [SerializeField] public Color victoryColor = Color.green;
    // [SerializeField] public Color failColor = Color.red;
    // [SerializeField] public MeshRenderer floorMeshRenderer;

    // // private bool isSuccess = false;

    // public override void OnEpisodeBegin()
    // {
    //     accumulatedReward = 0f;
    //     stepReward = 0f;
    //     startTime = Time.time;
    //     isSuccess = false;

    //     // Reset positions
    //     transform.localPosition = new Vector3(
    //         Random.Range(-startRange.x, startRange.x),
    //         startRange.y,
    //         Random.Range(-startRange.z, startRange.z)
    //     );

    //     pelletT.localPosition = new Vector3(
    //         Random.Range(-startRange.x, startRange.x),
    //         startRange.y,
    //         Random.Range(-startRange.z, startRange.z)
    //     );
    //     toTarget = pelletT.localPosition - transform.localPosition;
    // }

    // public override void Heuristic(in ActionBuffers actionsOut)
    // {
    //     var continuousActionOut = actionsOut.ContinuousActions;
    //     continuousActionOut[0] = Input.GetAxis("Horizontal");
    //     continuousActionOut[1] = Input.GetAxis("Vertical");
    // }

    // public override void OnActionReceived(ActionBuffers actionBuffers)
    // {
    //     float reward;
    //     stepReward = 0f;

    //     float actionX = actionBuffers.ContinuousActions[0];
    //     float actionZ = actionBuffers.ContinuousActions[1];

    //     Vector3 movementVector = new Vector3(actionX, 0, actionZ) * moveSpeed * Time.deltaTime;
    //     transform.Translate(movementVector, Space.World);

    //     float timePassed = Time.time - startTime;
    //     movementVector = movementVector.normalized;
    //     toTarget = pelletT.localPosition - transform.localPosition;
    //     float movementDot = Vector3.Dot(movementVector, toTarget.normalized);
        

    //     // Compute reward if moving closer
    //     // Determine reward or penalty 
    //     float reward;
    //     if(movementDot > 0.9)
    //     {
    //         reward = Mathf.Pow(directionReward, 1 + Mathf.Abs(movementDot));
    //     }
    //     else if(movementDot < 0)
    //     {
    //         reward = -1;
    //     }

    //     GiveReward(reward);
        
    //     // Boundary penalty
    //     if (Mathf.Abs(transform.localPosition.x) > startRange.x || Mathf.Abs(transform.localPosition.z) > startRange.z)
    //     {
    //         GiveReward(hitWallPenalty);
    //         OnEpisodeEnd(false);
    //     }

    //     // Check success
    //     if (toTarget.magnitude < 1f)
    //     {
    //         OnEpisodeEnd(true);
    //     }

    //     // Time-based penalty
    //     if (timePassed > maxTime)
    //     {
    //         GiveReward(maxTimePenalty);
    //         OnEpisodeEnd(false);
    //     }
    //     else
    //     {
    //         // GiveReward(penaltyPerStep);
    //     }

    //     Debug.Log($"Step Reward: {stepReward}, toTarget: {toTarget.normalized}, movementVector: {movementVector}, movementDot: {movementDot}");
    // }

    // void GiveReward(float reward)
    // {
    //     AddReward(reward);
    //     stepReward += reward;
    //     accumulatedReward += reward;
    // }

    // public override void CollectObservations(VectorSensor sensor)
    // {
    //     toTarget = pelletT.localPosition - transform.localPosition;

    //     sensor.AddObservation(toTarget.x);
    //     sensor.AddObservation(toTarget.z);
    //     sensor.AddObservation(transform.localPosition.x);
    //     sensor.AddObservation(transform.localPosition.z);
    // }

    // public void OnEpisodeEnd(bool success = false)
    // {
    //     if (success)
    //     {
    //         float timePassed = Time.time - startTime;
    //         float reward = hitPelletReward;
    //         if (timePassed > 0.001f)
    //         {
    //             reward = hitPelletReward * (1 - (timePassed / maxTime)) * speedRewardModifier;
    //         }    
    //         SetFloorColor(victoryColor);
    //         GiveReward(reward);
    //         Debug.Log($"Episode Success! Accumulated Reward: {accumulatedReward}");
    //         isSuccess = true;
    //     }
    //     else
    //     {
    //         SetFloorColor(failColor);
    //         Debug.Log($"Mission Failed... we'll get em next time Accumulated Reward: {accumulatedReward}");
    //     }
    //     EndEpisode();

        
    // }

    // private void SetFloorColor(Color newColor)
    // {
    //     MaterialPropertyBlock propertyBlock = new MaterialPropertyBlock();
    //     floorMeshRenderer.GetPropertyBlock(propertyBlock);
    //     propertyBlock.SetColor("_Color", newColor);
    //     floorMeshRenderer.SetPropertyBlock(propertyBlock);
    // }
}
