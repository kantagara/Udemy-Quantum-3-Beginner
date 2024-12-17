using Photon.Deterministic;

namespace Quantum
{
    public unsafe partial struct ShrinkingCircleState
    {
        public void EnterState(ShrinkingCircle* shrinkingCircle)
        {
            
            shrinkingCircle->CurrentTimeToNextState = TimeToNextState;
            switch (CircleStateUnion.Field)
            {
                case CircleStateUnion.PRESHRINKSTATE:
                    shrinkingCircle->TargetRadius = CircleStateUnion.PreShrinkState->TargetRadius;
                    break;
                case CircleStateUnion.INITIALSTATE:
                    shrinkingCircle->CurrentRadius = shrinkingCircle->InitialRadiusOfState = CircleStateUnion.InitialState->InitialRadius;
                    break;
                case Quantum.CircleStateUnion.SHRINKSTATE:
                    shrinkingCircle->InitialRadiusOfState = shrinkingCircle->CurrentRadius;
                    CircleStateUnion.ShrinkState->ShrinkingCircleTime = 0;
                    break;
            }
        }

        public void UpdateState(Frame f, ShrinkingCircle* shrinkingCircle)
        {
            if(shrinkingCircle->CurrentTimeToNextState <= 0) 
                return;
            shrinkingCircle->CurrentTimeToNextState -= f.DeltaTime;
            switch (CircleStateUnion.Field)
            {
                case Quantum.CircleStateUnion.SHRINKSTATE:
                    var shrinkState = CircleStateUnion.ShrinkState;
                    shrinkState->ShrinkingCircleTime += f.DeltaTime / TimeToNextState;
                    shrinkingCircle->CurrentRadius = FPMath.Lerp(shrinkingCircle->InitialRadiusOfState,
                        shrinkingCircle->TargetRadius, shrinkState->ShrinkingCircleTime);
                    Log.Info(shrinkState->ShrinkingCircleTime + " " + shrinkingCircle->CurrentRadius);
                    break;
            }
        }
    }
}