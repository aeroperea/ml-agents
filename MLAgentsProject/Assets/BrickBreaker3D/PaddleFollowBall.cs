using UnityEngine;

public class PaddleFollowBall : MonoBehaviour
{
    [SerializeField] private Transform ball;

void Update()
    {
        Vector3 pos = transform.position;
        pos.x = ball.position.x;
        pos.z = ball.position.z;
        pos.y = transform.position.y;
        transform.position = pos;
    }

}

