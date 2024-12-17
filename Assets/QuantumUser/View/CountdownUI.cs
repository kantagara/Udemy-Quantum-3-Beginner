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

    public override void OnUpdateView()
    {
        var f = PredictedFrame;
        var shrinkingCircle = f.GetSingleton<ShrinkingCircle>();
        var time = shrinkingCircle.CurrentTimeToNextState;
        var currentState = shrinkingCircle.CurrentState;
        timeRemainingText.text = time.AsFloat.ToString("F2", CultureInfo.InvariantCulture);
        timeProgressImage.fillAmount = (time.AsFloat / currentState.TimeToNextState.AsFloat);
    }
    
    public override void OnActivate(Frame frame)
    {
        QuantumEvent.Subscribe<EventGameOver>(this, GameOver);
    }

    private void GameOver(EventGameOver callback)
    {
        var f = callback.Game.Frames.Predicted;
        var playerRef = f.Get<PlayerLink>(callback.Winner).Player;
        var playerData = f.GetPlayerData(playerRef);
        Debug.Log("Winner is " + playerData.PlayerNickname);

    }

    public override void OnDeactivate()
    {
        QuantumEvent.UnsubscribeListener(this);
    }
}
