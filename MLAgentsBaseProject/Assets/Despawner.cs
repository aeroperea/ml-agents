using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Despawner : MonoBehaviour
{
    float despawnTime = 5;
    // Start is called before the first frame update
    void Start()
    {
        Destroy(this.gameObject, despawnTime);
    }
}
