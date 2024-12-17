using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Quantum
{
    public unsafe class WeaponPickupItem : PickupItemBase
    {
        public WeaponBase WeaponBase;
        public override void PickupItem(Frame f, EntityRef entityBeingPickedUp, EntityRef entityPickingUp)
        {
            var weapon = f.Unsafe.GetPointer<Weapon>(entityPickingUp);
            weapon->WeaponData = WeaponBase;
            weapon->CooldownTime = 0;
            WeaponBase.OnInit(f, entityPickingUp, weapon);
            f.Events.WeaponChanged(entityPickingUp, WeaponBase.WeaponType);
            f.Destroy(entityBeingPickedUp);
        }
    }
}
