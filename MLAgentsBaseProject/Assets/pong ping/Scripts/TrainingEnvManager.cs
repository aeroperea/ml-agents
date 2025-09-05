using Unity.Entities;
using UnityEngine;

public class TrainingEnvManager : MonoBehaviour
{

    public pongAgent positiveSideAgent;
    public pongAgent negativeSideAgent;

    public Ballz ball;
    
    // Update is called once per frame
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
}
