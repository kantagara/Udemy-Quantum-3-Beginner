namespace Quantum {
  using Photon.Deterministic;

  public unsafe class BulletData : AssetObject
  {
    public FP Duration;
    public EntityPrototype Bullet;
    public FP Damage;
    public FP Speed;

    public virtual void CreateBullet(Frame f, WeaponBase weaponData, EntityRef owner)
    {
      var bulletEntity = f.Create(Bullet);
      var bulletTransform = f.Unsafe.GetPointer<Transform2D>(bulletEntity);
      var ownersTransform = f.Get<Transform2D>(owner);

      bulletTransform->Position = ownersTransform.Position + weaponData.Offset.XZ.Rotate(ownersTransform.Rotation);
      bulletTransform->Rotation = ownersTransform.Rotation;
      var bullet = f.Unsafe.GetPointer<Bullet>(bulletEntity);
      bullet->Speed = Speed;
      bullet->Damage = Damage;
      bullet->HeightOffset = weaponData.Offset.Y;
      bullet->Owner = owner;
      bullet->Time = Duration;
      bullet->Direction = ownersTransform.Up;
    }
  }
}
