using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class goal : MonoBehaviour
{
    [SerializeField] 
    Ballz Ballz;

    void OnTriggerEnter(Collider collider)
    {
        Ballz.ResetBall();
    }
}
