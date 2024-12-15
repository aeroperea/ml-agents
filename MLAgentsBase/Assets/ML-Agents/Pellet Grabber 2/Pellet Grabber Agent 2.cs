using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using Random = UnityEngine.Random;

public class PelletGrabberAgent2 : Agent
{
    public Transform pelletT;
    public float moveSpeed = 10;

    public float hitPelletReward = 90f;
    public float speedRewardModifier = 2f;
    public float hitWallPenalty = -10f;
    public float maxTimePenalty = -10f;
    public float directionReward = 2;

    private float stepReward = 0;
    private float accumulatedReward = 0;

    public float maxTime = 10;
    private float startTime;
    private Vector3 toTarget;

    [SerializeField] public Vector3 startRange = new Vector3(14, 0, 14);
    [SerializeField] public Color victoryColor = Color.green;
    [SerializeField] public Color failColor = Color.red;
    [SerializeField] public MeshRenderer floorMeshRenderer;


    public override void OnEpisodeBegin()
    {
        accumulatedReward = 0;
        transform.localPosition = new Vector3(Random.Range(-startRange.x, startRange.x), 0, Random.Range(-startRange.z, startRange.z));
        pelletT.localPosition = new Vector3(Random.Range(-startRange.x, startRange.x), 0, Random.Range(-startRange.z, startRange.z));
        toTarget = pelletT.localPosition - transform.localPosition;
        startTime = Time.time;
    }

    public override void OnActionReceived(ActionBuffers actionBuffers)
    {
        stepReward = 0;

        float actionX = actionBuffers.ContinuousActions[0];
        float actionZ = actionBuffers.ContinuousActions[1];

        Vector3 movementVector = new Vector3(actionX,0,actionZ);
        transform.Translate(movementVector);

        toTarget = pelletT.localPosition - transform.localPosition;
        CalculateRewards(movementVector);
    }

    void CalculateRewards(Vector3 movementVector)
    {
        float movementDot = Vector3.Dot(movementVector, toTarget);
        AddReward(Mathf.Pow(directionReward, 1 + movementDot));

        if(Mathf.Abs(transform.localPosition.x) > startRange.x || Mathf.Abs(transform.localPosition.z) > startRange.z)
        {
            AddReward(hitWallPenalty);
            stepReward += hitWallPenalty;
            accumulatedReward += hitWallPenalty;
            SetFloorColor(failColor);
            EndEpisode();
        }

        if(Time.time > startTime + maxTime)
        {
            AddReward(maxTimePenalty);
            stepReward += maxTimePenalty;
            accumulatedReward += maxTimePenalty;
            SetFloorColor(failColor);
            EndEpisode();
            Debug.Log($"accumulated reward {accumulatedReward}");
        }

        if(toTarget.magnitude <= 1)
        {
            AddReward(hitPelletReward);
            stepReward += hitPelletReward;
            accumulatedReward += hitPelletReward;
            SetFloorColor(victoryColor);
            EndEpisode();
        }

        Debug.Log($"step reward {stepReward}");
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var continuousActionOut = actionsOut.ContinuousActions;
        continuousActionOut[0] = Input.GetAxis("Horizontal");
        continuousActionOut[1] = Input.GetAxis("Vertical");
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        toTarget = pelletT.localPosition - transform.localPosition;

        sensor.AddObservation(toTarget.x);
        sensor.AddObservation(toTarget.z);
        sensor.AddObservation(transform.localPosition.x);
        sensor.AddObservation(transform.localPosition.z);
    }


    private void SetFloorColor(Color newColor)
    {
        MaterialPropertyBlock propertyBlock = new MaterialPropertyBlock();
        floorMeshRenderer.GetPropertyBlock(propertyBlock);
        propertyBlock.SetColor("_Color", newColor);
        floorMeshRenderer.SetPropertyBlock(propertyBlock);
    }    
}
