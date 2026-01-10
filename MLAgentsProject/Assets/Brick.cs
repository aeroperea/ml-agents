using UnityEngine;

public class Brick : MonoBehaviour
{
    [SerializeField] LayerMask ballLayer;

    MeshRenderer mr;

    Material mat;

    // private variable to store brick health 
    // public variable store brick max health with a default of 1
    // on start set brick health to max health
    
    // on collision enter when the if is true reduce health by 1
    // if in OnCollisionEnter and if is true and brickHealth is <= 0 then brick dies

    void Start()
    {
        mr = GetComponent<MeshRenderer>();
        mat = mr.material;
    }

    void OnCollisionEnter(Collision other)
    {
        if(((1 << other.gameObject.layer) & ballLayer) != 0)
        {
            // we hit the ball
            gameObject.SetActive(false); // todo communicate to the brick manager l8er
        }
    }
}
