using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutomaticSpawner : MonoBehaviour
{
    public Transform target;
    public GameObject thing2Throw;
    public float throwForce = 100;
    public float throwPosRadius = 10;
    public float throwInterval = 1;
    public float intervalRandomness = 0.3f;

    float nextThrowTime = 0;

    // Update is called once per frame
    void Update()
    {
        if (Time.time > nextThrowTime)
        {
            ThrowObject();
            nextThrowTime = Time.time + throwInterval + Random.Range(-intervalRandomness, intervalRandomness);
        }

        // Time.timeScale = Input.GetKey(KeyCode.Space) ? 0.25f : 1;
    }

    void ThrowObject()
    {
        Vector2 randomCirclePos = Random.insideUnitCircle * throwPosRadius;
        Vector3 randomThrowPosition = target.position + new Vector3(randomCirclePos.x, throwPosRadius, randomCirclePos.y);
        Vector3 directionToTarget = target.position - randomThrowPosition;
        GameObject thrownObject = Instantiate(thing2Throw, randomThrowPosition, Quaternion.Euler(directionToTarget));
        Rigidbody rb = thrownObject.GetComponent<Rigidbody>();

        if (rb != null)
        {
            rb.AddForce(directionToTarget * throwForce, ForceMode.Impulse);
        }

    }
}
