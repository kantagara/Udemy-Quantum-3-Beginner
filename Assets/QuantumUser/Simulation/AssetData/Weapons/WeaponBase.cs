namespace Quantum
{
    using Photon.Deterministic;

    public abstract unsafe class WeaponBase : AssetObject
    {
        public WeaponType WeaponType;
        public FP Cooldown;
        public FPVector3 Offset;

        public virtual void OnInit(Frame f, EntityRef entity, Weapon* weapon)
        {
        }

        public virtual void OnUpdate(Frame f, WeaponSystem.Filter filter)
        {
            if (filter.Weapon->CooldownTime >= FP._0)
            {
                filter.Weapon->CooldownTime -= f.DeltaTime;
                return;
            }

            var input = f.GetPlayerInput(filter.Player->Player);
            if (input->Fire.WasPressed)
                OnFirePressed(f, filter);
            else if (input->Fire.WasReleased)
                OnFireReleased(f, filter);
            else if (input->Fire.IsDown) OnFireHeld(f, filter);
        }

        public virtual void OnFirePressed(Frame f, WeaponSystem.Filter filter)
        {
        }

        public virtual void OnFireReleased(Frame f, WeaponSystem.Filter filter)
        {
        }

        public virtual void OnFireHeld(Frame f, WeaponSystem.Filter filter)
        {
        }
    }
}