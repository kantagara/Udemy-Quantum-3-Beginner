using Photon.Deterministic;

namespace Quantum
{
    public class ShotgunBullet : BulletData
    {
        public int NumberOfBullets;
        public FP SpreadAngle;

        public override unsafe void CreateBullet(Frame f, WeaponBase weaponData, EntityRef owner)
        {
            var ownerTransform = f.Get<Transform2D>(owner);
            var spreadAngleRad = FP.Deg2Rad * SpreadAngle;

            for (int i = 0; i < NumberOfBullets; i++)
            {
                var bulletEntity = f.Create(Bullet);
                var bullet = f.Unsafe.GetPointer<Bullet>(bulletEntity);
                var bulletTransform = f.Unsafe.GetPointer<Transform2D>(bulletEntity);
                bulletTransform->Position =
                    ownerTransform.Position + weaponData.Offset.XZ.Rotate(ownerTransform.Rotation);
                bulletTransform->Rotation =
                    ownerTransform.Rotation + FPMath.Lerp(-spreadAngleRad, spreadAngleRad, (FP)i / (NumberOfBullets - 1));
                bullet->Speed = Speed;
                bullet->HeightOffset = weaponData.Offset.Y;
                bullet->Owner = owner;
                bullet->Time = Duration;
                bullet->Damage = Damage;
                bullet->Direction = bulletTransform->Up;
            }
        }
    }
}