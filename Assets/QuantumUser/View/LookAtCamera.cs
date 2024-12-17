using System.Collections;
using System.Collections.Generic;
using Quantum;
using UnityEngine;

public class LookAtCamera : QuantumViewComponent<CameraViewContext>
{

    // Update is called once per frame
    void Update()
    {
        transform.LookAt(ViewContext.VirtualCamera.transform);       
    }
}
