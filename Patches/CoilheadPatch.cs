using HarmonyLib;
using MoreCounterplay.Behaviours;
using MoreCounterplay.Items;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

namespace MoreCounterplay.Patches
{
    [HarmonyPatch]
    internal class CoilheadPatch
    {
        private const int SHOVEL_HIT_ID = 1;
        private const int KNIFE_HIT_ID = 5;

        [HarmonyPatch(typeof(EnemyAI), nameof(EnemyAI.HitEnemy))]
        [HarmonyPostfix]
        private static void HitCoilhead(EnemyAI __instance, int hitID = -1)
        {
            if (!MoreCounterplay.Settings.EnableCoilheadCounterplay) return;
            if (__instance.isEnemyDead || __instance.GetType() != typeof(SpringManAI)) return;
            SpringManAI coilhead = (SpringManAI)__instance;

            // Negate damage while the Coilhead is on cooldown.
            if (coilhead.inCooldownAnimation)
            {
                MoreCounterplay.Log($"Coilhead hit negated.");
                return;
            }

            // Obtain damage dealt from config file.
            int force = hitID switch
            {
                KNIFE_HIT_ID => MoreCounterplay.Settings.CoilheadKnifeDamage,
                SHOVEL_HIT_ID => MoreCounterplay.Settings.CoilheadShovelDamage,
                _ => MoreCounterplay.Settings.CoilheadDefaultDamage,
            };

            // Apply damage and kill on client if health drops below zero.
            MoreCounterplay.Log($"Coilhead hit for {force} damage on server.");
            coilhead.enemyHP -= force;
            if (coilhead.enemyHP <= 0)
            {
                MoreCounterplay.Log($"Coilhead killed!");
                coilhead.KillEnemyOnOwnerClient(false);
            }
        }

        [HarmonyPatch(typeof(EnemyAI), nameof(EnemyAI.KillEnemy))]
        [HarmonyPostfix]
        private static void KillCoilhead(EnemyAI __instance)
        {
            if (!MoreCounterplay.Settings.EnableCoilheadCounterplay) return;
            if (__instance.GetType() != typeof(SpringManAI)) return;
            SpringManAI coilhead = (SpringManAI)__instance;

            // Reset Coilhead animations, collider, and target upon death.
            ResetCoilhead(coilhead);

            // Boioioioing.
            coilhead.DoSpringAnimation(true);

            // Enable radioactive fire particles on clients and spawn scrap head from server.
            IgniteCoilhead(coilhead, SpawnHead(coilhead));
        }

        /// <summary>
        ///     Reset a given Coilhead's animations, collider, and target.
        /// </summary>
        /// <param name="coilhead">The Coilhead to reset.</param>
        private static void ResetCoilhead(SpringManAI coilhead)
        {
            coilhead.hasStopped = true;
            coilhead.creatureAnimator.SetFloat("walkSpeed", 0f);
            coilhead.currentAnimSpeed = 0f;
            coilhead.mainCollider.isTrigger = false;
            if (coilhead.IsOwner)
            {
                coilhead.agent.speed = 0f;
                coilhead.movingTowardsTargetPlayer = false;
                coilhead.SetDestinationToPosition(coilhead.transform.position);
            }
        }

