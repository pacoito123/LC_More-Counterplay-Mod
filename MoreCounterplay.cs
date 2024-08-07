using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using MoreCounterplay.Config;
using MoreCounterplay.MonoBehaviours;
using System.IO;
using System.Reflection;
using Unity.Netcode;
using UnityEngine;

namespace MoreCounterplay;

[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
[BepInDependency(LethalLib.Plugin.ModGUID, BepInDependency.DependencyFlags.HardDependency)]
public class MoreCounterplay : BaseUnityPlugin
{
    public static MoreCounterplay Instance { get; private set; } = null!;
    internal new static ManualLogSource Logger { get; private set; } = null!;
    internal static Harmony? Harmony { get; set; }


    #region Assets
    public static AssetBundle? Bundle;
    public static GameObject NetworkHandlerPreafab { get; private set; } = null!;
    public static Item HeadItem { get; private set; } = null!;
    public static Item TurretGunItem { get; private set; } = null!;
    #endregion

    public static bool IsHostOrServer => NetworkManager.Singleton.IsHost || NetworkManager.Singleton.IsServer;

    private void Awake()
    {
        Logger = base.Logger;
        Instance = this;

        LoadAssets();
        ConfigSettings.BindConfigSettings();
        Patch();
        NetcodePatcherAwake();

        Logger.LogInfo($"{MyPluginInfo.PLUGIN_GUID} v{MyPluginInfo.PLUGIN_VERSION} has loaded!");
    }

    private void NetcodePatcherAwake()
    {
        Log($"Patching Netcode");
        var types = Assembly.GetExecutingAssembly().GetTypes();

        foreach (var type in types)
        {
            var methods = type.GetMethods(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
            foreach (var method in methods)
            {
                var attributes = method.GetCustomAttributes(typeof(RuntimeInitializeOnLoadMethodAttribute), false);

                if (attributes.Length > 0)
                {
                    method.Invoke(null, null);
                }
            }
        }
    }

    private void LoadAssets()
    {
        Log($"Loading Assets");
        Bundle = AssetBundle.LoadFromFile(Path.Combine(Path.GetDirectoryName(Info.Location), "morecounterplayassets"));

        NetworkHandlerPreafab = Bundle.LoadAsset<GameObject>("NetworkHandler");
        HeadItem = Bundle.LoadAsset<Item>("Head.asset");
        TurretGunItem = Bundle.LoadAsset<Item>("TurretGun.asset");

        NetworkHandlerPreafab.AddComponent<PluginNetworkBehaviour>();

        // Head size, position, and rotation adjustments.
        HeadItem.itemSpawnsOnGround = false;
        HeadItem.restingRotation = new Vector3(-90, 0, 90);
        HeadItem.spawnPrefab.transform.localScale = new Vector3(0.1763f, 0.1763f, 0.1763f);
        HeadItem.spawnPrefab.transform.rotation *= Quaternion.Euler(-90f, 0f, 0f);
        HeadItem.verticalOffset = 0.1f;

        LethalLib.Modules.Items.RegisterItem(HeadItem); // Register as a plain item to persist when reloading the save file.
        LethalLib.Modules.NetworkPrefabs.RegisterNetworkPrefab(NetworkHandlerPreafab);
        LethalLib.Modules.NetworkPrefabs.RegisterNetworkPrefab(HeadItem.spawnPrefab);
        LethalLib.Modules.NetworkPrefabs.RegisterNetworkPrefab(TurretGunItem.spawnPrefab);
    }

    internal static void Patch()
    {
        Log($"Patching");
        Harmony ??= new Harmony(MyPluginInfo.PLUGIN_GUID);
        Harmony.PatchAll();
    }

    public static void Log(string message) => Logger.LogInfo(message);
    public static void LogError(string message) => Logger.LogError(message);
    public static void LogWarning(string message) => Logger.LogWarning(message);
    public static bool IsModLoaded(string guid) => BepInEx.Bootstrap.Chainloader.PluginInfos.ContainsKey(guid);
}
