using UnityEngine.Scripting;

namespace Quantum {
  using Photon.Deterministic;
  [Preserve]
  public unsafe class CharacterMovableSystem : SystemMainThreadFilter<CharacterMovableSystem.Filter> {
    public override void Update(Frame f, ref Filter filter)
    {
      var input = f.GetPlayerInput(filter.PlayerLink->Player);
      MovePlayer(f, filter, input);
      RotatePlayer(f, filter, input);
    }

    private void RotatePlayer(Frame frame, Filter filter, Input* input)
    {
      var direction = input->MousePosition - filter.Transform->Position;
      filter.Transform->Rotation = FPVector2.RadiansSigned(FPVector2.Up, direction);
    }

    private static void MovePlayer(Frame f, Filter filter, Input* input)
    {
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
      public Transform2D* Transform;
    }

    
  }
}
