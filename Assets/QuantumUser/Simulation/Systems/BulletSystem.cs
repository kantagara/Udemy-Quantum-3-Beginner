using UnityEngine.Scripting;

namespace Quantum {
  using Photon.Deterministic;

  [Preserve]
  public unsafe class BulletSystem : SystemMainThreadFilter<BulletSystem.Filter>, ISignalCreateBullet {
    public override void Update(Frame f, ref Filter filter) {
    }

    public struct Filter {
      public EntityRef Entity;
      public Bullet* Bullet;
    }

    public void CreateBullet(Frame f, EntityRef owner, WeaponData weaponData)
    {
      var bulletData = weaponData.BulletData;
      var bulletEntity = f.Create(bulletData.Bullet);
      var bulletTransform = f.Unsafe.GetPointer<Transform2D>(bulletEntity);
      var ownersTransform = f.Get<Transform2D>(owner);

      bulletTransform->Position = ownersTransform.Position + weaponData.Offset.XZ.Rotate(ownersTransform.Rotation);
      bulletTransform->Rotation = ownersTransform.Rotation;
      var bullet = f.Unsafe.GetPointer<Bullet>(bulletEntity);
      bullet->Speed = bulletData.Speed;
      bullet->Damage = bulletData.Damage;
      bullet->Owner = owner;
      bullet->Time = bulletData.Duration;
      bullet->Direction = ownersTransform.Up;
    }
  }
}
