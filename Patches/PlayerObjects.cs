using GameNetcodeStuff;
using HarmonyLib;
using Unity.Netcode;
using UnityEngine;

namespace BigLobby.Patches
{
    internal class PlayerObjects
    {
        [HarmonyPatch(typeof(StartOfRound), "Awake")]
        [HarmonyPostfix]
        public static void ResizeLists(ref StartOfRound __instance) {
            __instance.allPlayerObjects = Helper.ResizeArray(__instance.allPlayerObjects, Plugin.MaxPlayers);
            __instance.allPlayerScripts = Helper.ResizeArray(__instance.allPlayerScripts, Plugin.MaxPlayers);

            var playerPrefab = __instance.playerPrefab;
            var playerContainer = __instance.allPlayerObjects[1].transform.parent;
            if (GameNetworkManager.Instance.isHostingGame)
            { // We are the host, spawn extra players
                for (int i = 4; i < Plugin.MaxPlayers; i++)
                {
                    var newPlayer = Object.Instantiate<GameObject>(playerPrefab, playerContainer);
                    var newScript = newPlayer.GetComponent<PlayerControllerB>();
                    var netObject = newPlayer.GetComponent<NetworkObject>();

                    __instance.allPlayerObjects[i] = newPlayer;
                    __instance.allPlayerScripts[i] = newScript;
                    newPlayer.name = $"ExtraPlayer{i}";
                    newScript.playerClientId = (ulong)i;
                    netObject.Spawn();
                }
            }
            else // We are the client, look for extra players
            {
                var scripts = Object.FindObjectsOfType<PlayerControllerB>();
                foreach (var script in scripts)
                {
                    if (script.playerClientId < 4) continue;
                    var player = script.gameObject;
                    var index = ulong.Parse(player.name[-1].ToString());
                    script.playerClientId = index;
                    __instance.allPlayerObjects[index] = player;
                    __instance.allPlayerScripts[index] = script;
                }
            }
        }
    }
}