using DG.Tweening;

namespace Quantum {
  using UnityEngine;

  public class ShrinkingCircleView : QuantumEntityViewComponent
  {
    [SerializeField] private Transform redCircle, whiteCircle;

    public override void OnActivate(Frame frame)
    {
      QuantumEvent.Subscribe<EventShrinkingCircleChangedState>(this, ShrinkingCircleChangedState);
      var shrinkingCircle = frame.GetSingleton<ShrinkingCircle>();
      whiteCircle.localScale = redCircle.localScale =
        new Vector3(shrinkingCircle.CurrentRadius.AsFloat, shrinkingCircle.CurrentRadius.AsFloat);
      redCircle.gameObject.SetActive(false);
    }

    public override void OnDeactivate()
    {
      QuantumEvent.UnsubscribeListener(this);
    }

    public override void OnLateUpdateView()
    {
      var shrinkingCircle = PredictedFrame.GetSingleton<ShrinkingCircle>();
      if(shrinkingCircle.CurrentState.CircleStateUnion.Field != CircleStateUnion.SHRINKSTATE)
        return;
      redCircle.localScale = new Vector3(shrinkingCircle.CurrentRadius.AsFloat, shrinkingCircle.CurrentRadius.AsFloat);
    }

    private void ShrinkingCircleChangedState(EventShrinkingCircleChangedState callback)
    {
      var shrinkingCircle = VerifiedFrame.GetSingleton<ShrinkingCircle>();
      var currentState = shrinkingCircle.CurrentState.CircleStateUnion.Field;
      redCircle.gameObject.SetActive(currentState is CircleStateUnion.PRESHRINKSTATE or CircleStateUnion.SHRINKSTATE);
      if (currentState == CircleStateUnion.PRESHRINKSTATE)
        whiteCircle.DOScale(new Vector3(shrinkingCircle.TargetRadius.AsFloat, shrinkingCircle.TargetRadius.AsFloat),
          1f);
    }
  }
}
