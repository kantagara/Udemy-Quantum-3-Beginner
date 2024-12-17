using UnityEngine.Scripting;

namespace Quantum {
  using Photon.Deterministic;

  [Preserve]
  public unsafe class BulletSystem : SystemMainThreadFilter<BulletSystem.Filter>, ISignalCreateBullet {
    public override void Update(Frame f, ref Filter filter)
    {
      var nextPosition = filter.Bullet->Direction * filter.Bullet->Speed * f.DeltaTime;

      if (CheckForCollisions(f, filter, nextPosition, out var entityHit))
      {
        if (f.Unsafe.TryGetPointer<Damageable>(entityHit, out var damageable))
        {
          f.Signals.DamageableHit(entityHit, filter.Bullet->Owner, filter.Bullet->Damage, damageable);
        }
        f.Destroy(filter.Entity);
        return;
      }

      CheckBulletForTimeExpiration(f, filter);
      
      
      filter.Transform->Position += nextPosition;
    }

    private void CheckBulletForTimeExpiration(Frame frame, Filter filter)
    {
      filter.Bullet->Time -= frame.DeltaTime;
      if (filter.Bullet->Time <= 0)
      {
        frame.Destroy(filter.Entity);
      }
    }

    private bool CheckForCollisions(Frame frame, Filter filter, FPVector2 nextPosition, out EntityRef entityHit)
    {
      entityHit = EntityRef.None;

      var owner = filter.Bullet->Owner;
      var bulletTransform = frame.Get<Transform2D>(filter.Entity);
      var collisions = frame.Physics2D.LinecastAll(bulletTransform.Position, bulletTransform.Position + nextPosition,
        int.MaxValue, QueryOptions.HitAll & ~QueryOptions.HitTriggers);

      for (var i = 0; i < collisions.Count; i++)
      {
        var collision = collisions[i];
        if(collision.Entity == filter.Entity || collision.Entity == owner)
          continue;
        entityHit = collision.Entity;
        return true;
      }
      
      return false;
    }

    public struct Filter {
      public EntityRef Entity;
      public Bullet* Bullet;
      public Transform2D* Transform;
    }


    public void CreateBullet(Frame f, EntityRef owner, FiringWeapon weaponData)
    {
      var bulletData = weaponData.BulletData;
      bulletData.CreateBullet(f, weaponData, owner);
    }
  }
}
