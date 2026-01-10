using UnityEngine;
using TMPro;

public class Ball : MonoBehaviour
{
    public float speed = 12f;
    public Vector3 direction;

    private Rigidbody rb;
    private Vector3 ballStartingPoint;

    public TextMeshProUGUI speedText;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void Start()
    {
        ResetBall();
    }

    private void FixedUpdate()
    {
        if (rb.constraints == RigidbodyConstraints.FreezeAll)
            return;

        direction = direction.normalized;

        // Maintain constant speed
        rb.linearVelocity = direction * speed;

        if (speedText != null)
            speedText.text = rb.linearVelocity.magnitude.ToString("F2");
    }

    private void OnCollisionEnter(Collision collision)
    {
        Vector3 normal = collision.contacts[0].normal;

        direction = Vector3.Reflect(direction, normal);
        direction.Normalize();
    }

    public void ResetBall()
    {
        transform.position = ballStartingPoint;

        direction = new Vector3(
            Random.Range(-1f, 1f),
            Random.Range(0.3f, 1f),
            0f
        ).normalized;
    }

    public void setStartPoint(Vector3 newStartPoint)
    {
        ballStartingPoint = newStartPoint;
    }
}
