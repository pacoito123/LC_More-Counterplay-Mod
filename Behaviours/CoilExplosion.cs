using MoreCounterplay.Config;
using MoreCounterplay.Items;
using Unity.Netcode;
using UnityEngine;

namespace MoreCounterplay.Behaviours
{
	/// <summary>
	/// 	Network behaviour script handling a dissected Coilhead's volatility.
	/// </summary>
	internal class CoilExplosion : NetworkBehaviour
	{
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
			Coilhead = GetComponent<SpringManAI>();
			FireContainer = transform.Find("SpringManModel/RadioactiveFire").gameObject;
		}

		public void LateUpdate()
		{
			if (!Ticking)
			{
				// Update time since last stop if explosion timer is not ticking.
				TimeSinceLastStop += Time.deltaTime;
				return;
			}

			// Check if explosion time is up.
			if (TimeLeft <= 0.0f)
			{
				// Attempt to destroy 'Coilless Coilhead' scrap item if still attached.
				AttachedItem? attachedItem = HeadContainer?.GetComponent<AttachedItem>();
				if (attachedItem != null && attachedItem.IsAttached && !attachedItem.heldByPlayerOnServer && ConfigSettings.ExplosionDestroysHead.Value)
				{
					HeadContainer?.GetComponent<NetworkObject>().Despawn();
				}

				// Spawn explosion on clients.
				SpawnExplosionClientRpc();

				// Update scan node text on clients.
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
			Landmine.SpawnExplosion(transform.position, true, ConfigSettings.ExplosionKillRadius.Value,
				ConfigSettings.ExplosionDamageRadius.Value, ConfigSettings.ExplosionDamage.Value);

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
			// Play unused 'Spring1.ogg' sound effect, or "EnterCooldown.ogg" sound effect if the former is not found.
			Coilhead?.creatureVoice.PlayOneShot(HeadItem.Prefab?.GetComponent<HeadItem>().itemProperties.throwSFX ?? Coilhead?.enterCooldownSFX);
		}

		/// <summary>
		/// 	Update the Coilhead's scan node on all clients.
		/// </summary>
		/// <param name="ignited">Whether the Coilhead is currently on fire or not.</param>
		[ClientRpc]
		internal void UpdateScanNodeClientRpc(bool ignited)
		{
			// Obtain and re-enable scan node.
			ScanNodeProperties scanNode = transform.Find("SpringManModel/ScanNode").GetComponent<ScanNodeProperties>();
			scanNode.GetComponent<Collider>().enabled = true;

			// Update scan node to match current state.
			scanNode.headerText = $"{(ignited ? "Fissile" : "Decayed")} Coil-Head";
			scanNode.subText = ignited ? "Run." : "I wouldn't get close...";
			scanNode.nodeType = ignited ? 1 : 0;
		}
	}
}