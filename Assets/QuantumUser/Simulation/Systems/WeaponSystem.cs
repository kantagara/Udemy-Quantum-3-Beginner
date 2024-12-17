using UnityEngine.Scripting;

namespace Quantum {
  using Photon.Deterministic;
  [Preserve]
  public unsafe class WeaponSystem : SystemMainThreadFilter<WeaponSystem.Filter>, ISignalOnComponentAdded<Weapon> {
    public override void Update(Frame f, ref Filter filter)
    {
      var data = f.FindAsset(filter.Weapon->WeaponData);
      data.OnUpdate(f, filter);
    }

    public struct Filter {
      public EntityRef Entity;
      public PlayerLink* Player;
      public Weapon* Weapon;
    }

    public void OnAdded(Frame f, EntityRef entity, Weapon* component)
    {
      var data = f.FindAsset<WeaponBase>(component->WeaponData);
      data.OnInit(f, entity, component);
    }
  }
}
