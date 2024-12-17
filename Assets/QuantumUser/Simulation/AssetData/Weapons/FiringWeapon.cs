using Photon.Deterministic;

namespace Quantum
{
    public abstract unsafe class FiringWeapon : WeaponBase
    {
        public BulletData BulletData;
        public byte MaxAmmo;


        public override void OnInit(Frame f, EntityRef entity, Weapon* weapon)
        {
            weapon->Ammo = MaxAmmo;
        }


        protected void FireWeapon(Frame f, WeaponSystem.Filter filter)
        {
            if (filter.Weapon->Ammo <= 0) return;
            filter.Weapon->CooldownTime = Cooldown;
            filter.Weapon->Ammo--;
            f.Events.AmmoChanged(filter.Entity, filter.Weapon->Ammo);
            f.Signals.CreateBullet(filter.Entity, this);
        }
    }
}