using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class Ballz : MonoBehaviour
{
    public float speed = 5f; // Speed of the ball
    public float paddleForce = 3f;
    public Vector3 direction {get; private set;} // Current movement direction
    public float normalizedSpeed;
    public float currentMax {get; private set;}

    private Rigidbody rb;

    public Gradient colorGradient;
    public TextMeshProUGUI speedText;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void Start()
    {
        currentMax = 30;
        ResetBall();
    }

    private void FixedUpdate()
    {
        // print(rb.linearVelocity);

        // currentMax = rb.linearVelocity.magnitude > currentMax ? rb.linearVelocity.magnitude : currentMax;

        direction.z = Mathf.Max(Mathf.Abs(direction.z), 0.2f) * Mathf.Sign(direction.z);
        // Maintain consistent speed
        if (rb.linearVelocity.magnitude < speed)
        {
            rb.linearVelocity = direction * speed;
        }
        normalizedSpeed = rb.linearVelocity.magnitude / currentMax;
        
        print($"normalized speed: {normalizedSpeed}  current max {currentMax}");
        speedText.text = $"Speed: {rb.linearVelocity.magnitude}";
        speedText.color = colorGradient.Evaluate(normalizedSpeed);
    }

    void OnCollisionEnter(Collision collision)
    {
        // print("Collision Detected!");

        if (collision.gameObject.CompareTag("Paddle"))
        {
            // Reflect away from paddle and apply force
            Vector3 toBall = (transform.position - collision.transform.position).normalized;
            direction = toBall;
            rb.linearVelocity = direction * speed;
            rb.AddForce(direction * paddleForce, ForceMode.Impulse);
        }
        else
        {
            // Reflect based on surface normal
            ContactPoint contact = collision.contacts[0];
            direction = Vector3.Reflect(direction, contact.normal);
            rb.linearVelocity = direction * paddleForce;
            // rb.AddForce(direction * paddleForce, ForceMode.Impulse);
        }
    }

    public void ResetBall()
    {
        // Reset position
        transform.position = Vector3.zero;

        // Pick a random direction for the ball
        direction = new Vector3(0, 0, Random.Range(-1f, 1f)).normalized;

        // Apply initial force
        rb.linearVelocity = Vector3.zero; // Reset any existing velocity
        rb.AddForce(direction * speed, ForceMode.Impulse);
    }
}
