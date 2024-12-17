using UnityEngine;
using UnityEngine.UI;

namespace Quantum{
    
    public class PickupItemView : QuantumEntityViewComponent
    {
        [SerializeField] private Image progressBarFill;

        public override void OnUpdateView()
        {
            if(!PredictedFrame.Exists(EntityRef)) return;
            var pickupItem = PredictedFrame.Get<PickupItem>(EntityRef);
            var percentage = (pickupItem.CurrentPickupTime / pickupItem.PickupTime).AsFloat;
            progressBarFill.fillAmount = percentage;
        }
    }    
}