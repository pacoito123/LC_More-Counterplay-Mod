using GameNetcodeStuff;
using HarmonyLib;
using MoreCounterplay.Config;
using System.Linq;
using UnityEngine;
namespace MoreCounterplay.Patches
{
    [HarmonyPatch]
    internal class CoilheadPatch
    {
        private const int SHOVEL_HIT_ID = 1;
        private const int KNIFE_HIT_ID = 5;

        [HarmonyPatch(typeof(EnemyAI), nameof(EnemyAI.Start))]
        [HarmonyPostfix]
        public static void SetCoilheadHP(EnemyAI __instance)
        {
            if (!ConfigSettings.EnableCoilheadCounterplay.Value) return;
            if (__instance.GetType() != typeof(SpringManAI)) return;

            __instance.enemyHP = ConfigSettings.SpringDurability.Value;
            __instance.enemyType.canDie = true;
            __instance.dieSFX = ((SpringManAI)__instance).springNoises[Random.Range(0, ((SpringManAI)__instance).springNoises.Length)];
        }

        [HarmonyPatch(typeof(EnemyAI), nameof(EnemyAI.HitEnemy))]
        [HarmonyPostfix]
        public static void HitCoilhead(EnemyAI __instance, int force = 1, PlayerControllerB playerWhoHit = null, bool playHitSFX = false, int hitID = -1)
        {
            if (!ConfigSettings.EnableCoilheadCounterplay.Value) return;
            if (__instance.GetType() != typeof(SpringManAI)) return;

            if (__instance.IsOwner && !__instance.isEnemyDead)
            {
                switch (hitID)
                {
                    case KNIFE_HIT_ID:
                        CoilheadTakeHit(__instance, ConfigSettings.CoilheadKnifeDamage.Value);
                        break;

                    case SHOVEL_HIT_ID:
                        CoilheadTakeHit(__instance, ConfigSettings.CoilheadShovelDamage.Value);
                        break;

                    default:
                        CoilheadTakeHit(__instance, ConfigSettings.CoilheadDefaultDamage.Value);
                        break;
                }
            }
        }

        public static void CoilheadTakeHit(EnemyAI __instance, int force)
        {
            MoreCounterplay.Log($"Coilhead hit for {force} damage");
            __instance.enemyHP -= force;
            if (__instance.enemyHP <= 0) KillCoilhead(__instance);
        }

        public static void KillCoilhead(EnemyAI __instance)
        {
            MoreCounterplay.Log($"Coilhead killed");
            __instance.KillEnemyOnOwnerClient(false);
            __instance.meshRenderers.First(mesh => mesh.name == "Head").gameObject.SetActive(false);
        }
    }
}
