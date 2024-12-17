using System;
using Photon.Deterministic;

namespace Quantum
{
    public class CharacterStatsConfig : AssetObject
    {
        public FP HealthMultiplier;
        
        public FP FireRateModifier;

        private void OnValidate()
        {
            HealthMultiplier = FPMath.Clamp(HealthMultiplier, 1, 2);
            FireRateModifier = FPMath.Clamp(FireRateModifier, FP._0_10, 1);
        }
    }
}