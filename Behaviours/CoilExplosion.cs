using MoreCounterplay.Items;
using Unity.Netcode;
using UnityEngine;

namespace MoreCounterplay.Behaviours
{
	/// <summary>
	/// 	Network behaviour script handling a dissected Coilhead's volatility.
	/// </summary>
	public class CoilExplosion : NetworkBehaviour
	{
		/// <summary>
		/// 	Vanilla prefab for the Coilhead.
		/// </summary>
		public static GameObject? CoilheadPrefab { get; internal set; }

		/// <summary>
		/// 	Prefab for the radioactive fire particle effects.
		/// </summary>
		public static GameObject? RadioactiveFirePrefab { get; internal set; }

		/// <summary>
		/// 	Whether the timer until explosion is ticking or not.
		/// </summary>
		public bool Ticking { get; internal set; } = false;

		/// <summary>
		/// 	Time left until explosion in seconds.
		/// </summary>
		public float TimeLeft { get; internal set; } = 0.0f;

		/// <summary>
		/// 	Time since the Coilhead's previous stop in seconds.
		/// </summary>
		public float TimeSinceLastStop { get; internal set; } = 0.0f;

		/// <summary>
		/// 	Whether the explosion sound effect has already played or not.
		/// </summary>
		public bool PlayedSfx { get; internal set; } = false;

		/// <summary>
		/// 	Instance of 'Coilless Coilhead' scrap item.
		/// </summary>
		public GameObject? HeadContainer { get; internal set; }

		/// <summary>
		/// 	Instance of radioactive fire particle effects.
		/// </summary>
		public GameObject? FireContainer { get; private set; }

		/// <summary>
		/// 	Instance of Coilhead behaviour script.
		/// </summary>
		public SpringManAI? Coilhead { get; private set; }

		public override void OnNetworkSpawn()
		{
			base.OnNetworkSpawn();

			// Assign object containers.
			if (TryGetComponent(out SpringManAI coilhead))
			{
				Coilhead = coilhead;
			}
			FireContainer = transform.Find("SpringManModel/RadioactiveFire")?.gameObject;
		}

		public void LateUpdate()
		{
			if (!Ticking)
			{
				// Update time since last stop if the explosion timer is not ticking.
				TimeSinceLastStop += Time.deltaTime;
				return;
			}

			// Check if explosion time is up.
			if (TimeLeft <= 0.0f)
			{
				// Attempt to destroy 'Coilless Coilhead' scrap item if still attached.
				if (HeadContainer?.TryGetComponent(out AttachedItem attachedItem) == true
					&& attachedItem.IsAttached && !attachedItem.heldByPlayerOnServer && MoreCounterplay.Settings.ExplosionDestroysHead)
				{
					HeadContainer?.GetComponent<NetworkObject>().Despawn();
				}

				// Spawn explosion on clients.
				SpawnExplosionClientRpc();

				// Update scan node text on all clients.
				UpdateScanNodeClientRpc(false);

				// Disable script after exploding.
				enabled = false;
				return;
			}
			else if (!PlayedSfx && TimeLeft <= 1.0f)
			{
				// Play explosion sound effect on clients if there's less than a second left until exploding.
				PlaySfxClientRpc();
				PlayedSfx = true;
			}

			// Subtract time since last frame from the time left until exploding.
			TimeLeft -= Time.deltaTime;
		}

		/// <summary>
		/// 	Spawn explosion on clients.
		/// </summary>
		[ClientRpc]
		private void SpawnExplosionClientRpc()
		{
			// Spawn explosion.
			Landmine.SpawnExplosion(transform.position, true, MoreCounterplay.Settings.ExplosionKillRadius,
				MoreCounterplay.Settings.ExplosionDamageRadius, MoreCounterplay.Settings.ExplosionDamage);

			// Boioioioing.
			Coilhead?.DoSpringAnimation(true);

			// Disable radioactive fire particles.
			FireContainer?.SetActive(false);
		}

		/// <summary>
		/// 	Play explosion sound effect on all clients.
		/// </summary>
		[ClientRpc]
		private void PlaySfxClientRpc()
		{
			// Play 'Spring1.ogg' sound effect, or 'EnterCooldown.ogg' sound effect if the former is not found.
			Coilhead?.creatureVoice.PlayOneShot(HeadItem.Prefab?.TryGetComponent(out HeadItem headItem) == true
				? headItem.itemProperties.throwSFX : Coilhead?.enterCooldownSFX, MoreCounterplay.Settings.ExplosionWarnVolume.Value);
		}

		/// <summary>
		/// 	Update the Coilhead's scan node on all clients.
		/// </summary>
		/// <param name="ignited">Whether the Coilhead is currently on fire or not.</param>
		[ClientRpc]
		internal void UpdateScanNodeClientRpc(bool ignited)
		{
			// Return if not re-enabling the Coilhead scan node.
			if (!MoreCounterplay.Settings.ModifyCoilheadScanNode.Value) return;

			// Find and obtain Coilhead scan node.
			if (transform.Find("SpringManModel/ScanNode")?.TryGetComponent(out ScanNodeProperties scanNode) != true) return;

			// Find, obtain, and re-enable scan node Collider.
			if (scanNode.TryGetComponent(out Collider collider))
			{
				collider.enabled = true;
			}

			// Return if not making any further changes to the Coilhead scan node.
			if (!MoreCounterplay.Settings.ModifyCoilheadScanNode.Value) return;

			// Update scan node to match current state.
			scanNode.headerText = $"{(ignited ? "Fissile" : "Decayed")} Coil-Head";
			scanNode.subText = ignited ? "Run." : "Don't get too close...";
			scanNode.nodeType = ignited ? 1 : 0;
		}
	}
}