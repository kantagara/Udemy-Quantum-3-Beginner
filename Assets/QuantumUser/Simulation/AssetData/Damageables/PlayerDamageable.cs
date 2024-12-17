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
                DropLoot(f, victim);
                f.Signals.PlayerKilled();
                f.Destroy(victim);
                return;
            }

            f.Events.DamageableHealthUpdate(victim, MaxHealth, damageable->Health);
        }

        private unsafe void DropLoot(Frame f, EntityRef victim)
        {
            var transform = f.Get<Transform2D>(victim);
            //Drop Health Loot on the right side of the victim
            var healthLoot = f.Create(f.SimulationConfig.HealthPickupItem);
            f.Unsafe.GetPointer<Transform2D>(healthLoot)->Position = transform.Position + transform.Right * 2;
            //Check if the victim has a weapon component
            if(!f.TryGet<Weapon>(victim, out var weapon))
                return;
            //Drop Weapon Loot on the left side of the victim
            var weaponData = f.FindAsset<WeaponBase>(weapon.WeaponData);
            var weaponLoot = f.Create(f.SimulationConfig.GetEntityPrototypeFromWeaponType(weaponData.WeaponType));
            f.Unsafe.GetPointer<Transform2D>(weaponLoot)->Position = transform.Position + transform.Left * 2;
        }
    }
}