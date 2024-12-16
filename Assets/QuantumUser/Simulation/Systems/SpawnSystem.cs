using Quantum.Collections;
using UnityEngine.Scripting;

namespace Quantum
{
    using Photon.Deterministic;
    [Preserve]
    public unsafe class SpawnSystem : SystemSignalsOnly, ISignalOnPlayerAdded
    {
        public void OnPlayerAdded(Frame f, PlayerRef player, bool firstTime)
        {
            if (!firstTime) return;
            var playerEntityRef = CreatePlayer(f, player);
            PlacePlayerOnSpawnPosition(f, playerEntityRef);
        }

        private void PlacePlayerOnSpawnPosition(Frame frame, EntityRef playerEntityRef)
        {
            var spawnPointManager = frame.Unsafe.GetPointerSingleton<SpawnPointManager>();
            var availableSpawnPoints = frame.ResolveList(spawnPointManager->AvailableSpawnPoints);
            var usedSpawnPoints = frame.ResolveList(spawnPointManager->UsedSpawnPoints);
            if (availableSpawnPoints.Count == 0 && usedSpawnPoints.Count == 0)
            {
                foreach (var componentPair in frame.GetComponentIterator<SpawnPoint>())
                {
                    availableSpawnPoints.Add(componentPair.Entity);
                }
            }
            

            var randomIndex = frame.RNG->Next(0, availableSpawnPoints.Count);
            var spawnPoint = availableSpawnPoints[randomIndex];
            var spawnPointTransform = frame.Get<Transform2D>(spawnPoint);
            var playerTransform = frame.Unsafe.GetPointer<Transform2D>(playerEntityRef);
            playerTransform->Position = spawnPointTransform.Position;
            
            availableSpawnPoints.RemoveAt(randomIndex);
            usedSpawnPoints.Add(spawnPoint);
            if (availableSpawnPoints.Count == 0)
            {
                spawnPointManager->AvailableSpawnPoints = usedSpawnPoints;
                spawnPointManager->UsedSpawnPoints = new QListPtr<EntityRef>();
            }
        }

        private static EntityRef CreatePlayer(Frame f, PlayerRef player)
        {
            var playerData = f.GetPlayerData(player);
            var playerEntity = f.Create(playerData.PlayerAvatar);
            var playerLink = new PlayerLink()
            {
                Player = player
            };
            f.Add(playerEntity, playerLink);
            var kcc = f.Unsafe.GetPointer<KCC>(playerEntity);
            var kccSettings = f.FindAsset(kcc->Settings);
            kcc->Acceleration = kccSettings.Acceleration;
            kcc->MaxSpeed = kccSettings.BaseSpeed;

            return playerEntity;
        }
    }
}