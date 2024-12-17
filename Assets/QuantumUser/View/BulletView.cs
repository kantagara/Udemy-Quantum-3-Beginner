namespace Quantum {
  using UnityEngine;

  public class BulletView : QuantumEntityViewComponent
  {
    [SerializeField] private Transform bulletGraphics;

    public override void OnActivate(Frame frame)
    {
      var bullet = PredictedFrame.Get<Bullet>(EntityRef);
      var localPosition = bulletGraphics.localPosition;
      localPosition.y = bullet.HeightOffset.AsFloat;
      bulletGraphics.localPosition = localPosition;
    }
  }
}
