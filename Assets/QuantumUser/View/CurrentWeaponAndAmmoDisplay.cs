using System;
using System.Collections;
using System.Collections.Generic;
using Quantum;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CurrentWeaponAndAmmoDisplay : MonoBehaviour
{
    [SerializeField] private Image weaponImage;

    [SerializeField] private TMP_Text weaponAmmo;

    private void Awake()
    {
        QuantumEvent.Subscribe<EventWeaponChanged>(this, WeaponChanged);
        QuantumEvent.Subscribe<EventPlayerSpawned>(this, PlayerSpawned);
        QuantumEvent.Subscribe<EventAmmoChanged>(this, AmmoChanged);
    }

    private void OnDestroy()
    {
        QuantumEvent.UnsubscribeListener(this);
    }

    private void AmmoChanged(EventAmmoChanged callback)
    {
        var f = callback.Game.Frames.Predicted;
        if(callback.Game.PlayerIsLocal(f.Get<PlayerLink>(callback.Entity).Player) == false)
            return;
        FillImageAndText(f, callback.Entity);
    }

    private void PlayerSpawned(EventPlayerSpawned callback)
    {
        if(callback.Game.PlayerIsLocal(callback.PlayerLink.Player) == false)
            return;
        var f = callback.Game.Frames.Verified;
        FillImageAndText(f, callback.Player);
    }

    private void FillImageAndText(Frame f, EntityRef entityRef)
    {
        var weapon = f.Get<Weapon>(entityRef);
        weaponImage.sprite = f.FindAsset(weapon.WeaponData).WeaponSprite;
        weaponAmmo.text = weapon.Ammo.ToString();
    }

    private void WeaponChanged(EventWeaponChanged callback)
    {
        var f = callback.Game.Frames.Verified;
        if(callback.Game.PlayerIsLocal(f.Get<PlayerLink>(callback.Entity).Player) == false)
            return;
        FillImageAndText(f, callback.Entity);

    }
}
