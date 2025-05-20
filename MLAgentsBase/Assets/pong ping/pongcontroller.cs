using UnityEngine;

public class PlayerPaddle : MonoBehaviour
{
    public float speed = 10f; // Speed of the paddle
    public float boundary = 4.5f; // Limit paddle movement

    public bool useArrow;

    KeyCode upKey, downKey;

    public bool isPlayer1;

    string inputAxis = "Vertical";

    // inputAxis = "Vertical"
    // inputAxis = "Mouse Y"

    void Start()
    {
        string[] connectedControllers = Input.GetJoystickNames();
        foreach(string jName in connectedControllers)
        {
            print(jName);    
        }
        
    
        if(!isPlayer1)
        {
            inputAxis = "Mouse Y";
        }
        // if(useArrow)
        // {
        //     upKey = KeyCode.UpArrow;
        //     downKey = KeyCode.DownArrow;
        // }
        // else
        // {
        //     upKey = KeyCode.W;
        //     downKey = KeyCode.S;
        // }
    }

    private void Update()
    {
        
        // Get vertical input (-1 for "S", 1 for "W")
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
        // moveInput += Input.GetKey(upKey) ? 1 : 0;
        // moveInput -= Input.GetKey(downKey) ? 1 : 0;
        // Move the paddle
        transform.position += Vector3.right * moveInput * speed * Time.deltaTime;

        // Clamp paddle within boundaries
        float clampedX = Mathf.Clamp(transform.position.x, -boundary, boundary);
        transform.position = new Vector3(clampedX, transform.position.y, transform.position.z);
    }
}
