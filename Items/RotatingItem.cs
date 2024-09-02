using System;
using UnityEngine;

namespace MoreCounterplay.Items
{
	/// <summary>
	/// 	Represents an item that rotates across the z-axis towards a specific angle.
	/// </summary>
	public class RotatingItem : AttachedItem
	{
		/// <summary>
		/// 	The current angle of the item.
		/// </summary>
		public float CurrentAngle { get; private set; } = 90f;

		/// <summary>
		/// 	The speed at which the item rotates towards the target angle.
		/// </summary>
		public float RotationSpeed { get; private set; } = 0.5f;

		/// <summary>
		/// 	The angle at which the item starts its rotation from.
		/// </summary>
		public float StartingAngle { get; private set; } = 90f;

		/// <summary>
		/// 	The target angle to reach while rotating.
		/// </summary>
		public float TargetAngle { get; private set; } = 35f;

		/// <summary>
		/// 	Whether the rotating item has reached its target angle or not.
		/// </summary>
		public bool ReachedTarget { get; private set; } = false;

		public override void Start()
		{
			base.Start();

			// Reset rotation upon equipping the item.
			OnEquip = ResetRotation;
		}

		public override void ApplyOffsets()
		{
			// Apply rotation and position offsets.
			Vector3 rotationOffset = new(itemProperties.rotationOffset.x, itemProperties.rotationOffset.y, CurrentAngle);
			transform.position += parentObject.rotation * itemProperties.positionOffset;
			transform.Rotate(rotationOffset);

			// Return if item has reached its target angle.
			if (ReachedTarget) return;

			// Update current angle if the target angle has not been reached.
			if (CurrentAngle > (TargetAngle + RotationSpeed * 2))
			{
				CurrentAngle -= RotationSpeed;
			}
			else if (CurrentAngle < (TargetAngle - RotationSpeed * 2))
			{
				CurrentAngle += RotationSpeed;
			}
			else
			{
				// Otherwise, set target angle as reached.
				ReachedTarget = true;
			}
		}

		/// <summary>
		/// 	Resets item rotation to the starting angle.
		/// </summary>
		public void ResetRotation()
		{
			SetRotation(StartingAngle);
		}

		/// <summary>
		/// 	Sets current angle to a specified amount.
		/// </summary>
		/// <param name="angle">The current angle of the item.</param>
		public void SetRotation(float angle)
		{
			CurrentAngle = angle;
			ReachedTarget = false;
		}

		/// <summary>
		/// 	Sets starting and target angles for the item.
		/// </summary>
		/// <param name="startingAngle">The angle at which the item starts its rotation from.</param>
		/// <param name="targetAngle">The target angle to reach while rotating.</param>
		public void SetRotationTarget(float startingAngle, float targetAngle)
		{
			StartingAngle = Math.Abs(startingAngle % 360);
			TargetAngle = Math.Abs(targetAngle % 360);
			ResetRotation();
		}

		/// <summary>
		/// 	Sets item rotation speed.
		/// </summary>
		/// <param name="rotationSpeed">The speed at which the item should rotate.</param>
		public void SetRotationSpeed(float rotationSpeed)
		{
			RotationSpeed = rotationSpeed;
		}

		public override string __getTypeName()
		{
			return "RotatingItem";
		}
	}
}