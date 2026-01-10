using UnityEngine;

public class camracontroler : MonoBehaviour
{
    public Vector3 camraOffsetTarget = new Vector3(0, 2, -4);

    [SerializeField] Transform cameraPivot;
    public Transform paddleT;
    [SerializeField] private Transform ball;

    public float camraDistance = 40;
    public float distanceRange = 30;
    public float lerpSpeed = 7f;
    public float distanceSense = 100f;

    private float maxDistance;
    private float minDistance;

    public Vector3 AvergePosition;

    void Start()
    {
      
        camraOffsetTarget = transform.localPosition.normalized;

    
        maxDistance = camraDistance + distanceRange;
        minDistance = camraDistance - distanceRange;
    }

    void LateUpdate()
    {
     
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        camraDistance += scroll * distanceSense;
        camraDistance = Mathf.Clamp(camraDistance, minDistance, maxDistance);

     
        AvergePosition = (ball.position + paddleT.position) * 0.5f;
        cameraPivot.position = AvergePosition;

        Vector3 targetPosition = camraOffsetTarget * camraDistance;
        transform.localPosition = Vector3.Lerp(
            transform.localPosition,
            targetPosition,
            lerpSpeed * Time.deltaTime
        );
    }
}

