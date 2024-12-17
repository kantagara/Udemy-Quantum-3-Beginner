namespace Quantum
{
    public unsafe class AmmoPickup : PickupItemBase
    {
        public override void PickupItem(Frame f, EntityRef entityBeingPickedUp, EntityRef entityPickingUp)
        {
            var weapon = f.Unsafe.GetPointer<Weapon>(entityPickingUp);
            var weaponData = f.FindAsset<WeaponBase>(weapon->WeaponData);
            if(weaponData is not FiringWeapon firingWeapon)
                return;
            weapon->Ammo = firingWeapon.MaxAmmo;
            f.Events.AmmoChanged(entityPickingUp, weapon->Ammo);
            f.Destroy(entityBeingPickedUp);
        }
    }
}