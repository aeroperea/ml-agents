using UnityEngine;

public class PhysicsPlayer : MonoBehaviour
{
    Rigidbody rb;
    float moveSide;
    float jumpInput;
    float moveFwd;

    public float moveSpeed;
    public float jumpForce;

    public bool gravityOn = true;
    public float explosionRadius = 20;
    public float explosionForce = 10000;

    public GameObject explosionPrefab;
    public LayerMask lm;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        moveFwd = Input.GetAxis("Vertical") * moveSpeed;
        moveSide = Input.GetAxis("Horizontal") * moveSpeed;
        jumpInput = Input.GetKey(KeyCode.Space) ? jumpForce : Input.GetKey(KeyCode.LeftControl) ? -jumpForce : 0;

        if(Input.GetKeyDown(KeyCode.X))
        {
            gravityOn = !gravityOn;
            rb.useGravity = gravityOn;
        }

        if(Input.GetKeyDown(KeyCode.E))
        {
            Explosion();
        }
    }

    private void FixedUpdate()
    {
        Vector3 moveVec = new Vector3(moveSide, jumpInput, moveFwd);
        rb.AddForce(moveVec * rb.mass);
    }

    private void Explosion()
    {
        Collider[] hitCols = Physics.OverlapSphere(transform.position, explosionRadius, lm);

        foreach(var col in hitCols)
        {
            Rigidbody hitRb = col.transform.GetComponent<Rigidbody>();
            if(hitCols != null)
            {
                hitRb.AddExplosionForce(explosionForce, transform.position, explosionRadius);
            }
        }

        Transform explosionEffect = Instantiate(explosionPrefab, transform.position, Random.rotation).transform;
        explosionEffect.localScale = Vector3.zero;
        //explosionEffect.localScale = Vector3.one * Random.value;
    }
}
