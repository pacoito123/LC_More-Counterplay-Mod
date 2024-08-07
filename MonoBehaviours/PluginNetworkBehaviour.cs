using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Unity.Netcode;
using UnityEngine;

namespace MoreCounterplay.MonoBehaviours
{
    public class PluginNetworkBehaviour : NetworkBehaviour
    {
        public static PluginNetworkBehaviour Instance { get; private set; } = null!;

        public override void OnNetworkSpawn()
        {
            if (NetworkManager.Singleton.IsHost || NetworkManager.Singleton.IsServer)
                Instance?.gameObject.GetComponent<NetworkObject>().Despawn();
            Instance = this;

            base.OnNetworkSpawn();
        }

        [ClientRpc]
        public void DisableTurretGunClientRpc(NetworkObjectReference turretReference)
        {
            if (IsHost || IsServer) return;
            MoreCounterplay.Log($"Disable turret ClientRpc");

            if (turretReference.TryGet(out NetworkObject turretObject))
            {
                DisableTurretGunOnLocalClient(turretObject);
            }
            else
            {
                MoreCounterplay.Log($"Turret not found");
            }
        }

        public void DisableTurretGunOnLocalClient(NetworkObject turretReference)
        {
            turretReference.transform.parent.GetComponentsInChildren<Transform>().First(child => child.name == "GunBody").gameObject.SetActive(false);
        }
    }
}
