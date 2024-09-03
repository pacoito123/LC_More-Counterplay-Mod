using HarmonyLib;
using MoreCounterplay.Behaviours;
using MoreCounterplay.Config;
using MoreCounterplay.Items;
using MoreCounterplay.Util;
using UnityEngine;

namespace MoreCounterplay.Patches
{
    /// <summary>
    ///     Patch for loading texture assets upon loading in.
    /// </summary>
    [HarmonyPatch(typeof(StartOfRound))]
    internal class StartOfRoundPatch
    {
        [HarmonyPatch(typeof(StartOfRound), nameof(StartOfRound.Start))]
        [HarmonyPostfix]
        private static void FindAndLoadTextures()
        {
            MoreCounterplay.Log("Loading prefab textures...");

            // Try to find and obtain the Coilhead's enemy prefab.
            if (VanillaPrefabUtils.GetInsideEnemyPrefab("Spring", out GameObject? coilheadPrefab))
            {
                if (HeadItem.Prefab != null && coilheadPrefab != null)
                {
                    SpringManAI coilhead = coilheadPrefab.GetComponent<SpringManAI>();
                    coilhead.enemyHP = ConfigSettings.SpringDurability.Value;
                    coilhead.enemyType.canDie = true;

                    // Add explosion network behaviour script to the Coilhead prefab.
                    CoilExplosion coilExplosion = coilheadPrefab.AddComponent<CoilExplosion>();
                    coilExplosion.enabled = false;

                    // Add radioactive fire particle effects container to the Coilhead prefab.
                    CoilExplosion.RadioactiveFirePrefab?.transform.SetParent(coilheadPrefab.transform.Find("SpringManModel"), false);
                    CoilExplosion.RadioactiveFirePrefab?.SetActive(false);

                    // Obtain Coilhead material from its enemy prefab and assign it to the 'Coilless Coilhead' scrap item prefab.
                    Material coilheadMaterial = coilheadPrefab.transform.Find("SpringManModel/Head").GetComponent<MeshRenderer>().material;
                    HeadItem.Prefab.GetComponent<MeshRenderer>().material = coilheadMaterial;
                }
                else
                {
                    MoreCounterplay.LogWarning("Either the 'Coilless Coilhead' prefab did not load or the Coilhead enemy prefab could not be found. "
                        + "Either way, some stuff might not load or work properly.");
                }
            }

            // Try to find and obtain the Forest Giant enemy prefab.
            if (VanillaPrefabUtils.GetOutsideEnemyPrefab("ForestGiant", out GameObject? giantPrefab))
            {
                if (CoilExplosion.RadioactiveFirePrefab != null && giantPrefab != null)
                {
                    // Obtain burning Forest Giant flame texture from its enemy prefab and assign it to the radioactive fire's 'GreenFlame' material.
                    Texture flameTexture = giantPrefab.transform.Find("FireParticlesContainer/LingeringFire").GetComponent<ParticleSystemRenderer>().material.mainTexture;
                    CoilExplosion.RadioactiveFirePrefab.transform.Find("GreenFlame").GetComponent<ParticleSystemRenderer>().material.mainTexture = flameTexture;
                }
                else
                {
                    MoreCounterplay.LogWarning("Either the 'RadioactiveFire' prefab did not load or the Forest Giant enemy prefab could not be found. "
                        + "Either way, some stuff might not load or work properly.");
                }
            }

            MoreCounterplay.Log("Finished loading textures!");

            // Unpatch this method after running once.
            MoreCounterplay.Harmony?.Unpatch(AccessTools.Method(typeof(StartOfRound), nameof(StartOfRound.Start)),
                HarmonyPatchType.Postfix, MyPluginInfo.PLUGIN_GUID);
        }
    }
}