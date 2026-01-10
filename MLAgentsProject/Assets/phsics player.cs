

//using UnityEngine;

//public class Physicplayer : MonoBehaviour
//{
//    Rigidbody rb;
//    float moveSide;
//    float moveFwd;
//    float jumpInput;
//    public float moveSpeed = 20f;
//    public float jumpForce = 5f;

//    public bool gravityOn = true;

//    void Awake()
//    {
//        rb = GetComponent<Rigidbody>();
//    }

//    void Update()
//    {
//        return;
//        moveFwd = Input.GetAxis("Vertical") * moveSpeed;
//        moveSide = Input.GetAxis("Horizontal") * moveSpeed;
//        jumpInput = Input.GetKey(KeyCode.Space) ? jumpForce : 0;

//        if (Input.GetKeyDown(KeyCode.X))
//        {
//            gravityOn = !gravityOn;
//            rb.useGravity = gravityOn;
//        }
//    }

//    private void FixedUpdate()
//    {
//        Vector3 moveVec = new Vector3(moveSide, jumpInput, moveFwd);
//        rb.linearVelocity = moveVec;
//    }
//}
