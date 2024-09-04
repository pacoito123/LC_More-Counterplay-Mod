using BepInEx.Configuration;
using CSync.Extensions;
using CSync.Lib;
using HarmonyLib;
using MoreCounterplay.Patches;
using System.Collections.Generic;
using System.Reflection;

namespace MoreCounterplay.Config
{
    public class ConfigSettings : SyncedConfig2<ConfigSettings>
    {
        #region Variables
        #region Jester
        [field: SyncedEntryField] public SyncedEntry<bool> EnableJesterCounterplay { get; private set; }
        [field: SyncedEntryField] public SyncedEntry<float> WeightToPreventJester { get; private set; }
        #endregion

        #region Turret
        [field: SyncedEntryField] public SyncedEntry<bool> EnableTurretCounterplay { get; private set; }
        #endregion

        #region Coilhead
        [field: SyncedEntryField] public SyncedEntry<bool> EnableCoilheadCounterplay { get; private set; }
        [field: SyncedEntryField] public SyncedEntry<int> SpringDurability { get; private set; }
        [field: SyncedEntryField] public SyncedEntry<int> CoilheadDefaultDamage { get; private set; }
        [field: SyncedEntryField] public SyncedEntry<int> CoilheadKnifeDamage { get; private set; }
        [field: SyncedEntryField] public SyncedEntry<int> CoilheadShovelDamage { get; private set; }
        [field: SyncedEntryField] public SyncedEntry<bool> DropHeadAsScrap { get; private set; }
        [field: SyncedEntryField] public SyncedEntry<int> MinHeadValue { get; private set; }
        [field: SyncedEntryField] public SyncedEntry<int> MaxHeadValue { get; private set; }
        [field: SyncedEntryField] public SyncedEntry<bool> LoreAccurateCoilheads { get; private set; }
        [field: SyncedEntryField] public SyncedEntry<int> ExplosionDamage { get; private set; }
        [field: SyncedEntryField] public SyncedEntry<float> ExplosionDamageRadius { get; private set; }
        [field: SyncedEntryField] public SyncedEntry<float> ExplosionKillRadius { get; private set; }
        [field: SyncedEntryField] public SyncedEntry<float> MinExplosionTimer { get; private set; }
        [field: SyncedEntryField] public SyncedEntry<float> MaxExplosionTimer { get; private set; }
        [field: SyncedEntryField] public SyncedEntry<bool> ExplosionDestroysHead { get; private set; }
        #endregion
        #endregion

        public ConfigSettings(ConfigFile config) : base(MyPluginInfo.PLUGIN_GUID)
        {
            // Disable saving config after a call to 'Bind()' is made.
            config.SaveOnConfigSet = false;

            #region Jester
            EnableJesterCounterplay = config.BindSyncedEntry("Server-side", "EnableJesterCounterplay", true, "Add counterplay for Jester.");
            WeightToPreventJester = config.BindSyncedEntry("Server-side", "WeightToPreventJester", 30f, "Weight of items needed to prevent Jester pop out.");
            #endregion

            #region Turret
            EnableTurretCounterplay = config.BindSyncedEntry("Server-side", "EnableTurretCounterplay", true, "Add counterplay for Turret.");
            #endregion

            #region Coilhead
            EnableCoilheadCounterplay = config.BindSyncedEntry("Server-side", "EnableCoilheadCounterplay", true, "Add counterplay for Coilheads (requires restart). Required by settings below.");
            SpringDurability = config.BindSyncedEntry("Server-side", "SpringDurability", 3, "Set Coilhead health points (requires restart).");
            CoilheadDefaultDamage = config.BindSyncedEntry("Server-side", "CoilheadDefaultDamage", 0, "Amount of damage that Coilheads take from any source not specified below.");
            CoilheadKnifeDamage = config.BindSyncedEntry("Server-side", "CoilheadKnifeDamage", 1, "Amount of damage that Coilheads take from Knife.");
            CoilheadShovelDamage = config.BindSyncedEntry("Server-side", "CoilheadShovelDamage", 0, "Amount of damage that Coilheads take from Shovel.");
            DropHeadAsScrap = config.BindSyncedEntry("Server-side", "DropHeadAsScrap", true, "Enable the Coilhead head scrap item spawning on death (requires restart).");
            MinHeadValue = config.BindSyncedEntry("Server-side", "MinHeadValue", 30, "Minimum value of the Coilhead head item.");
            MaxHeadValue = config.BindSyncedEntry("Server-side", "MaxHeadValue", 70, "Maximum value of the Coilhead head item.");

            LoreAccurateCoilheads = config.BindSyncedEntry("Server-side", "LoreAccurateCoilheads", true, "Enable lore accurate (volatile) Coilhead counterplay (requires restart). Required by settings below.");
            ExplosionDamage = config.BindSyncedEntry("Server-side", "ExplosionDamage", 50, "Amount of damage the Coilhead explosion deals.");
            ExplosionDamageRadius = config.BindSyncedEntry("Server-side", "ExplosionDamageRadius", 4f, "Radius of the Coilhead explosion damage zone.");
            ExplosionKillRadius = config.BindSyncedEntry("Server-side", "ExplosionKillRadius", 2f, "Radius of the Coilhead explosion kill zone.");
            MinExplosionTimer = config.BindSyncedEntry("Server-side", "MinExplosionTimer", 0.5f, "Minimum time until Coilhead explosion.");
            MaxExplosionTimer = config.BindSyncedEntry("Server-side", "MaxExplosionTimer", 5f, "Maximum time until Coilhead explosion.");
            ExplosionDestroysHead = config.BindSyncedEntry("Server-side", "ExplosionDestroysHead", true, "Destroy Coilhead scrap head if still attached during explosion.");
            #endregion

            // Function to run after configuration is synced (upon joining lobby).
            InitialSyncCompleted += new((_, _) => LoadPatches.FindAndModifyPrefabs());

            // Remove old config settings.
            ClearOrphanedEntries(config);

            // Re-enable saving and save config.
            config.SaveOnConfigSet = true;
            config.Save();

            // Register config with 'CSync'.
            ConfigManager.Register(this);
        }

        /// <summary>
        ///     Remove old (orphaned) configuration entries.
        /// </summary>
        /// <remarks>Obtained from: https://lethal.wiki/dev/intermediate/custom-configs#better-configuration</remarks>
        /// <param name="config">The config file to clear orphaned entries from.</param>
        private void ClearOrphanedEntries(ConfigFile config)
        {
            // Obtain 'OrphanedEntries' dictionary from ConfigFile through reflection.
            PropertyInfo orphanedEntriesProp = AccessTools.Property(typeof(ConfigFile), "OrphanedEntries");
            Dictionary<ConfigDefinition, string>? orphanedEntries = (Dictionary<ConfigDefinition, string>?)orphanedEntriesProp.GetValue(config);

            // Clear orphaned entries.
            orphanedEntries?.Clear();
        }
    }
}