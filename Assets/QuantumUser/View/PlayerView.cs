using System.Collections;
using System.Collections.Generic;
using Quantum;
using UnityEngine;

public unsafe class PlayerView : QuantumEntityViewComponent
{
    private static readonly int MoveX = Animator.StringToHash("moveX");
    private static readonly int MoveZ = Animator.StringToHash("moveZ");
    [SerializeField] private Animator animator;

    public override void OnUpdateView()
    {
        UpdateAnimator();
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
