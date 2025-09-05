using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectThrower : MonoBehaviour
{
    public GameObject thing;

    Transform camT;

    public float throwForce = 20; 

    // Start is called before the first frame update
    void Start()
    {
        camT = Camera.main.transform;
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Mouse0))
        {
            ThrowThing();
        }
    }

    void ThrowThing()
    {
        Vector3 mousePos = Input.mousePosition;

        Ray ray = Camera.main.ScreenPointToRay(mousePos);

        Vector3 rayOrigin = ray.origin;
        Vector3 rayDirection = ray.direction;

        GameObject spawnedObj = Instantiate(thing, rayOrigin, Quaternion.Euler(rayDirection));
        Rigidbody rb = spawnedObj.GetComponent<Rigidbody>();

        if(rb != null)
        {
            rb.AddForce(rayDirection * throwForce, ForceMode.Impulse);
        }
        
    }
}
