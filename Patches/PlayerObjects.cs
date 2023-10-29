using GameNetcodeStuff;
using HarmonyLib;
using Unity.Netcode;
using UnityEngine;
using System.Reflection;

namespace BigLobby.Patches
{
    [HarmonyPatch]
    internal class PlayerObjects
    {
        [HarmonyPatch(typeof(StartOfRound), "Awake")]
        [HarmonyPostfix]
        public static void ResizeLists(ref StartOfRound __instance) {
            __instance.allPlayerObjects = Helper.ResizeArray(__instance.allPlayerObjects, Plugin.MaxPlayers);
            __instance.allPlayerScripts = Helper.ResizeArray(__instance.allPlayerScripts, Plugin.MaxPlayers);
        }
        [HarmonyPatch(typeof(SoundManager), "Start")]
        [HarmonyPostfix]
        public static void ResizeSoundManagerLists(ref float[] ___playerVoicePitchLerpSpeed, ref float[] ___playerVoicePitchTargets, ref float[] ___playerVoicePitches) {
            ___playerVoicePitchLerpSpeed = new float[Plugin.MaxPlayers];
            ___playerVoicePitchTargets = new float[Plugin.MaxPlayers];
            ___playerVoicePitches = new float[Plugin.MaxPlayers];
            for (int i = 0; i < Plugin.MaxPlayers; i++)
            {
                ___playerVoicePitchLerpSpeed[i] = 3f;
                ___playerVoicePitchTargets[i] = 1f;
                ___playerVoicePitches[i] = 1f;
            }
        }
        [HarmonyPatch(typeof(StartOfRound), "Start")]
        [HarmonyPrefix]
        public static void AddPlayers(ref StartOfRound __instance) {
            var playerPrefab = __instance.allPlayerObjects[1];//__instance.playerPrefab;
            var playerContainer = __instance.allPlayerObjects[1].transform.parent;
            for (int i = 0; i < Plugin.MaxPlayers; i++)
            {
                var newPlayer = Object.Instantiate<GameObject>(playerPrefab, playerContainer);
                var newScript = newPlayer.GetComponent<PlayerControllerB>();
                var netObject = newPlayer.GetComponent<NetworkObject>();
                __instance.allPlayerObjects[i] = newPlayer;
                __instance.allPlayerScripts[i] = newScript;
                newPlayer.name = $"ExtraPlayer{i}";
                newScript.playersManager = __instance;
                newScript.playerClientId = (ulong)i;
                newScript.enabled = true;
                var idProperty = typeof(NetworkObject).GetProperty("NetworkObjectId");
                idProperty.SetValue(netObject, (ulong)(35 + i));
            }
        }
    }
}