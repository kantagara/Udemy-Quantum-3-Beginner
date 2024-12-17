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

        public void DamageableHit(Frame f, EntityRef damageableEntity, FP damage, Damageable* damageable)
        {
            damageable->Health -= damage;
            if (damageable->Health < 0)
            {
                f.Destroy(damageableEntity);
            }
        }
    }
}