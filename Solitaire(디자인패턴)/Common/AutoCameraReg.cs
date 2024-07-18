using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutoCameraReg : MonoBehaviour
{ 
    // Start is called before the first frame update
    void Start()
    {
            GetComponent<Canvas>().worldCamera = GameObject.FindWithTag("MainCamera").GetComponent<Camera>();  
    }

}
