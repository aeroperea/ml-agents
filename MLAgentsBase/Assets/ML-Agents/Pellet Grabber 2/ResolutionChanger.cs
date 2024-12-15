using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResolutionChanger : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        Screen.SetResolution(300, 300, false);  
    }
}
