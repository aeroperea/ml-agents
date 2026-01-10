using UnityEngine;

public class Brick : MonoBehaviour
{
    [SerializeField] LayerMask ballLayer;

    

    void OnCollisionEnter(Collision other)
    {
        if(((1 << other.gameObject.layer) & ballLayer) != 0)
        {
            // we hit the ball
            gameObject.SetActive(false); // todo communicate to the brick manager l8er
        }
    }
}
