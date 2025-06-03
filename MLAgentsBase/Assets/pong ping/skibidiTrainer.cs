using UnityEngine;

public class skibidiTrainer : MonoBehaviour
{
    
    public pongAgent positiveSideAgent;
    public pongAgent negativeSideAgent;

    public Ballz ball;

    public float goalZExtents = 21;


     void Update()
     {
        if(ball.transform.localPosition.z < negativeSideAgent.transform.localPosition.z)
        {
            ProcessScoredGoal(positiveSideAgent, negativeSideAgent);
        }

        if (ball.transform.localPosition.z > positiveSideAgent.transform.localPosition.z)
        {
            ProcessScoredGoal(negativeSideAgent, positiveSideAgent);
        }
     }

     private void ProcessScoredGoal(pongAgent agentWonPoint, pongAgent agentLostPoint)
     {
        if (agentWonPoint != null) agentWonPoint.AgentWonPoint();
        agentLostPoint.AgentLostPoint();

        ball.ResetBall();
     }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green * new Vector4(1, 1, 1, 0.44f);
        Vector3 goalGizmoSize = new Vector3(30, 10, 1);
        Gizmos.DrawCube(transform.position + new Vector3(0, 0, goalZExtents), goalGizmoSize);
        //Gizmos.color = Color.blue * new Vector4(1, 1, 1, 0.5f);
        Gizmos.DrawCube(transform.position + new Vector3(0, 0, -goalZExtents), goalGizmoSize);
    }
}
