using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using Quantum;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CountdownUI : QuantumSceneViewComponent
{
    [SerializeField] private TMP_Text timeRemainingText;
    [SerializeField] private Image timeProgressImage;
    [SerializeField] private TMP_Text gameStateText;

    private QuantumRunner _quantumRunner;

    public override void OnActivate(Frame frame)
    {
        QuantumEvent.Subscribe<EventShrinkingCircleChangedState>(this, ShrinkingCircleChangedState);
        if (PredictedFrame.GetSingleton<GameManager>().CurrentGameState == GameState.WaitingForPlayers)
        {
            gameStateText.SetText("Waiting for players!");
        }
        
        _quantumRunner = QuantumRunner.Default;
    }

    public override void OnDeactivate()
    {
        QuantumEvent.UnsubscribeListener(this);
    }

    private void ShrinkingCircleChangedState(EventShrinkingCircleChangedState callback)
    {
        gameStateText.SetText(ShrinkingCircleStateToMessage());
    }

    public override void OnUpdateView()
    {
        var f = PredictedFrame;
        var gameManager = PredictedFrame.GetSingleton<GameManager>();
        if (gameManager.CurrentGameState == GameState.WaitingForPlayers)
        {
            var data = f.FindAsset(gameManager.GameManagerConfig);
            var time = gameManager.TimeToWaitForPlayers.AsFloat;
            timeRemainingText.SetText(time.ToString("F2", CultureInfo.InvariantCulture));
            timeProgressImage.fillAmount = time / data.TimeToWaitForPlayers.AsFloat;
        }
        else
        {
            DisableRoomJoining();
            
            var shrinkingCircle = f.GetSingleton<ShrinkingCircle>();
            var time = Mathf.Max(0, shrinkingCircle.CurrentTimeToNextState.AsFloat);
            var currentState = shrinkingCircle.CurrentState;
            timeRemainingText.text = time.ToString("F2", CultureInfo.InvariantCulture);
            timeProgressImage.fillAmount = (time / currentState.TimeToNextState.AsFloat);
        }
    }

    private void DisableRoomJoining()
    {
        if(_quantumRunner.NetworkClient == null)
            return;
        if (_quantumRunner.NetworkClient.CurrentRoom.IsVisible == false)
            return;
        _quantumRunner.NetworkClient.CurrentRoom.IsVisible = false;
    }

    private string ShrinkingCircleStateToMessage()
    {
        var shrinkingCircleState = PredictedFrame.GetSingleton<ShrinkingCircle>().CurrentState.CircleStateUnion.Field;

        switch (shrinkingCircleState)
        {
            case CircleStateUnion.PRESHRINKSTATE:
                return "Go to the safe area!";
            case CircleStateUnion.SHRINKSTATE:
                return "Area shrinking!";
            case CircleStateUnion.COOLDOWNSTATE:
                return "Cooldown!";
            case CircleStateUnion.INITIALSTATE:
                return "Area Initialized!";
            default: throw new ArgumentOutOfRangeException("Uknown state!");
            
        }
    }
    
}
