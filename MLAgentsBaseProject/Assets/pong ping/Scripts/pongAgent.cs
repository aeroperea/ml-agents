using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using Random = UnityEngine.Random;

public class pongAgent : Agent
{
    //variables
    float stepReward = 0; //reward for the current step
    float accumulatedReward = 0; //reward for episode

    float paddleHeight;
    float halfPaddleWidth;

    [SerializeField] Ballz ballz;
    

    public Vector3 boundary = new Vector3(10,0,14);
    public float paddleBoundary = 4.5f;
    public float paddleSpeed = 10f;
    private Transform ball;

    public bool isPlayer1;
    public bool controlsBall;
    public bool paddleHitBall;

    string inputAxis = "Vertical";

    const float hitReward = 1.25f;
    const float missPenalty = -2f;
    const float livingCost = -0.001f;
    const float edgeMultiplier = 0.25f;

    float timer = 0;
    public float timerMax = 5; 
    private Vector3 startingPosition;
    private float ballDirMultiplier = 0.75f;

    public override void Initialize()
    {
        paddleHeight = transform.lossyScale.x;
        halfPaddleWidth = paddleHeight * 0.5f;
        base.Initialize();
        startingPosition = transform.localPosition;
        ball = ballz.transform;
    }
    public override void OnEpisodeBegin()
    {
        timer = timerMax;

        transform.localPosition = startingPosition;
        // string[] connectedControllers = Input.GetJoystickNames();
        // foreach(string jName in connectedControllers)
        // {
        //     print(jName);    
        // }
        
    
        // if(!isPlayer1)
        // {
        //      putAxis = "Mouse Y";
        // }
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        float moveInput = 0;
        moveInput = -Input.GetAxis(inputAxis);
        //if (isPlayer1)
        //{
        //    //print(Input.GetAxis(inputAxis));
        //    moveInput = -Input.GetAxis(inputAxis);
        //}
        //else
        //{
        //    //moveInput -= Input.GetKey(KeyCode.W) ? 1 : 0;
        //    //moveInput += Input.GetKey(KeyCode.S) ? 1 : 0;
        //}
        var continuousActions = actionsOut.ContinuousActions;
        continuousActions[0] = moveInput;
    }

    public override void OnActionReceived(ActionBuffers actionBuffers)
    {
        // small living cost each step
        AddReward(livingCost);

        // move the paddle along x based on the network output
        transform.localPosition += Vector3.right * actionBuffers.ContinuousActions[0]
                                   * paddleSpeed * Time.deltaTime;

        // clamp paddle within horizontal bounds
        float clampedX = Mathf.Clamp(transform.localPosition.x, -paddleBoundary, paddleBoundary);
        transform.localPosition = new Vector3(clampedX, transform.localPosition.y, transform.localPosition.z);

        // no distance shaping, no movement penalty, no timer bonus
    }

    void GiveReward(float reward)
    {
        AddReward(reward);
        stepReward += reward;
        accumulatedReward += reward;
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        // ballz position x
        // ballz position z
        // make directions relative
        // ballz direction x
        // ballz direction z

        // ballz speed normalized
        // paddle position x normalized
        float normalizedBallPosX = ballz.transform.localPosition.x / boundary.x;
        float normalizedBallPosZ = ballz.transform.localPosition.z / boundary.z;
        sensor.AddObservation(normalizedBallPosX);
        sensor.AddObservation(normalizedBallPosZ);
        sensor.AddObservation(ballz.direction.normalized.x * Mathf.Sign(transform.localPosition.z));
        sensor.AddObservation(ballz.direction.normalized.z * Mathf.Sign(transform.localPosition.z));

        float normalizedPaddlePosX = transform.localPosition.x / (boundary.x - paddleHeight);
        sensor.AddObservation(normalizedPaddlePosX);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("ball"))
        {
            // compute how close to the edge of the paddle the hit occurred
            Vector3 toBall = ball.localPosition - transform.localPosition;
            float edgeFactor = Mathf.Clamp01(Mathf.Abs(toBall.x) / halfPaddleWidth);

            toBall = toBall.normalized;
            ballz.AddForceAndSetNewDirection(toBall);
            float ballDirDot = Vector3.Dot(toBall, transform.forward);
            float ballDirDotFactor = ballDirDot > 0.5 ? 1 : ballDirDot;

            // base hitReward plus extra for edge hits
            float totalHit = hitReward + edgeMultiplier * edgeFactor + ballDirMultiplier * ballDirDotFactor;
            AddReward(totalHit);
        }
    }

    public void AgentWonPoint()
    {
        AddReward(hitReward);
        EndEpisode();
    }


    public void AgentLostPoint()
    {
       AddReward(missPenalty);
       EndEpisode();
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireCube(transform.parent.position, boundary);
        Gizmos.color = new Color(1,0,0,0.25f);
        Gizmos.DrawCube(new Vector3(paddleBoundary, transform.localPosition.y, transform.localPosition.z), transform.lossyScale);
        Gizmos.DrawCube(new Vector3(-paddleBoundary, transform.localPosition.y, transform.localPosition.z), transform.lossyScale);
    }
    
}

