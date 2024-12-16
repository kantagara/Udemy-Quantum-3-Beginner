using UnityEngine.Scripting;

namespace Quantum {
  using Photon.Deterministic;
  [Preserve]
  public unsafe class CharacterMovableSystem : SystemMainThreadFilter<CharacterMovableSystem.Filter>, ISignalOnPlayerAdded {
    public override void Update(Frame f, ref Filter filter)
    {
      var input = f.GetPlayerInput(filter.PlayerLink->Player);
      var direction = input->Direction.XOY;
      if (direction.Magnitude > 1)
      {
        direction = direction.Normalized;
      }
      filter.CharacterController3D->Move(f, filter.Entity, direction);
    }

    public struct Filter {
      public EntityRef Entity;
      public CharacterController3D* CharacterController3D;
      public PlayerLink* PlayerLink;
    }

    public void OnPlayerAdded(Frame f, PlayerRef player, bool firstTime)
    {
      var playerData = f.GetPlayerData(player);
      var playerEntity = f.Create(playerData.PlayerAvatar);
      var playerLink = new PlayerLink()
      {
        Player = player
      };
      f.Add(playerEntity, playerLink);
    }
  }
}
