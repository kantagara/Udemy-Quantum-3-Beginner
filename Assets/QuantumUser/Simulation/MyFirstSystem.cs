using UnityEngine.Scripting;

namespace Quantum {
  using Photon.Deterministic;
  [Preserve]
  public unsafe class MyFirstSystem : SystemMainThread {
    public override void Update(Frame f) {
        Log.Info("Hello From my First Systems Update Loop!");
    }
  }
}