        /// <summary>
        ///     Spawn 'Coilless Coilhead' scrap item and attach it to the given Coilhead's head.
        /// </summary>
        /// <param name="coilhead">The Coilhead to attach the head to.</param>
        /// <returns>The spawned 'Coilless Coilhead' instance.</returns>
        private static GameObject? SpawnHead(SpringManAI coilhead)
        {
            if ((!coilhead.IsServer && !coilhead.IsHost) || !MoreCounterplay.Settings.DropHeadAsScrap) return null;

            if (HeadItem.Prefab == null)
            {
                MoreCounterplay.LogWarning("'Coilless Coilhead' prefab did not load correctly or is missing; it will not be spawned.");
                return null;
            }

            // Obtain Coilhead's head prefab.
            GameObject originalHead = coilhead.meshRenderers.First(mesh => mesh.name == "Head").gameObject;

            // Create head item instance.
            GameObject scrapHead = Object.Instantiate(HeadItem.Prefab, originalHead.transform.position, originalHead.transform.rotation);

            // Find and obtain head item's NetworkObject and HeadItem components.
            if (!scrapHead.TryGetComponent(out NetworkObject networkHead) || !scrapHead.TryGetComponent(out HeadItem headItem))
            {
                MoreCounterplay.LogWarning("'Coilless Coilhead' instance has missing components; it will not be spawned.");
                Object.Destroy(scrapHead);
                return null;
            }

            // Spawn head item instance on all clients.
            networkHead.Spawn();

            // Assign scrap value and attach item to original head.
            headItem.SetScrapValue(HeadItem.Random.Next(MoreCounterplay.Settings.MinHeadValue, MoreCounterplay.Settings.MaxHeadValue + 1));
            headItem.AttachItemClientRpc(coilhead.thisNetworkObject);

            // Sync scrap values with clients.
            RoundManager.Instance.SyncScrapValuesClientRpc([networkHead], [headItem.scrapValue]);

            return scrapHead;
        }

        /// <summary>
        ///     Ignites a given Coilhead and enables their explosion timer.
        /// </summary>
        /// <param name="coilhead">The Coilhead to set on fire.</param>
        /// <param name="scrapHead">The 'Coilless Coilhead' instance to (possibly) destroy.</param>
        private static void IgniteCoilhead(SpringManAI coilhead, GameObject? scrapHead)
        {
            if (!MoreCounterplay.Settings.LoreAccurateCoilheads) return;

            // Enable radioactive fire particle effects.
            coilhead.transform.Find("SpringManModel/RadioactiveFire")?.gameObject?.SetActive(true);

            // Activate behaviour script only when called from the server.
            if (!coilhead.IsServer && !coilhead.IsHost) return;

            // Find and obtain the given Coilhead's CoilExplosion script.
            if (!coilhead.TryGetComponent(out CoilExplosion coilExplosion))
            {
                MoreCounterplay.LogWarning("Coilhead CoilExplosion script did not load correctly or is missing; explosion will not happen.");
                return;
            }

            // Assign object reference to behaviour script.
            coilExplosion.HeadContainer = scrapHead;

            // Update scan node text on all clients.
            coilExplosion.UpdateScanNodeClientRpc(true);

            // Set explosion timer to how long the Coilhead last moved for, clamped to configuration values.
            coilExplosion.TimeLeft = Mathf.Clamp(coilExplosion.TimeSinceLastStop, MoreCounterplay.Settings.MinExplosionTimer, MoreCounterplay.Settings.MaxExplosionTimer);
            coilExplosion.Ticking = true; // Run.
            coilExplosion.enabled = true;
        }

        [HarmonyPatch(typeof(SpringManAI), nameof(SpringManAI.SetAnimationGoServerRpc))]
        [HarmonyPostfix]
        private static void OnStartedMoving(SpringManAI __instance)
        {
            // Enable behaviour script timer only when called from the server.
            if ((!__instance.IsServer && !__instance.IsHost) || !MoreCounterplay.Settings.LoreAccurateCoilheads) return;

            // Try to find CoilExplosion behaviour script.
            if (__instance.TryGetComponent(out CoilExplosion coilExplosion))
            {
                // Reset timer since last stop and start updating it.
                coilExplosion.TimeSinceLastStop = 0.0f;
                coilExplosion.enabled = true;
            }
        }

        [HarmonyPatch(typeof(SpringManAI), nameof(SpringManAI.SetAnimationStopServerRpc))]
        [HarmonyPostfix]
        private static void OnStoppedMoving(SpringManAI __instance)
        {
            // Disable behaviour script timer only when called from the server.
            if ((!__instance.IsServer && !__instance.IsHost) || !MoreCounterplay.Settings.LoreAccurateCoilheads) return;

            // Try to find CoilExplosion behaviour script.
            if (__instance.TryGetComponent(out CoilExplosion coilExplosion))
            {
                // Stop updating time since last stop.
                coilExplosion.enabled = false;
            }
        }
    }
}