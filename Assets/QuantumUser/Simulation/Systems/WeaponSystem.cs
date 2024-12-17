using UnityEngine.Scripting;

namespace Quantum {
  using Photon.Deterministic;
  [Preserve]
  public unsafe class WeaponSystem : SystemMainThreadFilter<WeaponSystem.Filter> {
    public override void Update(Frame f, ref Filter filter)
    {
      if (filter.Weapon->CooldownTime >= FP._0)
      {
        filter.Weapon->CooldownTime -= f.DeltaTime;
        return;
      }

      var input = f.GetPlayerInput(filter.Player->Player);
      if (input->Fire.WasPressed)
      {
        var weaponData = f.FindAsset(filter.Weapon->WeaponData);
        filter.Weapon->CooldownTime = weaponData.Cooldown;
        f.Signals.CreateBullet(filter.Entity, weaponData);
      }
    }

    public struct Filter {
      public EntityRef Entity;
      public PlayerLink* Player;
      public Weapon* Weapon;
    }
  }
}
