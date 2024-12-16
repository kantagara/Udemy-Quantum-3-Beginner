using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using Quantum;
using UnityEngine;

public class CameraViewContext : MonoBehaviour, IQuantumViewContext
{
    [field:SerializeField] public CinemachineVirtualCamera VirtualCamera { get; private set; }
}
