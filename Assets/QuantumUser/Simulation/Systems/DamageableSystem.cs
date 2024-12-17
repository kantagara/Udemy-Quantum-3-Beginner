using UnityEngine.Scripting;

namespace Quantum {
  using Photon.Deterministic;

  [Preserve]
  public unsafe class DamageableSystem : SystemSignalsOnly, ISignalOnComponentAdded<Damageable>, ISignalDamageableHit {
    

   
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
  }
}
