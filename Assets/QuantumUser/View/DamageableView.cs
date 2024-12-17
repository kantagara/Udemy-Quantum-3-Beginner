using System.Collections;
using System.Collections.Generic;
using Photon.Deterministic;
using Quantum;
using UnityEngine;
using UnityEngine.UI;

public class DamageableView : QuantumEntityViewComponent
{
    [SerializeField] private Image healthImage;

    public override void OnActivate(Frame frame)
    {
        base.OnActivate(frame);
        QuantumEvent.Subscribe<EventDamageableHit>(this, DamageableHit);
    }

    public override void OnDeactivate()
    {
        base.OnDeactivate();
        QuantumEvent.UnsubscribeListener(this);

    }

    private void DamageableHit(EventDamageableHit callback)
    {
        if(EntityRef != callback.entityRef) return;
        StartCoroutine(UpdateHealthUI(callback.maxHealth, callback.currentHealth));
    }

    private IEnumerator UpdateHealthUI(FP maxHealth, FP currentHealth)
    {
        float percentage = (currentHealth / maxHealth).AsFloat;
        while (!Mathf.Approximately(percentage, healthImage.fillAmount))
        {
            healthImage.fillAmount = Mathf.Lerp(percentage, healthImage.fillAmount, 0.1f);
            yield return null;
        }
    }
}
