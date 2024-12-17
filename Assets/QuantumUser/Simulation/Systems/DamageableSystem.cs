using UnityEngine.Scripting;

namespace Quantum {
  using Photon.Deterministic;

  [Preserve]
  public unsafe class DamageableSystem : SystemMainThreadFilter<DamageableSystem.Filter>, ISignalOnComponentAdded<Damageable>, ISignalDamageableHit, ISignalDamageableHealthRestored {
    public struct Filter
    {
      public EntityRef Entity;
      public Damageable* Damageable;
    }

    public override void Update(Frame f, ref Filter filter)
    {
      if (!f.TryGet<PlayerLink>(filter.Entity, out _))
        return;
      var shrinkingCircle = f.GetSingleton<ShrinkingCircle>();
      if (CheckIfPlayerIsOutsideCircle(f, filter, shrinkingCircle))
      {
        var damageableAsset = f.FindAsset<DamageableBase>(filter.Damageable->DamageableData);
        var shrinkingCircleConfig = f.FindAsset(shrinkingCircle.ShrinkingCircleConfig);
        damageableAsset.DamageableHit(f, filter.Entity, filter.Entity, shrinkingCircleConfig.DamageDealingPerSecond * f.DeltaTime, filter.Damageable);
      }
    }

    private bool CheckIfPlayerIsOutsideCircle(Frame f, Filter filter, ShrinkingCircle shrinkingCircle)
    {
      var transform = f.Get<Transform2D>(filter.Entity);
      return FPVector2.Distance(transform.Position, shrinkingCircle.Position) >= shrinkingCircle.CurrentRadius / 2;
    }

    public void OnAdded(Frame f, EntityRef entity, Damageable* component)
    {
      var damageableData = f.FindAsset(component->DamageableData);
      component->Health = damageableData.MaxHealth;
    }


    public void DamageableHit(Frame f, EntityRef victim, EntityRef hitter, FP damage, Damageable* damageable)
    {
      var damageableBase = f.FindAsset<DamageableBase>(damageable->DamageableData);
      damageableBase.DamageableHit(f, victim, hitter, damage, damageable);
    }

    public void DamageableHealthRestored(Frame f, EntityRef entity, Damageable* damageable)
    {
      var maxHealth = f.FindAsset(damageable->DamageableData).MaxHealth;
      damageable->Health = maxHealth;
      f.Events.DamageableHealthUpdate(entity, maxHealth, damageable->Health);

    }

    
  }
}
