using HarmonyLib;
using MoreCounterplay.Behaviours;
using MoreCounterplay.Items;
using MoreCounterplay.Util;
using UnityEngine;

namespace MoreCounterplay.Patches
{
    /// <summary>
    ///     Patches for loading/unloading stuff upon joining or leaving a lobby.
    /// </summary>
    [HarmonyPatch]
    internal class LoadPatches
    {
        [HarmonyPatch(typeof(GameNetworkManager), nameof(GameNetworkManager.StartDisconnect))]
        [HarmonyPostfix]
        private static void OnDisconnect()
        {
            // Remove added components from the Coilhead prefab.
            CoilExplosion.RadioactiveFirePrefab?.transform.SetParent(null);
            Object.Destroy(CoilExplosion.CoilheadPrefab?.GetComponent<CoilExplosion>());

            // Reset Coilhead prefab to vanilla values.
            if (CoilExplosion.CoilheadPrefab?.TryGetComponent(out SpringManAI coilhead) ?? false)
            {
                coilhead.enemyHP = 3;
                coilhead.enemyType.canDie = false;
            }
        }

        /// <summary>
        ///     Finds vanilla prefabs and grabs their textures or adds components to them.
        /// </summary>
        internal static void FindAndModifyPrefabs()
        {
            // Currently, only the Coilhead counterplay requires textures loaded at runtime.
            if (!MoreCounterplay.Settings.EnableCoilheadCounterplay) return;

            MoreCounterplay.Log("Loading prefab textures...");

            // Try to find and obtain the Coilhead's enemy prefab.
            if (VanillaPrefabUtils.GetInsideEnemyPrefab("Spring", out GameObject? coilheadPrefab))
            {
                // Save reference to vanilla Coilhead prefab for convenience.
                CoilExplosion.CoilheadPrefab = coilheadPrefab;

                if (HeadItem.Prefab != null && coilheadPrefab != null)
                {
                    // Modify Coilhead health to the configured amount and allow it to die.
                    SpringManAI coilhead = coilheadPrefab.GetComponent<SpringManAI>();
                    coilhead.enemyHP = MoreCounterplay.Settings.SpringDurability;
                    coilhead.enemyType.canDie = true;

                    if (MoreCounterplay.Settings.LoreAccurateCoilheads)
                    {
                        // Add explosion network behaviour script to the Coilhead prefab.
                        coilheadPrefab.AddComponent<CoilExplosion>().enabled = false;

                        // Add radioactive fire particle effects container to the Coilhead prefab.
                        CoilExplosion.RadioactiveFirePrefab?.transform.SetParent(coilheadPrefab.transform.Find("SpringManModel"), false);
                        CoilExplosion.RadioactiveFirePrefab?.SetActive(false);
                    }

                    if (MoreCounterplay.Settings.DropHeadAsScrap)
                    {
                        // Obtain Coilhead material from its enemy prefab and assign it to the 'Coilless Coilhead' scrap item prefab.
                        Material coilheadMaterial = coilheadPrefab.transform.Find("SpringManModel/Head").GetComponent<MeshRenderer>().material;
                        HeadItem.Prefab.GetComponent<MeshRenderer>().material = coilheadMaterial;
                    }
                }
                else
                {
                    MoreCounterplay.LogWarning("Either the 'Coilless Coilhead' prefab did not load or the Coilhead enemy prefab could not be found. "
                        + "Either way, some stuff might not load or work properly.");
                }
            }

            // Try to find and obtain the Forest Giant enemy prefab.
            if (MoreCounterplay.Settings.LoreAccurateCoilheads && VanillaPrefabUtils.GetOutsideEnemyPrefab("ForestGiant", out GameObject? giantPrefab))
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
        }
    }
}