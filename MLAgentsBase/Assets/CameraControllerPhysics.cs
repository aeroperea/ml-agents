using UnityEngine;

public class CameraControllerPhysics : MonoBehaviour
{
    Vector3 cameraOffsetTarget = new Vector3(0, 2, -4);

    public Transform targetTransform;

    public float lerpSpeed = 7;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void LateUpdate()
    {
        Vector3 targetPos = targetTransform.position + cameraOffsetTarget;
        Vector3 lerpedPosition = Vector3.Lerp(transform.position, targetPos, lerpSpeed * Time.deltaTime);
        transform.position = lerpedPosition;
    }
}
