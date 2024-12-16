using System;
using Photon.Deterministic;
using Quantum.Core;

namespace Quantum
{
	public enum KCCMovementType
	{
		None,
		Free,
		Tangent
	}

	public struct KCCMovementData
	{
		public KCCMovementType Type;
		public FPVector2 Correction;
		public FPVector2 Direction;
		public FP MaxPenetration;
	}

	public unsafe class KCCSettings : AssetObject
	{
		// This is the KCC actual radius (non penetrable)
		public FP Radius = FP._0_50;
		public Int32 MaxContacts = 2;
		public FP AllowedPenetration = FP._0_10;
		public FP CorrectionSpeed = FP._10;
		public FP BaseSpeed = FP._2;
		public FP Acceleration = FP._10;
		public Boolean Debug = false;
		public FP Brake = 1;
		
		public LayerMask LayerMask;
		
		public bool MoveAlongsideRotation = true;

		public void Init(ref KCC kcc)
		{
			kcc.Settings = this;
			kcc.MaxSpeed = BaseSpeed;
			kcc.Acceleration = Acceleration;
		}

		public void Move(Frame frame, EntityRef entity, FPVector2 direction)
		{
			var kccMovementData = ComputeRawMovement(frame, entity, direction);
			SteerAndMove(frame, entity, in kccMovementData);
		}

		private void SteerAndMove(FrameBase f, EntityRef entity, in KCCMovementData movementData)
		{
			KCC* kcc = null;
			if (f.Unsafe.TryGetPointer(entity, out kcc) == false)
			{
				return;
			}

			Transform2D* transform = null;
			if (f.Unsafe.TryGetPointer(entity, out transform) == false)
			{
				return;
			}

			if (movementData.Type != KCCMovementType.None)
			{

				var direction = GetMovementDirection(transform, movementData);
				// Now apply this direction, scaled by acceleration and deltaTime
				kcc->Velocity += kcc->Acceleration * f.DeltaTime * direction;

				// Clamp the velocity to the maximum speed
				if (kcc->Velocity.SqrMagnitude > kcc->MaxSpeed * kcc->MaxSpeed) {
					kcc->Velocity = kcc->Velocity.Normalized * kcc->MaxSpeed;
				}
			} else {
				// Apply braking logic
				kcc->Velocity = FPVector2.MoveTowards(kcc->Velocity, FPVector2.Zero, f.DeltaTime * Brake);
			}


			if (movementData.MaxPenetration > AllowedPenetration)
			{
				if (movementData.MaxPenetration > AllowedPenetration * 2)
				{
					transform->Position += movementData.Correction;
				}
				else
				{
					transform->Position += movementData.Correction * f.DeltaTime * CorrectionSpeed;
				}

			}


			transform->Position += kcc->Velocity * f.DeltaTime;



#if DEBUG
			if (Debug)
			{
				Draw.Circle(transform->Position, Radius, ColorRGBA.Blue);
				Draw.Ray(transform->Position, transform->Forward * Radius, ColorRGBA.Red);
			}
#endif
		}

		private FPVector2 GetMovementDirection(Transform2D* transform, KCCMovementData movementData)
		{
			if (MoveAlongsideRotation)
			{
				// Calculate the forward direction based on rotation if transform->Forward is not updated
				// Assuming transform->Rotation is the angle in radians
				FP cosRotation = FPMath.Cos(transform->Rotation);
				FP sinRotation = FPMath.Sin(transform->Rotation);

				// If there's additional directional input that needs to be considered (e.g., from a joystick),
				// it should be rotated accordingly. Assuming movementData.Direction is such an input:
				FPVector2 rotatedDirection = new FPVector2(
					movementData.Direction.X * cosRotation - movementData.Direction.Y * sinRotation,
					movementData.Direction.X * sinRotation + movementData.Direction.Y * cosRotation
				);

				return rotatedDirection;
			}

			return movementData.Direction;
		}

		public KCCMovementData ComputeRawMovement(FrameBase f, EntityRef entity, FPVector2 direction)
		{
			KCC* kcc = null;
			if (f.Unsafe.TryGetPointer(entity, out kcc) == false)
			{
				return default;
			}

			Transform2D* transform = null;
			if (f.Exists(entity) == false || f.Unsafe.TryGetPointer(entity, out transform) == false)
			{
				return default;
			}

			KCCMovementData movementPack = default;


			movementPack.Type = direction != default ? KCCMovementType.Free : KCCMovementType.None;
			movementPack.Direction = direction;
			Shape2D shape = Shape2D.CreateCircle(Radius);

			var layer = LayerMask;
      var hits = f.Physics2D.OverlapShape(transform->Position, FP._0, shape, layer, options: QueryOptions.HitStatics | QueryOptions.ComputeDetailedInfo);
      int count = Math.Min(MaxContacts, hits.Count);

      if (hits.Count > 0)
			{
				Boolean initialized = false;
				hits.Sort(transform->Position);
				for (int i = 0; i < hits.Count && count > 0; i++)
				{
					// ignore triggers
					if (hits[i].IsTrigger)
					{
						// callback here...
						continue;
					}

					// ignoring "self" contact
					if (hits[i].Entity == entity)
					{
						continue;
					}

					var contactPoint = hits[i].Point;
					var contactToCenter = transform->Position - contactPoint;
					var localDiff = contactToCenter.Magnitude - Radius;


#if DEBUG
					if (Debug)
					{
						Draw.Circle(contactPoint, FP._0_10, ColorRGBA.Red);
					}
#endif

					var localNormal = contactToCenter.Normalized;

					count--;

					// define movement type
					if (!initialized)
					{
						initialized = true;

						if (direction != default)
						{
							var angle = FPVector2.RadiansSkipNormalize(direction.Normalized, localNormal);
							if (angle >= FP.Rad_90)
							{
								var d = FPVector2.Dot(direction, localNormal);
								var tangentVelocity = direction - localNormal * d;
								if (tangentVelocity.SqrMagnitude > FP.EN4)
								{
									movementPack.Direction = tangentVelocity.Normalized;
									movementPack.Type = KCCMovementType.Tangent;
								}
								else
								{
									movementPack.Direction = default;
									movementPack.Type = KCCMovementType.None;
								}

							}
						}
						movementPack.MaxPenetration = FPMath.Abs(localDiff);
					}

					// any real contact contributes to correction and average normal
					var localCorrection = localNormal * -localDiff;
					movementPack.Correction += localCorrection;
				}
			}

			return movementPack;
		}

		
	}
}