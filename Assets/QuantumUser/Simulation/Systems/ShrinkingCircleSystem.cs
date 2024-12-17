using Photon.Deterministic;
using UnityEngine.Scripting;

namespace Quantum
{
    [Preserve]
    public unsafe class ShrinkingCircleSystem : SystemMainThread, ISignalOnComponentAdded<ShrinkingCircle>
    {
        public override void Update(Frame f)
        {
            var shrinkingCircle = f.Unsafe.GetPointerSingleton<ShrinkingCircle>();
            var config = f.FindAsset(shrinkingCircle->ShrinkingCircleConfig);
            shrinkingCircle->CurrentState.UpdateState(f, shrinkingCircle);

            if (shrinkingCircle->CurrentTimeToNextState > 0) return;
            if(shrinkingCircle->CurrentStateIndex >= config.States.Length - 1)
                return;
            
            shrinkingCircle->CurrentStateIndex++;
            config.States[shrinkingCircle->CurrentStateIndex].Materialize(f, ref shrinkingCircle->CurrentState);
            shrinkingCircle->CurrentState.EnterState(shrinkingCircle);
        }

        public unsafe void OnAdded(Frame f, EntityRef entity, ShrinkingCircle* shrinkingCircle)
        {
            shrinkingCircle->CurrentStateIndex = 0;
            var config = f.FindAsset(shrinkingCircle->ShrinkingCircleConfig);
            config.States[0].Materialize(f, ref shrinkingCircle->CurrentState);
            shrinkingCircle->CurrentState.EnterState(shrinkingCircle);

            var transform = f.Unsafe.GetPointer<Transform2D>(entity);
            var randomX = f.RNG->Next(config.MinimumBounds.X, config.MaximumBounds.X);
            var randomY = f.RNG->Next(config.MinimumBounds.Y, config.MaximumBounds.Y);
            var pos = new FPVector2(randomX, randomY);
            transform->Position = pos;
            shrinkingCircle->Position = pos;
        }
    }
}