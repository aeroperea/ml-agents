using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using Random = UnityEngine.Random;

public class PelletGrabberAgent : Agent
{
    [SerializeField] Transform pelletT;
    private float moveSpeed = 10;
    private float hitPelletReward = 90f; // find a reward formula for this later
    private float hitWallPenalty = -1f;
    private float maxTimePenalty = -90f;
    private float proximityReward = 5f;
    private float distancePenalty = -0.5f;
    // private float wrongWayPenaltyModifier = 0.75f;
    private float penaltyPerStep = -0.01f;
    private float speedRewardModifier = 2;


    private float maxTime = 10;
    Vector3 toTarget;
    float lastClosestSq;
    float sqrMagnitude;
    float startTime;
    float timePassed;
    float proximityCheckInterval = 0.1f;
    float lastProximityCheckTime;

    private float exponentialBias = 0.15f;

    // [Header("Environment Variables")]
    public Vector3 startRange = new Vector3(14, 0, 14);
    public Color victoryColor = Color.green;
    public Color failColor = Color.red;
    public MeshRenderer floorMeshRenderer;

    // Flags for managing episode termination
    private bool isEpisodeDone = false;
    private bool isSuccess = false;

    private float accumulatedReward; // Stores the total reward for the current episode

    public override void OnEpisodeBegin()
    {
        accumulatedReward = 0;
        startTime = Time.time;
        timePassed = 0;
        lastProximityCheckTime = Time.time;
        transform.localPosition = new Vector3(Random.Range(-startRange.x, startRange.x), startRange.y, Random.Range(-startRange.z, startRange.z));
        pelletT.localPosition = new Vector3(Random.Range(-startRange.x, startRange.x), startRange.y, Random.Range(-startRange.z, startRange.z));
        toTarget = pelletT.localPosition - transform.localPosition;
        sqrMagnitude = Mathf.Abs(toTarget.sqrMagnitude);
        lastClosestSq = sqrMagnitude;
        // Debug.Log("last closest on episode begin" + lastClosestSq);
        isEpisodeDone = false;
        isSuccess = false;
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var continuousActionOut = actionsOut.ContinuousActions;
        continuousActionOut[0] = Input.GetAxis("Horizontal");
        continuousActionOut[1] = Input.GetAxis("Vertical");
    }

    public override void OnActionReceived(ActionBuffers actionBuffers)
    {
        float actionX = actionBuffers.ContinuousActions[0];
        float actionZ = actionBuffers.ContinuousActions[1];

        Vector3 movementVector = new Vector3(actionX, 0, actionZ) * moveSpeed * Time.deltaTime;
        transform.Translate(movementVector, Space.World);

        timePassed = Time.time - startTime;

        toTarget = pelletT.localPosition - transform.localPosition;
        sqrMagnitude = Mathf.Abs(toTarget.sqrMagnitude);
        float distanceDelta = Mathf.Abs(lastClosestSq - sqrMagnitude);
        bool isCloser = sqrMagnitude < lastClosestSq;

        // Add rewards or penalties and update accumulated reward
        if (Time.time > lastProximityCheckTime + proximityCheckInterval)
        {
            float distanceRewardSignal = Mathf.Pow(
    (1 / Mathf.Max(sqrMagnitude, 0.0001f)) * (1 + distanceDelta) * proximityReward,
    1f + exponentialBias
);

            float reward = isCloser
                ? Mathf.Clamp(distanceRewardSignal, 0, (proximityReward * distanceDelta) ^ (1f + exponentialBias)))
                : Mathf.Clamp(distancePenalty * sqrMagnitude * 0.0025f, distancePenalty * 0.25f, 0);

            AddReward(reward);
            accumulatedReward += reward; // Update accumulated reward
            lastProximityCheckTime = Time.time;
        }

        if (Mathf.Abs(transform.localPosition.x) > startRange.x || Mathf.Abs(transform.localPosition.z) > startRange.z)
        {
            AddReward(hitWallPenalty);
            accumulatedReward += hitWallPenalty; // Update accumulated reward
        }

        if (toTarget.magnitude < 1f)
        {
            isSuccess = true;
            isEpisodeDone = true;
        }

        if (timePassed > maxTime)
        {
            AddReward(maxTimePenalty);
            isEpisodeDone = true;
            accumulatedReward += maxTimePenalty; // Update accumulated reward
        }
        else
        {
            AddReward(penaltyPerStep);
            accumulatedReward += penaltyPerStep; // Update accumulated reward
        }

        if (isEpisodeDone)
        {
            OnEpisodeEnd(isSuccess ? CalculateSuccessReward() : 0, isSuccess);
        }

        lastClosestSq = Mathf.Min(sqrMagnitude, lastClosestSq);
    }

    private float CalculateSuccessReward()
    {
        timePassed = Time.time - startTime;
        if (timePassed > 0.001f) // Avoid division by zero
        {
            return hitPelletReward * Mathf.Exp(-speedRewardModifier * timePassed / maxTime);
        }
        return hitPelletReward;
    }

    public void OnEpisodeEnd(float reward, bool success = false)
    {
        if (success)
        {
            reward += CalculateSuccessReward();
            SetFloorColor(victoryColor);
            Debug.Log($"hit pelletReward {reward}");
            AddReward(reward);
        }
        else
        {
            SetFloorColor(failColor);
        }
        EndEpisode();

        // Reset flags
        isEpisodeDone = false;
        isSuccess = false;
    }


    void SetFloorColor(Color newColor)
    {
        // Create a new MaterialPropertyBlock
        MaterialPropertyBlock propertyBlock = new MaterialPropertyBlock();

        // Get the current MaterialPropertyBlock values from the renderer
        floorMeshRenderer.GetPropertyBlock(propertyBlock);

        // Set the color property (assumes the shader uses "_Color" for the main color)
        propertyBlock.SetColor("_Color", newColor);

        // Apply the updated MaterialPropertyBlock to the renderer
        floorMeshRenderer.SetPropertyBlock(propertyBlock);
    }

}
