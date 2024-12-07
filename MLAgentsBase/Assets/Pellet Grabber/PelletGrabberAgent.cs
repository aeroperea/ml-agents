using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using Random = UnityEngine.Random;

public class PelletGrabberAgent : Agent
{
    [SerializeField] Transform pelletT;
    private float moveSpeed = 10f;
    private float hitPelletReward = 90f;
    private float hitWallPenalty = -10f;
    private float maxTimePenalty = -10f;
    private float proximityReward = 5f;
    // private float distancePenalty = -1f;
    private float penaltyPerStep = 0f;
    private float speedRewardModifier = 2f;
    public float exponentialBias = 0.15f;

    private float maxTime = 10f;
    private Vector3 toTarget;
    private float lastMagSq;
    private float initialSqrDist;
    private float sqrMagnitude;
    private float startTime;
    private float accumulatedReward = 0f;
    private float stepReward = 0f;

    [SerializeField] public Vector3 startRange = new Vector3(14, 0, 14);
    [SerializeField] public Color victoryColor = Color.green;
    [SerializeField] public Color failColor = Color.red;
    [SerializeField] public MeshRenderer floorMeshRenderer;

    private bool isEpisodeDone = false;
    private bool isSuccess = false;

    public override void OnEpisodeBegin()
    {
        accumulatedReward = 0f;
        stepReward = 0f;
        startTime = Time.time;

        // Reset positions
        transform.localPosition = new Vector3(
            Random.Range(-startRange.x, startRange.x),
            startRange.y,
            Random.Range(-startRange.z, startRange.z)
        );

        pelletT.localPosition = new Vector3(
            Random.Range(-startRange.x, startRange.x),
            startRange.y,
            Random.Range(-startRange.z, startRange.z)
        );

        Vector3 initialToTarget = pelletT.localPosition - transform.localPosition;
        initialSqrDist = Mathf.Max(initialToTarget.sqrMagnitude, 0.0001f); // Prevent division by zero
        lastMagSq = initialSqrDist;
        lastPosition = transform.localPosition;

        isEpisodeDone = false;
        isSuccess = false;
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var continuousActionOut = actionsOut.ContinuousActions;
        continuousActionOut[0] = Input.GetAxis("Horizontal");
        continuousActionOut[1] = Input.GetAxis("Vertical");
    }

    Vector3 lastPosition;

    public override void OnActionReceived(ActionBuffers actionBuffers)
    {
        stepReward = 0f;

        float actionX = actionBuffers.ContinuousActions[0];
        float actionZ = actionBuffers.ContinuousActions[1];

        Vector3 movementVector = new Vector3(actionX, 0, actionZ) * moveSpeed * Time.deltaTime;
        transform.Translate(movementVector, Space.World);

        float timePassed = Time.time - startTime;
        movementVector = movementVector.normalized;
        toTarget = pelletT.localPosition - transform.localPosition;
        float movementDot = Vector3.Dot(movementVector, toTarget.normalized);
        sqrMagnitude = Mathf.Abs(Mathf.Max(toTarget.sqrMagnitude, 0.0001f)); // Prevent invalid math
        
        bool isCloser = sqrMagnitude < lastMagSq;
        bool inDirection = movementDot == 1;

        // Calculate improvement based on squared distance traveled toward the target
        float distanceTraveledTowardTarget = initialSqrDist - sqrMagnitude;

        // Compute reward if moving closer
        float penaltyIfFarther = Mathf.Abs(Mathf.Pow(proximityReward, 0.00042f * Mathf.Pow(proximityReward * (1 + Mathf.Abs(distanceTraveledTowardTarget)), 1f + exponentialBias)));

        // Determine reward or penalty
        float reward = isCloser
            ? Mathf.Clamp(Mathf.Pow(proximityReward, proximityReward * movementDot * 0.25f), 0f, 10)
            : Mathf.Clamp(-penaltyIfFarther, -1000f, 0f) * 0.6f;
        // //Debug.Log($"weird thing { Mathf.Pow(Mathf.Abs(-distancePenalty * (1 + -distanceTraveledTowardTarget)), 1f + exponentialBias)} isCloser {isCloser}");
        AddReward(reward);
        stepReward += reward;
        accumulatedReward += reward;
        lastPosition = transform.localPosition;

        // Boundary penalty
        if (Mathf.Abs(transform.localPosition.x) > startRange.x || Mathf.Abs(transform.localPosition.z) > startRange.z)
        {
            AddReward(hitWallPenalty);
            stepReward += hitWallPenalty;
            accumulatedReward += hitWallPenalty;
            isEpisodeDone = true;
        }

        // Check success
        if (toTarget.magnitude < 1f)
        {
            isSuccess = true;
            isEpisodeDone = true;
        }

        // Time-based penalty
        if (timePassed > maxTime)
        {
            AddReward(maxTimePenalty);
            stepReward += maxTimePenalty;
            accumulatedReward += maxTimePenalty;
            isEpisodeDone = true;
        }
        else
        {
            AddReward(penaltyPerStep);
            stepReward += penaltyPerStep;
            accumulatedReward += penaltyPerStep;
        }

        Debug.Log($"Step Reward: {stepReward}, toTarget: {toTarget}, Time Passed: {timePassed}");

        if (isEpisodeDone)
        {
            OnEpisodeEnd(isSuccess ? hitPelletReward : 0, isSuccess);
        }

        lastMagSq = sqrMagnitude;
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        toTarget = pelletT.localPosition - transform.localPosition;

        sensor.AddObservation(toTarget.x / startRange.x);
        sensor.AddObservation(toTarget.z / startRange.z);
        sensor.AddObservation(toTarget.magnitude / (startRange.magnitude));
        sensor.AddObservation(transform.localPosition.x / startRange.x);
        sensor.AddObservation(transform.localPosition.z / startRange.z);
        Vector3 normalizedDirection = toTarget.normalized;
        sensor.AddObservation(normalizedDirection.x);
        sensor.AddObservation(normalizedDirection.z);
        float timeRemaining = Mathf.Clamp01((maxTime - (Time.time - startTime)) / maxTime);
        sensor.AddObservation(timeRemaining);
        sensor.AddObservation(isSuccess ? 1f : 0f);
    }

    public void OnEpisodeEnd(float reward, bool success = false)
    {
        if (success)
        {
            float timePassed = Time.time - startTime;
            if (timePassed > 0.001f)
            {
                reward += hitPelletReward * (1 - (timePassed / maxTime)) * speedRewardModifier;
            }
            SetFloorColor(victoryColor);
            AddReward(reward);
            accumulatedReward += reward;
            Debug.Log($"Episode Success! Accumulated Reward: {accumulatedReward}");
        }
        else
        {
            SetFloorColor(failColor);
            Debug.Log($"Episode Failed. Accumulated Reward: {accumulatedReward}");
        }
        EndEpisode();

        isEpisodeDone = false;
        isSuccess = false;
    }

    private void SetFloorColor(Color newColor)
    {
        MaterialPropertyBlock propertyBlock = new MaterialPropertyBlock();
        floorMeshRenderer.GetPropertyBlock(propertyBlock);
        propertyBlock.SetColor("_Color", newColor);
        floorMeshRenderer.SetPropertyBlock(propertyBlock);
    }
}
