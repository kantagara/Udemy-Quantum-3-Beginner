namespace Quantum
{
    public class SemiAutomaticWeapon : FiringWeapon
    {
        public override void OnFirePressed(Frame f, WeaponSystem.Filter filter)
        {
            FireWeapon(f, filter);
        }
    }
}