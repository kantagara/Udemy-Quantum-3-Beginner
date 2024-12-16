using UnityEngine.Scripting;

namespace Quantum {
  using Photon.Deterministic;

  [Preserve]
  public unsafe class CharacterControllerSystem : SystemMainThreadFilter<CharacterControllerSystem.Filter> {
    public override void Update(Frame f, ref Filter filter)
    {
      filter.CharacterController->Move(f, filter.Entity, default);
    }

    public struct Filter {
      public EntityRef Entity;
      public CharacterController3D* CharacterController;
    }
  }
}
