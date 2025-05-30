using UnityEngine;
using TMPro;

public class Ballz : MonoBehaviour
{
    public float speed = 5f; // Speed of the ball
    public float paddleForce = 3f;
    public Vector3 direction;
    public float normalizedSpeed;
    public float currentMax {get; private set;} 

    private Rigidbody rb;

    public Gradient colorGradient;
    //public TextMeshProUGUI speedText;
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

        direction.z = Mathf.Max(Mathf.Abs(direction.z), 0.2f) * Mathf.Sign(direction.z);
        // Maintain consistent speed
        if (rb.linearVelocity.magnitude < speed)
        {
            rb.linearVelocity = direction * speed;
        }
        normalizedSpeed = rb.linearVelocity.magnitude / currentMax;
        //speedText.text = $"Speed: {rb.linearVelocity.magnitude}";
        //speedText.color = colorGradient.Evaluate(normalizedSpeed);

        // print($"normalized speed: {normalizedSpeed}  current max {currentMax}"); 
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Paddle"))
        {
            // Reflect away from paddle and apply force
            Vector3 toBall = (transform.localPosition - collision.transform.localPosition).normalized;
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
            //rb.AddForce(direction * paddleForce, ForceMode.Impulse);
        }
    }

    public void ResetBall()
    {
        // Reset position
        transform.localPosition = Vector3.zero;

        // Pick a random direction for the ball
        direction = new Vector3(Random.Range(-1f,1f), 0, Random.Range(-1f, 1f)).normalized;

        // Apply initial force
        rb.linearVelocity = Vector3.zero; // Reset any existing velocity
        rb.AddForce(direction * speed, ForceMode.Impulse);
    }
}
