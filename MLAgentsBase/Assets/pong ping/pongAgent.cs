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
    float lastX;

    [SerializeField] Ballz ballz;
    

    public Vector3 boundary = new Vector3(10,0,14);
    public float paddleBoundary = 4.5f;
    public float paddleSpeed = 10f;
    private Transform ball;

    public bool isPlayer1;
    public bool controlsBall;
    public bool paddleHitBall;

    string inputAxis = "Vertical";

    float skibidiToilet = 3;
    float flusher = 1;
    // float plunger = -1;
    float cameraMan = -3;

    float timer = 0;
    public float timerMax = 5; 
    private Vector3 startingPosition;

    public override void Initialize()
    {
        paddleHeight = transform.lossyScale.x;
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

    private void FixedUpdate()
    {
        timer -= Time.fixedDeltaTime;
        if(timer < 0)
        {
            AddReward (skibidiToilet);
            timer += timerMax;
        }
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

        // === 1. Distance Shaping Reward ===
        float distanceY = Mathf.Abs(ball.position.y - transform.position.y);
        AddReward(-distanceY * 0.25f); // Mild penalty to encourage positioning

        // === 2. Positive Reward: Paddle Hits Ball ===
        if (paddleHitBall)
        {
            float relativeHitY = ball.position.y - transform.position.y;
            float halfPaddleHeight = paddleHeight / 2.0f;

            // Compute how close to the edge the hit occurred [0=center, 1=edge]
            float edgeFactor = Mathf.Abs(relativeHitY) / halfPaddleHeight;
            float edgeBonus = Mathf.Clamp01(edgeFactor); // Safety clamp

            // Reward: +1 base + up to +1 for edge hit
            AddReward(edgeBonus);
            paddleHitBall = false;
        }

        // === 3. Negative Reward: Ball Missed Paddle ===
  
        // === 4. Optional: Penalize Unnecessary Movement ===
     
        // Move the paddle
        transform.localPosition += Vector3.right * actionBuffers.ContinuousActions[0] * paddleSpeed * Time.deltaTime;

        // Clamp paddle within boundaries
        float clampedX = Mathf.Clamp(transform.localPosition.x, -paddleBoundary, paddleBoundary);
        transform.localPosition = new Vector3(clampedX, transform.localPosition.y, transform.localPosition.z);
        float velocity = lastX - transform.localPosition.x;
        float movementReward = Mathf.Abs(velocity*Time.deltaTime);
        AddReward(velocity < 1 ? -movementReward : movementReward); // Light penalty for jitter
        lastX = transform.localPosition.x;
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
        if(collision.gameObject.CompareTag("ball"))
        {
            AddReward(skibidiToilet);
        }
    }

    public void AgentWonPoint()
    {
        AddReward(flusher);
        EndEpisode();
    }


    public void AgentLostPoint()
    {
       AddReward(cameraMan);
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

