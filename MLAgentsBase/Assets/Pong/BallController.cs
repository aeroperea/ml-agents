using UnityEngine;

public class Ball : MonoBehaviour
{
    public float speed = 5f; // Speed of the ball
    public float paddleForce = 3f;
    private Vector3 direction; // Movement direction

    Rigidbody rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void Start()
    {
        // Launch the ball in a random direction at start
        ResetBall();
    }

    private void FixedUpdate()
    {
        // Move the ball
        Vector3 currentVel = rb.linearVelocity;
        currentVel.x = Mathf.Max(Mathf.Abs(currentVel.x), speed) * Mathf.Sign(currentVel.x);
        currentVel.x = Mathf.Max(Mathf.Abs(currentVel.z), speed) * Mathf.Sign(currentVel.z);
        rb.linearVelocity = new Vector3(currentVel.x, 0, currentVel.z);
        // transform.position += (Vector3)(direction * speed * Time.deltaTime);
    }

    private void OnCollisionEnter(Collision collision)
    {
        
        // print(collision.gameObject.tag);

        // Bounce off paddles
        if (collision.gameObject.CompareTag("Paddle"))
        {
            // direction.z = -direction.z; // Reverse horizontal direction
            Vector3 toPaddle = collision.transform.position - transform.position;
            toPaddle = toPaddle.normalized;
            rb.linearVelocity(-toPaddle);
            rb.AddForce(-toPaddle * paddleForce, ForceMode.Impulse);
        }
    

        // Bounce off top and bottom walls
        if (collision.gameObject.CompareTag("Wall"))
        {
            rb.velocity.x = -rb.velocity.x
            // rb.velocity= new Vector3()
        }
    }

    public void ResetBall()
    {
        // Randomize initial direction (left or right)
        float randomZ = Random.Range(-0.5f, 0.5f);
        direction = new Vector3(0, 0, Random.value < 0f ? -1 : 1).normalized;
    }
    

}