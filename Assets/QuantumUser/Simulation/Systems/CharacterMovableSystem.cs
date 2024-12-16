using UnityEngine.Scripting;

namespace Quantum {
  using Photon.Deterministic;
  [Preserve]
  public unsafe class CharacterMovableSystem : SystemMainThreadFilter<CharacterMovableSystem.Filter> {
    public override void Update(Frame f, ref Filter filter)
    {
      var input = f.GetPlayerInput(filter.PlayerLink->Player);
      var direction = input->Direction;
      if (direction.Magnitude > 1)
      {
        direction = direction.Normalized;
      }

      var kccSettings = f.FindAsset(filter.KCC->Settings);
      kccSettings.Move(f, filter.Entity, direction);
    }

    public struct Filter {
      public EntityRef Entity;
      public KCC* KCC;
      public PlayerLink* PlayerLink;
    }

    
  }
}
