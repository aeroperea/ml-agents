using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class goal : MonoBehaviour
{
    [SerializeField] 
    Ballz ballz;

    public float middleZ_Pos = 0;
    public pongAgent leftAgent;

    void OnTriggerEnter(Collider collider)
    {
        if(ballz.transform.localPosition.z < middleZ_Pos)
        {
            leftAgent.AgentLostPoint();
        }
        ballz.ResetBall();

    }
}
//oasnjldfnkl