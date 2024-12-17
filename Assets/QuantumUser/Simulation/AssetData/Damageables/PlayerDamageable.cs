using Photon.Deterministic;

namespace Quantum
{
    public class PlayerDamageable : DamageableBase
    {
        public override unsafe void DamageableHit(Frame f, EntityRef victim, EntityRef hitter, FP damage, Damageable* damageable)
        {
            damageable->Health -= damage;
            if (damageable->Health <= 0)
            {
                f.Destroy(victim);
                return;
            }

            f.Events.DamageableHit(victim, MaxHealth, damageable->Health);
        }
    }
}