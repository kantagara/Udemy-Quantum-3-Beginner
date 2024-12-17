using Photon.Deterministic;
using UnityEngine.Scripting;

namespace Quantum
{
    [Preserve]
    public unsafe class GameManagerSystem : SystemMainThread, ISignalOnComponentAdded<GameManager>, ISignalPlayerKilled
    {
        public override void Update(Frame f)
        {
            var gameManager = f.Unsafe.GetPointerSingleton<GameManager>();
            if(gameManager->CurrentGameState != GameState.WaitingForPlayers)
                return;
            gameManager->TimeToWaitForPlayers -= f.DeltaTime;
            if (gameManager->TimeToWaitForPlayers <= FP._0)
            {
                gameManager->CurrentGameState =
                    f.ComponentCount<PlayerLink>() > 1 ? GameState.Playing : GameState.GameOver;

                if (gameManager->CurrentGameState == GameState.GameOver)
                {
                    var winner = GetWinner(f);
                    if (winner == EntityRef.None)
                    {
                        Log.Error("No winner was found!");
                        return;
                    }

                    f.Events.GameOver(winner);
                }
            }
        }

        private EntityRef GetWinner(Frame f)
        {
            EntityRef entityRef = EntityRef.None;
            foreach (var entityPair in f.GetComponentIterator<PlayerLink>())
            {
                entityRef = entityPair.Entity;
            }

            return entityRef;
        }

        public void OnAdded(Frame f, EntityRef entity, GameManager* component)
        {
            var config = f.FindAsset<GameManagerConfig>(component->GameManagerConfig);
            component->TimeToWaitForPlayers = config.TimeToWaitForPlayers;
        }

        public void PlayerKilled(Frame f)
        {
            
        }
    }
}