namespace Quantum
{
    public class BasicPickupItem : PickupItemBase
    {
        public override void PickupItem(Frame f, EntityRef entityBeingPickedUp, EntityRef entityPickingUp)
        {
            f.Destroy(entityBeingPickedUp);
        }
    }
}