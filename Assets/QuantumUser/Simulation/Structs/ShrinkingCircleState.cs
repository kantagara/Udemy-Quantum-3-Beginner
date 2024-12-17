namespace Quantum
{
    public unsafe partial struct ShrinkingCircleState
    {
        public void EnterState(ShrinkingCircle* shrinkingCircle)
        {
            Log.Info("ENTERING STATE " + CircleStateUnion.Field);
            shrinkingCircle->CurrentTimeToNextState = TimeToNextState;
            switch (CircleStateUnion.Field)
            {
                case CircleStateUnion.PRESHRINKSTATE:
                    shrinkingCircle->TargetRadius = CircleStateUnion.PreShrinkState->TargetRadius;
                    break;
                case CircleStateUnion.INITIALSTATE:
                    shrinkingCircle->TargetRadius = CircleStateUnion.InitialState->InitialRadius;
                    break;
            }
        }

        public void UpdateState(Frame f, ShrinkingCircle* shrinkingCircle)
        {
            if(shrinkingCircle->CurrentTimeToNextState <= 0) 
                return;
            shrinkingCircle->CurrentTimeToNextState -= f.DeltaTime;
            
        }
    }
}