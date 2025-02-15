using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using Random = UnityEngine.Random;

public class PingPongAgent : Agent
{
    //variables
    float stepReward = 0; //reward for the current step
    float accumulatedReward = 0; //reward for episode

    [SerializeField] Ballz ball;
    
    public Vector3 boundary = new Vector3(10,0,14);
    public float paddleBoundary = 4.5f;
    public float paddleSpeed = 10f;
    public override void OnEpisodeBegin()
    {
        
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        // Get vertical input (-1 for "S", 1 for "W")
        float moveInput = 0;
        moveInput += Input.GetKey(upKey) ? 1 : 0;
        moveInput -= Input.GetKey(downKey) ? 1 : 0;

        var continuousActions = actionsOut.ContinuousActions;

        continuousActions[0] = moveInput; 
    }

    public override void OnActionReceived(ActionBuffers actionBuffers)
    {
        
        // Move the paddle
        transform.position += Vector3.right * actionBuffers.ContinuousActions[0] * paddleSpeed * Time.deltaTime;

        // Clamp paddle within boundaries
        float clampedX = Mathf.Clamp(transform.position.x, -paddleBoundary, paddleBoundary);
        transform.position = new Vector3(clampedX, transform.position.y, transform.position.z);
    }

    void GiveReward(float reward)
    {
        AddReward(reward);
        stepReward += reward;
        accumulatedReward += reward;
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        // ball position x
        // ball position z
        // make directions relative
        // ball direction x
        // ball direction z
        
        // ball speed normalized
        // paddle position x normalized
        Vector3 normalizedPosition = new Vector3(ball.transform.localPosition.x / boundary.x,
                                                 ball.transform.localPosition.y,
                                                 ball.transform.localPosition.z / boundary.z);
        sensor.AddObservation(normalizedPosition.x);
        sensor.AddObservation(normalizedPosition.z);
        sensor.AddObservation(ball.direction.normalized.x * Mathf.Sign(transform.localPosition.z));
        sensor.AddObservation(ball.direction.normalized.z * Mathf.Sign(transform.localPosition.z));
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireCube(transform.parent.position, boundary);

        Gizmos.color = new Color(1,0,0,0.25f);
        Gizmos.DrawCube(new Vector3(paddleBoundary, transform.position.y, transform.position.z), transform.lossyScale);
        Gizmos.DrawCube(new Vector3(-paddleBoundary, transform.position.y, transform.position.z), transform.lossyScale);
    }
}
