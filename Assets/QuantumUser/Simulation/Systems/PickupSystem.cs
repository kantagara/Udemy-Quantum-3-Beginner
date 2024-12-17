using UnityEngine.Scripting;

namespace Quantum {
  using Photon.Deterministic;

  [Preserve]
  public unsafe class PickupSystem : SystemMainThreadFilter<PickupSystem.Filter>, ISignalOnTriggerEnter2D, ISignalOnTriggerExit2D, ISignalOnComponentAdded<PickupItem> {
    public override void Update(Frame f, ref Filter filter) {
      if(filter.PickupItem->EntityPickingUp == EntityRef.None)
        return;
      filter.PickupItem->CurrentPickupTime += f.DeltaTime;
      if (filter.PickupItem->CurrentPickupTime >= filter.PickupItem->PickupTime)
      {
        var baseConfig = f.FindAsset<PickupItemBase>(filter.PickupItem->PickupItemBase);
        baseConfig.PickupItem(f, filter.Entity, filter.PickupItem->EntityPickingUp);
      }
    }

    public struct Filter {
      public EntityRef Entity;
      public PickupItem* PickupItem;
    }

    public void OnTriggerEnter2D(Frame f, TriggerInfo2D info)
    {
       if(!f.TryGet(info.Entity, out PlayerLink _)) return;
       if(!f.Unsafe.TryGetPointer(info.Other, out PickupItem* pickupItem))
         return;
       if(pickupItem->EntityPickingUp != EntityRef.None)
         return;
       pickupItem->EntityPickingUp = info.Entity;
    }

    public void OnTriggerExit2D(Frame f, ExitInfo2D info)
    {
      if(!f.TryGet(info.Entity, out PlayerLink _)) return;
      if(!f.Unsafe.TryGetPointer(info.Other, out PickupItem* pickupItem))
        return;
      if(pickupItem->EntityPickingUp != info.Entity)
        return;
      pickupItem->EntityPickingUp = EntityRef.None;
      pickupItem->CurrentPickupTime = 0;
    }

    public void OnAdded(Frame f, EntityRef entity, PickupItem* component)
    {
      var baseConfig = f.FindAsset<PickupItemBase>(component->PickupItemBase);
      component->PickupTime = baseConfig.PickupTime;
    }
  }
}
