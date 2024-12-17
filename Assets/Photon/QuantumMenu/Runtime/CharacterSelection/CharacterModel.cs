using System.Collections;
using System.Collections.Generic;
using Quantum;
using UnityEngine;

[CreateAssetMenu(fileName = "Character", menuName = "Character Model")]
public class CharacterModel : ScriptableObject
{
  [field:SerializeField] public Sprite CharacterImage { get; private set; }
  [field:SerializeField] public string CharacterName { get; private set; }
  [field:SerializeField] public EntityPrototype EntityPrototype { get; private set; }
}
