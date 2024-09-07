using Unity.Netcode;
using UnityEngine;

namespace MoreCounterplay.Items
{
	/// <summary>
	/// 	Defines the 'Coilless Coilhead' scrap item.
	/// </summary>
	public class HeadItem : RotatingItem
	{
		/// <summary>
		/// 	Prefab for the 'Coilless Coilhead' scrap item.
		/// </summary>
		public static GameObject? Prefab { get; internal set; }

		/// <summary>
		/// 	Time-seeded Random instance used for randomization purposes.
		/// </summary>
		public static System.Random Random
		{
			get => _random ??= new();
			private set => _random = value;
		}
		private static System.Random? _random;

		public override void Start()
		{
			base.Start();

			// Apply head position and rotation adjustments.
			transform.Rotate(new Vector3(0f, -90f, -35f));
			transform.Translate(new Vector3(-0.05f, -0.05f, 0f));

			// Remove hide flags.
			hideFlags = HideFlags.None;

			// Function to run when equipping in hand.
			OnEquip = () =>
			{
				if (!IsOwner) return;

				// Obtain a random angle.
				int randomAngle = Random.Next(0, 360);

				// Slowly turn head towards goal of 35° if the random angle is less than or equal to 75°, (roughly ~20% chance for it to happen).
				if (randomAngle <= 75)
				{
					OnEquipHeadServerRpc(1f / Random.Next(1, 5), randomAngle);
				}
				else
				{
					// Otherwise, set the item's angle to 35°.
					SetRotationTarget(35f, 35f);
				}
			};
		}

		[ClientRpc]
		public override void AttachItemClientRpc(NetworkObjectReference coilheadReference)
		{
			// Try to get Coilhead NetworkObject from its reference.
			if (coilheadReference.TryGet(out NetworkObject coilheadNetworkObject))
			{
				// Obtain Coilhead's head prefab.
				GameObject originalHead = coilheadNetworkObject.transform.Find("SpringManModel/Head").gameObject;

				// Attach head scrap item to original head object.
				AttachTo(originalHead);

				// Stop original head from rendering (instead of disabling it).
				originalHead.GetComponent<Renderer>().enabled = false;
			}
		}

		/// <summary>
		/// 	Set head rotation speed and starting angle on clients.
		/// </summary>
		/// <param name="rotationSpeed">The speed at which the head rotates towards the target angle.</param>
		/// <param name="startingAngle">The angle at which the head starts its rotation from.</param>
		[ClientRpc]
		public void OnEquipHeadClientRpc(float rotationSpeed, int startingAngle)
		{
			// Set head rotation speed and starting angle.
			SetRotationSpeed(rotationSpeed);
			SetRotationTarget(startingAngle, 35f);
		}

		/// <summary>
		/// 	Set head rotation speed and starting angle on server.
		/// </summary>
		/// <param name="rotationSpeed">The speed at which the head rotates towards the target angle.</param>
		/// <param name="startingAngle">The angle at which the head starts its rotation from.</param>
		[ServerRpc(RequireOwnership = false)]
		public void OnEquipHeadServerRpc(float rotationSpeed, int startingAngle)
		{
			// Set head rotation speed and starting angle on clients.
			OnEquipHeadClientRpc(rotationSpeed, startingAngle);
		}
	}
}