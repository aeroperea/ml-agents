using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class goal : MonoBehaviour
{
    [SerializeField] 
    Ballz ballz;

    public float middleZ_Pos = 0;
    public pongAgent leftAgent;
    //public pongAgent rightAgent;

    [SerializeField] Transform envParent;

    private void Awake()
    {
        if(leftAgent == null)
        {
            pongAgent[] pongAgents = envParent.GetComponentsInChildren<pongAgent>();
        }
    }

    void OnTriggerEnter(Collider collider)
    {
        if(envParent == null)
        {
            Debug.LogError("Gotta assign the environment parent!");
        }

        if(ballz.transform.localPosition.z < middleZ_Pos)
        {
            leftAgent.AgentLostPoint();
        }
        ballz.ResetBall();
    }
}
//oasnjldfnkl
