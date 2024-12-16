using System;
using System.Collections;
using System.Collections.Generic;
using Quantum;
using UnityEngine;
using LayerMask = UnityEngine.LayerMask;

public unsafe class PlayerView : QuantumEntityViewComponent
{
    private static readonly int MoveX = Animator.StringToHash("moveX");
    private static readonly int MoveZ = Animator.StringToHash("moveZ");
    [SerializeField] private Animator animator;
    [SerializeField] private GameObject overheadUi;

    private bool _isLocalPlayer;
    private Renderer[] _renderers;

    private void Awake()
    {
        _renderers = GetComponentsInChildren<Renderer>(true);
    }

    public override void OnUpdateView()
    {
        UpdateAnimator();
    }

    public override void OnActivate(Frame frame)
    {
        _isLocalPlayer = _game.PlayerIsLocal(frame.Get<PlayerLink>(EntityRef).Player);
        var layer = LayerMask.NameToLayer(_isLocalPlayer ? "Player_Local" : "Player_Remote");

        foreach (var renderer in _renderers)
        {
            renderer.gameObject.layer = layer;
            renderer.enabled = true;
        }
        overheadUi.SetActive(true);
        QuantumEvent.Subscribe<EventOnPlayerEnteredGrass>(this, OnPlayerEnteredGrass);
        QuantumEvent.Subscribe<EventOnPlayerExitGrass>(this, OnPlayerExitGrass);
    }

    public override void OnDeactivate()
    {
        QuantumEvent.UnsubscribeListener(this);
    }

    private void OnPlayerExitGrass(EventOnPlayerExitGrass callback)
    {
        ToggleRndererVisibility(callback.Player, true);
    }

    private void ToggleRndererVisibility(PlayerRef player, bool visible)
    {
        if(player != PredictedFrame.Get<PlayerLink>(EntityRef).Player)
            return;
        if(_isLocalPlayer)
            return;
        foreach (var renderer in _renderers)
        {
            renderer.enabled = visible;
        }
        overheadUi.SetActive(visible);
    }

    private void OnPlayerEnteredGrass(EventOnPlayerEnteredGrass callback)
    {
        ToggleRndererVisibility(callback.Player, false);
    }

    private void UpdateAnimator()
    {
        var input = PredictedFrame.GetPlayerInput(PredictedFrame.Get<PlayerLink>(EntityRef).Player);
        var kcc = PredictedFrame.Get<KCC>(EntityRef);
        var velocity = kcc.Velocity;
        animator.SetFloat(MoveX, velocity.X.AsFloat);
        animator.SetFloat(MoveZ, velocity.Y.AsFloat);
    }
}
