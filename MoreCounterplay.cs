using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using MoreCounterplay.Behaviours;
using MoreCounterplay.Config;
using MoreCounterplay.Items;
using System;
using System.IO;
using System.Reflection;
using UnityEngine;

namespace MoreCounterplay;

[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
[BepInDependency(LethalLib.Plugin.ModGUID, BepInDependency.DependencyFlags.HardDependency)]
public class MoreCounterplay : BaseUnityPlugin
{
    public static MoreCounterplay Instance { get; private set; } = null!;
    internal new static ManualLogSource Logger { get; private set; } = null!;
    internal static Harmony? Harmony { get; set; }

    public static AssetBundle? Bundle;

    private void Awake()
    {
        Logger = base.Logger;
        Instance = this;

        NetcodePatcher();
        LoadAssets();
        ConfigSettings.BindConfigSettings();
        Patch();

        Logger.LogInfo($"{MyPluginInfo.PLUGIN_GUID} v{MyPluginInfo.PLUGIN_VERSION} has loaded!");
    }

    private void LoadAssets()
    {
        Log($"Loading Assets...");
        Bundle = AssetBundle.LoadFromFile(Path.Combine(Path.GetDirectoryName(Info.Location), "morecounterplayassets"));

        // Load 'Coilless Coilhead' prefab.
        Item headProperties = Bundle.LoadAsset<Item>("CoillessCoilhead.asset");

        // Add HeadItem component to 'Coilless Coilhead' prefab.
        headProperties.spawnPrefab.AddComponent<HeadItem>().itemProperties = headProperties;
        HeadItem.Prefab = headProperties.spawnPrefab;

        // Load radioactive fire particle effects prefab.
        CoilExplosion.RadioactiveFirePrefab = Bundle.LoadAsset<GameObject>("RadioactiveFire.prefab");

        // Register 'Coilless Coilhead' as both a plain item and a network object.
        LethalLib.Modules.Items.RegisterItem(headProperties);
        LethalLib.Modules.NetworkPrefabs.RegisterNetworkPrefab(headProperties.spawnPrefab);
    }

    internal static void Patch()
    {
        Harmony ??= new Harmony(MyPluginInfo.PLUGIN_GUID);
        Harmony.PatchAll();
    }

    private static void NetcodePatcher()
    {
        foreach (Type type in Assembly.GetExecutingAssembly().GetTypes())
        {
            foreach (MethodInfo method in type.GetMethods(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static))
            {
                if (method.GetCustomAttributes(typeof(RuntimeInitializeOnLoadMethodAttribute), false).Length > 0)
                {
                    method.Invoke(null, null);
                }
            }
        }
    }

    public static void Log(string message) => Logger.LogInfo(message);
    public static void LogError(string message) => Logger.LogError(message);
    public static void LogWarning(string message) => Logger.LogWarning(message);
    public static bool IsModLoaded(string guid) => BepInEx.Bootstrap.Chainloader.PluginInfos.ContainsKey(guid);
}
