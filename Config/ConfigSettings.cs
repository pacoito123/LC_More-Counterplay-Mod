using BepInEx.Configuration;
using System.Collections.Generic;
using System.IO;

namespace MoreCounterplay.Config
{
    public static class ConfigSettings
    {
        #region Variables
        #region Jester
        public static ConfigEntry<bool> EnableJesterCounterplay;
        public static ConfigEntry<float> WeightToPreventJester;
        #endregion

        #region Turret
        public static ConfigEntry<bool> EnableTurretCounterplay;
        #endregion

        #region Coilhead
        public static ConfigEntry<bool> EnableCoilheadCounterplay;
        public static ConfigEntry<int> SpringDurability;
        public static ConfigEntry<int> CoilheadDefaultDamage;
        public static ConfigEntry<int> CoilheadKnifeDamage;
        public static ConfigEntry<int> CoilheadShovelDamage;
        public static ConfigEntry<bool> DropHeadAsScrap;
        public static ConfigEntry<int> MinHeadValue;
        public static ConfigEntry<int> MaxHeadValue;
        public static ConfigEntry<bool> LoreAccurateCoilheads;
        public static ConfigEntry<int> ExplosionDamage;
        public static ConfigEntry<float> ExplosionDamageRadius;
        public static ConfigEntry<float> ExplosionKillRadius;
        public static ConfigEntry<float> MinExplosionTimer;
        public static ConfigEntry<float> MaxExplosionTimer;
        public static ConfigEntry<bool> ExplosionDestroysHead;
        #endregion
        #endregion

        public static Dictionary<string, ConfigEntryBase> currentConfigEntries = [];

        public static void BindConfigSettings()
        {
            MoreCounterplay.Log("Binding Configs");

            #region Jester
            EnableJesterCounterplay = AddConfigEntry(MoreCounterplay.Instance.Config.Bind("Server-side", "EnableJesterCounterplay", true, "Add counterplay for Jester."));
            WeightToPreventJester = AddConfigEntry(MoreCounterplay.Instance.Config.Bind("Server-side", "WeightToPreventJester", 30f, "Weight of items needed to prevent Jester pop out."));
            #endregion

            #region Turret
            EnableTurretCounterplay = AddConfigEntry(MoreCounterplay.Instance.Config.Bind("Server-side", "EnableTurretCounterplay", true, "Add counterplay for Turret."));
            #endregion

            #region Coilhead
            EnableCoilheadCounterplay = AddConfigEntry(MoreCounterplay.Instance.Config.Bind("Server-side", "EnableCoilheadCounterplay", true, "Add counterplay for Coilheads (requires restart). Required by settings below."));
            SpringDurability = AddConfigEntry(MoreCounterplay.Instance.Config.Bind("Server-side", "SpringDurability", 3, "Set Coilhead health points (requires restart)."));
            CoilheadDefaultDamage = AddConfigEntry(MoreCounterplay.Instance.Config.Bind("Server-side", "CoilheadDefaultDamage", 0, "Amount of damage that Coilheads take from any source not specified below."));
            CoilheadKnifeDamage = AddConfigEntry(MoreCounterplay.Instance.Config.Bind("Server-side", "CoilheadKnifeDamage", 1, "Amount of damage that Coilheads take from Knife."));
            CoilheadShovelDamage = AddConfigEntry(MoreCounterplay.Instance.Config.Bind("Server-side", "CoilheadShovelDamage", 0, "Amount of damage that Coilheads take from Shovel."));
            DropHeadAsScrap = AddConfigEntry(MoreCounterplay.Instance.Config.Bind("Server-side", "DropHeadAsScrap", true, "Enable the Coilhead head scrap item spawning on death (requires restart)."));
            MinHeadValue = AddConfigEntry(MoreCounterplay.Instance.Config.Bind("Server-side", "MinHeadValue", 30, "Minimum value of the Coilhead head item."));
            MaxHeadValue = AddConfigEntry(MoreCounterplay.Instance.Config.Bind("Server-side", "MaxHeadValue", 70, "Maximum value of the Coilhead head item."));
            LoreAccurateCoilheads = AddConfigEntry(MoreCounterplay.Instance.Config.Bind("Server-side", "LoreAccurateCoilheads", true, "Enable lore accurate (volatile) Coilhead counterplay (requires restart). Required by settings below."));
            ExplosionDamage = AddConfigEntry(MoreCounterplay.Instance.Config.Bind("Server-side", "ExplosionDamage", 50, "Amount of damage the Coilhead explosion deals."));
            ExplosionDamageRadius = AddConfigEntry(MoreCounterplay.Instance.Config.Bind("Server-side", "ExplosionDamageRadius", 4f, "Radius of the Coilhead explosion damage zone."));
            ExplosionKillRadius = AddConfigEntry(MoreCounterplay.Instance.Config.Bind("Server-side", "ExplosionKillRadius", 2f, "Radius of the Coilhead explosion kill zone."));
            MinExplosionTimer = AddConfigEntry(MoreCounterplay.Instance.Config.Bind("Server-side", "MinExplosionTimer", 0.5f, "Minimum time until Coilhead explosion."));
            MaxExplosionTimer = AddConfigEntry(MoreCounterplay.Instance.Config.Bind("Server-side", "MaxExplosionTimer", 5f, "Maximum time until Coilhead explosion."));
            ExplosionDestroysHead = AddConfigEntry(MoreCounterplay.Instance.Config.Bind("Server-side", "ExplosionDestroysHead", true, "Destroy Coilhead scrap head if still attached during explosion."));
            #endregion

            TryRemoveOldConfigSettings();
        }

