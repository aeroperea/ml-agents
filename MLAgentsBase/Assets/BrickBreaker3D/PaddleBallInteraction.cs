using System;
using UnityEngine;

public class PaddleBallInteraction : MonoBehaviour
{
    [SerializeField] private Rigidbody ballRB;

    [SerializeField] private Transform ballStartingPoint;
    public float launchSpeed;
    public bool launchingState;
    public float launchRotationSpeed = 5;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        launchingState = true;

        ballStartingPoint.eulerAngles = Vector3.right * -90;
    }

    // Update is called once per frame
    void Update()
    {
        if (!launchingState) return;

        ballRB.linearVelocity = Vector3.zero;
        ballRB.angularVelocity = Vector3.zero;
        ballRB.transform.position = ballStartingPoint.position;

        float horizontalInput = Input.GetAxis("Horizontal");
        float verticalInput = Input.GetAxis("Vertical");

        ballStartingPoint.transform.Rotate(Vector3.right * verticalInput * launchRotationSpeed * Time.deltaTime, Space.World);
        ballStartingPoint.transform.Rotate(Vector3.forward * horizontalInput * launchRotationSpeed * Time.deltaTime, Space.World);

        if (Input.GetKeyDown(KeyCode.Space))
        {
            LaunchBall();
        }
    }

    private void LaunchBall()
    {
        
    }
}
