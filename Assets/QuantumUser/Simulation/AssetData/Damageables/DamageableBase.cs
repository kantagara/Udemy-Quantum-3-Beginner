namespace Quantum {
  using Photon.Deterministic;

  public abstract unsafe class DamageableBase : AssetObject
  {
    public FP MaxHealth;
    public abstract void DamageableHit(Frame frame, EntityRef victim, EntityRef hitter, FP damage, Damageable* damageable);
  }
}
