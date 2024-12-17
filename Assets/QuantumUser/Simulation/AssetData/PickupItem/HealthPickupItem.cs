namespace Quantum
{
    public unsafe class HealthPickupItem : PickupItemBase
    {
        public override void PickupItem(Frame f, EntityRef entityBeingPickedUp, EntityRef entityPickingUp)
        {
            if(f.Unsafe.TryGetPointer<Damageable>(entityPickingUp, out var damageable))
                f.Signals.DamageableHealthRestored(entityPickingUp, damageable);
            f.Destroy(entityBeingPickedUp);
        }
    }
}