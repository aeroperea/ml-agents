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

    [SerializeField] Ballz ballz;
    

    public Vector3 boundary = new Vector3(10,0,14);
    public float paddleBoundary = 4.5f;
    public float paddleSpeed = 10f;

    public bool isPlayer1;
    public bool controlsBall;

    string inputAxis = "Vertical";

    float skibidiToilet = 5;
    float flusher = 1;
    //float plunger = -1;
    float cameraMan = -5;

    float timer = 0;
    public float timerMax = 5;

    private Vector3 startingPosition;

    public  override void Initialize() 
    {
        base.Initialize();
        startingPosition = transform.localPosition;
    }

    public override void OnEpisodeBegin()
    {
        timer = timerMax;

        transform.localPosition = startingPosition; // new

        /*string[] connectedControllers = Input.GetJoystickNames();
        foreach (string jName in connectedControllers)
        {
            print(jName);
        }


        if (!isPlayer1)
        {
            inputAxis = "Mouse Y";
        }*/
    }

    private void FixedUpdate()
    {
        // add reward for surviving timer max amnt of seconds
        timer -= Time.fixedDeltaTime;
        if(timer < 0)
        {
            AddReward(skibidiToilet);
            timer += timerMax;
        }
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        float moveInput = 0;
        if(isPlayer1)
        {
            print(Input.GetAxis(inputAxis));
            moveInput = -Input.GetAxis(inputAxis);
        }
        else
        {
            moveInput += Input.GetKey(KeyCode.W) ? 1 : 0;
            moveInput -= Input.GetKey(KeyCode.S) ? 1 : 0;
        }
        var continuousActions = actionsOut.ContinuousActions;
        continuousActions[0] = moveInput;
    }

    public override void OnActionReceived(ActionBuffers actionBuffers)
    {
       
        // Move the paddle
        transform.localPosition += Vector3.right * actionBuffers.ContinuousActions[0] * paddleSpeed * Time.deltaTime;

        // Clamp paddle within boundaries
        float clampedX = Mathf.Clamp(transform.localPosition.x, -paddleBoundary, paddleBoundary);
        transform.localPosition = new Vector3(clampedX, transform.localPosition.y, transform.localPosition.z);  
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
        Vector3 normalizedPosition = new Vector3(ballz.transform.localPosition.x / boundary.x,
                                                 ballz.transform.localPosition.y,
                                                 ballz.transform.localPosition.z / boundary.z);
        sensor.AddObservation(normalizedPosition.x);
        sensor.AddObservation(normalizedPosition.z);
        sensor.AddObservation(ballz.direction.normalized.x * Mathf.Sign(transform.localPosition.z));
        sensor.AddObservation(ballz.direction.normalized.z * Mathf.Sign(transform.localPosition.z));
    }

    private void OnCollisionEnter(Collision collision)
    {
        if(collision.gameObject.CompareTag("ball"))
        {
            // we hit the ball
            AddReward(flusher);
        }
    }

    //new function
    public void AgentWonPoint()
    {
        AddReward(skibidiToilet);
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
