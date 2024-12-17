using Quantum;
using UnityEngine;

public class PlayerWeapon : MonoBehaviour
{
    [field:SerializeField] public WeaponType WeaponType { get; private set; }
}
