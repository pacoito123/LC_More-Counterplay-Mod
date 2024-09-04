using HarmonyLib;
using UnityEngine;

namespace MoreCounterplay.Patches
{
    [HarmonyPatch]
    internal class TurretPatch
    {
        private const int KNIFE_HIT_ID = 5;

        [HarmonyPatch(typeof(Turret), "IHittable.Hit")]
        [HarmonyPostfix]
        public static void CheckHitID(Turret __instance, int hitID = -1)
        {
            if (!MoreCounterplay.Settings.EnableTurretCounterplay) return;
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
            if (!MoreCounterplay.Settings.EnableTurretCounterplay) return true;
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