        public static ConfigEntry<T> AddConfigEntry<T>(ConfigEntry<T> configEntry)
        {
            currentConfigEntries.Add(configEntry.Definition.Key, configEntry);
            return configEntry;
        }

        public static void TryRemoveOldConfigSettings()
        {
            HashSet<string> headers = [];
            HashSet<string> keys = [];

            foreach (ConfigEntryBase entry in currentConfigEntries.Values)
            {
                headers.Add(entry.Definition.Section);
                keys.Add(entry.Definition.Key);
            }

            try
            {
                ConfigFile config = MoreCounterplay.Instance.Config;
                string filepath = config.ConfigFilePath;

                if (File.Exists(filepath))
                {
                    string contents = File.ReadAllText(filepath);
                    string[] lines = File.ReadAllLines(filepath);

                    string currentHeader = "";

                    for (int i = 0; i < lines.Length; i++)
                    {
                        lines[i] = lines[i].Replace("\n", "");
                        if (lines[i].Length <= 0)
                            continue;

                        if (lines[i].StartsWith("["))
                        {
                            if (currentHeader != "" && !headers.Contains(currentHeader))
                            {
                                currentHeader = "[" + currentHeader + "]";
                                int index0 = contents.IndexOf(currentHeader);
                                int index1 = contents.IndexOf(lines[i]);
                                contents = contents.Remove(index0, index1 - index0);
                            }
                            currentHeader = lines[i].Replace("[", "").Replace("]", "").Trim();
                        }

                        else if (currentHeader != "")
                        {
                            if (i <= (lines.Length - 4) && lines[i].StartsWith("##"))
                            {
                                int numLinesEntry = 1;
                                while (i + numLinesEntry < lines.Length && lines[i + numLinesEntry].Length > 3)
                                    numLinesEntry++;

                                if (headers.Contains(currentHeader))
                                {
                                    int indexAssignOperator = lines[i + numLinesEntry - 1].IndexOf("=");
                                    string key = lines[i + numLinesEntry - 1].Substring(0, indexAssignOperator - 1);
                                    if (!keys.Contains(key))
                                    {
                                        int index0 = contents.IndexOf(lines[i]);
                                        int index1 = contents.IndexOf(lines[i + numLinesEntry - 1]) + lines[i + numLinesEntry - 1].Length;
                                        contents = contents.Remove(index0, index1 - index0);
                                    }
                                }
                                i += numLinesEntry - 1;
                            }
                            else if (lines[i].Length > 3)
                                contents = contents.Replace(lines[i], "");
                        }
                    }

                    if (!headers.Contains(currentHeader))
                    {
                        currentHeader = "[" + currentHeader + "]";
                        int index0 = contents.IndexOf(currentHeader);
                        contents = contents.Remove(index0, contents.Length - index0);
                    }

                    while (contents.Contains("\n\n\n"))
                        contents = contents.Replace("\n\n\n", "\n\n");

                    File.WriteAllText(filepath, contents);
                    config.Reload();
                }
            }
            catch { }
        }
    }
}