using UnityEngine;

public class ExplosionBehaviour : MonoBehaviour
{
    float aliveTime = 3.5f;

    float scaleModifier = 0.1f;
    float scaleIncrease = 0.1f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        transform.localScale = Vector3.one * scaleModifier;
        scaleModifier += scaleIncrease;

        aliveTime -= Time.deltaTime;

        if(aliveTime <= 0)
        {
            Destroy(this.gameObject);
        }
            
    }
}
