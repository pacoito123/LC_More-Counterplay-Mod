using System;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Animations;

namespace MoreCounterplay.Items
{
	/// <summary>
	/// 	Represents an item that spawns attached to another object.
	/// </summary>
	public class AttachedItem : PhysicsProp
	{
		/// <summary>
		/// 	Whether the item is still attached to its initial parent or not.
		/// </summary>
		public bool IsAttached { get; private set; } = false;

		/// <summary>
		/// 	Function callback to run whenever the item is grabbed.
		/// </summary>
		public Action? OnGrab { get; set; }

		/// <summary>
		/// 	Function callback to run whenever the item is equipped.
		/// </summary>
		public Action? OnEquip { get; set; }

		public override void Start()
		{
			base.Start();

			grabbable = true;
			grabbableToEnemies = true;

			// Detach item from its parent when grabbed.
			OnGrab = () =>
			{
				if (IsAttached && IsOwner)
				{
					// Detach item on server if still attached.
					DetachItemServerRpc();
				}
			};
		}

		public override void LateUpdate()
		{
			// Update radar icon position.
			if (radarIcon != null)
			{
				radarIcon.position = transform.position;
			}

			// Return if item does not have a parent assigned or is still attached.
			if (parentObject == null || IsAttached) return;

			// Update position and rotation only after detaching.
			transform.position = parentObject.position;
			transform.rotation = parentObject.rotation;

			// Apply position and rotation offsets.
			ApplyOffsets();
		}

		public override void GrabItem()
		{
			base.GrabItem();

			// Invoke grab item callback.
			OnGrab?.Invoke();
		}

		public override void EquipItem()
		{
			base.EquipItem();

			// Invoke equip item callback.
			OnEquip?.Invoke();
		}

		/// <summary>
		/// 	Applies position and rotation offsets to the item. Meant to be overridden by inheriting classes.
		/// </summary>
		public virtual void ApplyOffsets()
		{
			transform.position += parentObject.rotation * itemProperties.positionOffset;
			transform.Rotate(itemProperties.rotationOffset);
		}

		/// <summary>
		/// 	Attach item to a parent object.
		/// </summary>
		/// <param name="parent">The object to attach to.</param>
		public void AttachTo(GameObject parent)
		{
			if (IsAttached) return;

			// Add parent as source and activate constraint to move alongside it.
			ParentConstraint parentConstraint = gameObject.GetComponent<ParentConstraint>();
			parentConstraint.AddSource(new()
			{
				weight = 1f,
				sourceTransform = parent.transform
			});
			parentConstraint.constraintActive = true;

			// Set item as attached to its parent.
			parentObject = parent.transform;
			IsAttached = true;
		}

		/// <summary>
		/// 	Attach item to a parent object on clients.
		/// </summary>
		/// <param name="parent">NetworkObject reference of the object instance to attach to.</param>
		[ClientRpc]
		public virtual void AttachItemClientRpc(NetworkObjectReference parent)
		{
			// Try to get parent NetworkObject from its reference.
			if (parent.TryGet(out NetworkObject networkObject))
			{
				// Attach item to parent object.
				AttachTo(networkObject.gameObject);
			}
		}

		/// <summary>
		/// 	Detach item from its parent object on clients.
		/// </summary>
		[ClientRpc]
		public void DetachItemClientRpc()
		{
			// Remove source and deactivate constraint.
			ParentConstraint parentConstraint = gameObject.GetComponent<ParentConstraint>();
			parentConstraint.RemoveSource(0);
			parentConstraint.constraintActive = false;

			// Set item as no longer attached.
			IsAttached = false;
		}

		/// <summary>
		/// 	Detach item from its parent object on server.
		/// </summary>
		[ServerRpc(RequireOwnership = false)]
		public void DetachItemServerRpc()
		{
			DetachItemClientRpc();
		}

		public override string __getTypeName()
		{
			return "AttachedItem";
		}
	}
}