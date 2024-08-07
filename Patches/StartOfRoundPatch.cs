using HarmonyLib;
using UnityEngine;
using Unity.Netcode;

namespace MoreCounterplay.Patches
{
    [HarmonyPatch(typeof(StartOfRound))]
    internal class StartOfRoundPatch
    {
        [HarmonyPatch("Awake")]
        [HarmonyPostfix]
        static void SpawnNetworkHandler()
        {
            if (NetworkManager.Singleton.IsHost || NetworkManager.Singleton.IsServer)
            {
                var networkHandler = Object.Instantiate(MoreCounterplay.NetworkHandlerPreafab, Vector3.zero, Quaternion.identity);
                networkHandler.GetComponent<NetworkObject>().Spawn();
            }
        }
    }
}
