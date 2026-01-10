using System.Xml.Serialization;
using UnityEngine;
using static UnityEngine.Rendering.VirtualTexturing.Debugging;

public class paddleballinteraction : MonoBehaviour
{
    [SerializeField] private Rigidbody ballRB;
    private Ball ballScript;

    [SerializeField] private Transform ballStartingPoint;

    public bool launchingState = true;
    public float launchRotationSpeed = 50f;
    public float launchForce = 20f;

    private LineRenderer lineRenderer;
    public float lineLength = 5;

    

    void Start()
    {
        lineRenderer = GetComponent<LineRenderer>();
      

        ballScript = ballRB.GetComponent<Ball>();
        ballScript.setStartPoint(ballStartingPoint.position);

        ballStartingPoint.eulerAngles = Vector3.right * -90;

        InitLaunchingState();
    }

    public void InitLaunchingState()
    {
        launchingState = true;
      //  pController.enabled = false;

        lineRenderer.enabled = true;

   


        ballRB.linearVelocity = Vector3.zero;
        ballRB.angularVelocity = Vector3.zero;

        ballRB.transform.position = ballStartingPoint.position;
        ballRB.transform.eulerAngles = ballStartingPoint.eulerAngles;


        ballRB.constraints = RigidbodyConstraints.FreezeAll;
    }

    void Update()
    {
        if (!launchingState) return;

        lineRenderer.SetPosition(0, ballStartingPoint.position);
        lineRenderer.SetPosition(1, ballStartingPoint.position + ballRB.transform.forward * lineLength);

        float horizontalInput = Input.GetAxis("Horizontal");
        float verticalInput = Input.GetAxis("Vertical");

        ballRB.transform.Rotate(Vector3.right * verticalInput * launchRotationSpeed * Time.deltaTime, Space.World);
        ballRB.transform.Rotate(Vector3.forward * -horizontalInput * launchRotationSpeed * Time.deltaTime, Space.World);

        if (Input.GetKeyDown(KeyCode.Space))
            LaunchBall();
    }

    private void LaunchBall()
    {
        launchingState = false;
        lineRenderer.enabled = false;

     //pController.enabled = true;


        ballRB.constraints = RigidbodyConstraints.FreezePositionZ | RigidbodyConstraints.FreezeRotation;

        ballScript.direction = ballRB.transform.forward;

        //ballRB.AddForce(ballRB.transform.forward * launchForce, ForceMode.Impulse);  ball handles it's own volocity now.
    }
    public Transform getballStartingpoint()
    {
        return ballStartingPoint;
    }
}
