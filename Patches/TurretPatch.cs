using HarmonyLib;
using MoreCounterplay.Config;
using UnityEngine;
using GameNetcodeStuff;
using Unity.Netcode;
using System.Linq;
using System;
using MoreCounterplay.MonoBehaviours;

namespace MoreCounterplay.Patches
{
    [HarmonyPatch]
    internal class TurretPatch
    {
        private const int KNIFE_HIT_ID = 5;

        [HarmonyPatch(typeof(Turret), "IHittable.Hit")]
        [HarmonyPostfix]
        public static void CheckHitID(Turret __instance, int force, Vector3 hitDirection, PlayerControllerB playerWhoHit, bool playHitSFX, int hitID = -1)
        {
            if (!ConfigSettings.EnableTurretCounterplay.Value) return;
            if (hitID == KNIFE_HIT_ID)
            {
                MoreCounterplay.Log($"Turret hit using knife");
                var counterplay = __instance.gameObject.AddComponent<TurretCounterplay>();
                counterplay.TurretDisabled = true;
            }
        }

        [HarmonyPatch(typeof(Turret), "Update")]
        [HarmonyPrefix]
        public static bool CheckIfTurretGotDisabled(Turret __instance)
        {
            if (!ConfigSettings.EnableTurretCounterplay.Value) return true;
            if (__instance.turretMode != TurretMode.Detection) return true;

            var turret = __instance.gameObject.GetComponent<TurretCounterplay>();

            if (turret != null && turret.TurretDisabled)
            {
                if (__instance.turretActive)
                {
                    __instance.ToggleTurretEnabled(false);
                    __instance.mainAudio.Stop();
                    __instance.farAudio.Stop();
                    __instance.berserkAudio.Stop();
                    __instance.bulletCollisionAudio.Stop();
                    __instance.bulletParticles.Stop();
                    __instance.mainAudio.PlayOneShot(__instance.turretDeactivate);

                    if (!MoreCounterplay.IsHostOrServer) return false;
                    if (!ConfigSettings.DropGunAsScrap.Value) return false;
                    MoreCounterplay.Log($"Spawn turret scrap");

                    var spawnPosition = __instance.transform.parent.GetComponentsInChildren<Transform>().First(child => child.name == "GunBody").position;
                    var gunRotation = __instance.transform.parent.GetComponentsInChildren<Transform>().First(child => child.name == "GunBody").rotation;
                    var gunItem = GameObject.Instantiate(MoreCounterplay.TurretGunItem.spawnPrefab, spawnPosition, gunRotation);
                    gunItem.GetComponentInChildren<GrabbableObject>().SetScrapValue(UnityEngine.Random.Range(ConfigSettings.MinGunValue.Value, ConfigSettings.MaxGunValue.Value));
                    gunItem.GetComponentInChildren<NetworkObject>().Spawn();
                    RoundManager.Instance.SyncScrapValuesClientRpc(new NetworkObjectReference[] { gunItem.GetComponent<NetworkObject>() }, new int[] { gunItem.GetComponent<GrabbableObject>().scrapValue });

                    PluginNetworkBehaviour.Instance.DisableTurretGunOnLocalClient(__instance.GetComponent<NetworkObject>());
                    PluginNetworkBehaviour.Instance.DisableTurretGunClientRpc(__instance.GetComponent<NetworkObject>());
                }
                return false;
            }

            return true;
        }
    }

    internal class TurretCounterplay : MonoBehaviour
    {
        public bool TurretDisabled = false;
    }
}
