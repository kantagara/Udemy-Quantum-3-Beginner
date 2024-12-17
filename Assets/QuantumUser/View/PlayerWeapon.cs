using Quantum;
using UnityEngine;
using UnityEngine.Animations.Rigging;

public class PlayerWeapon : MonoBehaviour
{
    [field:SerializeField] public WeaponType WeaponType { get; private set; }
    [field:SerializeField] public Rig Rig { get; private set; }
}
