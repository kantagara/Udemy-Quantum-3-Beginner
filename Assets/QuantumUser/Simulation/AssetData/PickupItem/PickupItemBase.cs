using Photon.Deterministic;

namespace Quantum
{
    public abstract class PickupItemBase : AssetObject
    {
        public FP PickupTime;

        public abstract void PickupItem(Frame f, EntityRef entityBeingPickedUp, EntityRef entityPickingUp);
    }
}