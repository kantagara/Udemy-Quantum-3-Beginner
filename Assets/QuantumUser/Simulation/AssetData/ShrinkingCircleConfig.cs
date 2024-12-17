using Photon.Deterministic;
using Quantum.Prototypes;
using UnityEngine;

namespace Quantum
{
    public class ShrinkingCircleConfig : AssetObject
    {
        public ShrinkingCircleStatePrototype[] States;
        public FP DamageDealingPerSecond;
        public FPVector2 MinimumBounds, MaximumBounds;
    }
}
