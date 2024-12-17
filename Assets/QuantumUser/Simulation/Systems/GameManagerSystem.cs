using Photon.Deterministic;
using UnityEngine;
using UnityEngine.Scripting;

namespace Quantum
{
    [Preserve]
    public unsafe class GameManagerSystem : SystemMainThread, ISignalOnComponentAdded<GameManager>, ISignalPlayerKilled, ISignalOnPlayerDisconnected
    {
        public override void Update(Frame f)
        {
            
            var gameManager = f.Unsafe.GetPointerSingleton<GameManager>();
            if (gameManager->CurrentGameState != GameState.WaitingForPlayers)
                return;
            gameManager->TimeToWaitForPlayers -= f.DeltaTime;
            if (gameManager->TimeToWaitForPlayers <= FP._0)
            {
                gameManager->CurrentGameState =
                    f.ComponentCount<PlayerLink>() > 1 ? GameState.Playing : GameState.GameOver;

                if (gameManager->CurrentGameState == GameState.GameOver)
                {
                    if (GetWinner(f, out var entityRef))
                        f.Events.GameOver(entityRef);
                    else
                        Log.Error("No winner found");
                }
            }
        }

        private bool GetWinner(Frame f, out EntityRef entityRef)
        {
            entityRef = EntityRef.None;
            foreach (var entityPair in f.GetComponentIterator<PlayerLink>()) entityRef = entityPair.Entity;

            return entityRef != EntityRef.None;
        }

        public void OnAdded(Frame f, EntityRef entity, GameManager* component)
        {
            var config = f.FindAsset<GameManagerConfig>(component->GameManagerConfig);
            component->TimeToWaitForPlayers = config.TimeToWaitForPlayers;
        }

        public void PlayerKilled(Frame f)
        {
            EvaluateGameOverCondition(f);
        }

        private void EvaluateGameOverCondition(Frame f)
        {
            var gameManager = f.Unsafe.GetPointerSingleton<GameManager>();
            if (gameManager->CurrentGameState != GameState.Playing)
            {
                return;
            }
            var count = f.ComponentCount<PlayerLink>();
            
            if (count > 1)
                return;
            
            if (GetWinner(f, out var winner))
            {
                f.Events.GameOver(winner);
                gameManager->CurrentGameState = GameState.GameOver;
            }
            else Log.Error("No winner found");
        }

        public void OnPlayerDisconnected(Frame f, PlayerRef player)
        {
            foreach (var pair in f.GetComponentIterator<PlayerLink>())
            {
                if (pair.Component.Player == player)
                {
                    f.Destroy(pair.Entity);
                    break;
                }
            }
            
            EvaluateGameOverCondition(f);
        }
    }
}