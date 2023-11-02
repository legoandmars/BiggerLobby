using GameNetcodeStuff;
using HarmonyLib;
using Unity.Netcode;
using UnityEngine;
using System.Reflection;
using System;

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
        private static StartOfRound startOfRound;
        private static bool instantiating = false;
        private static int nextClientId = 0;
        private static PlayerControllerB referencePlayer;
        [HarmonyPatch(typeof(StartOfRound), "Start")]
        [HarmonyPrefix]
        public static void AddPlayers(ref StartOfRound __instance) {
            startOfRound = __instance;
            referencePlayer = __instance.allPlayerObjects[0].GetComponent<PlayerControllerB>();
            var playerPrefab = __instance.playerPrefab;
            var playerContainer = __instance.allPlayerObjects[0].transform.parent;
            var spawnMethod = typeof(NetworkSpawnManager).GetMethod(
                "SpawnNetworkObjectLocally",
                BindingFlags.Instance | BindingFlags.NonPublic,
                null,
                CallingConventions.Any,
                new Type[]{typeof(NetworkObject), typeof(ulong), typeof(bool), typeof(bool), typeof(ulong), typeof(bool)},
                null
            );
            instantiating = true;
            for (int i = 0; i < Plugin.MaxPlayers; i++)
            {
                nextClientId = i;
                var newPlayer = GameObject.Instantiate<GameObject>(playerPrefab, playerContainer);
                var newScript = newPlayer.GetComponent<PlayerControllerB>();
                var netObject = newPlayer.GetComponent<NetworkObject>();
                Debug.Log("[BigLobby] Trying to spawn new player");
                spawnMethod.Invoke(NetworkManager.Singleton.SpawnManager, new object[]{
                    netObject,
                    1234567890ul + (ulong)i,
                    true,
                    false,
                    netObject.OwnerClientId,
                    true
                });
            }
            instantiating = false;
        }
        [HarmonyPatch(typeof(PlayerControllerB), "Awake")]
        [HarmonyPrefix]
        public static void FixPlayerObject(ref PlayerControllerB __instance) {
            if (!instantiating) return;
            startOfRound.allPlayerObjects[nextClientId] = __instance.gameObject;
            startOfRound.allPlayerScripts[nextClientId] = __instance;
            __instance.gameObject.name = $"ExtraPlayer{nextClientId}";
            __instance.playerClientId = (ulong)nextClientId;
            var fields = typeof(PlayerControllerB).GetFields();
            foreach (FieldInfo field in fields) {
                var myValue = field.GetValue(__instance);
                var referenceValue = field.GetValue(referencePlayer);
                if (myValue == null && referenceValue != null)
                    field.SetValue(__instance, referenceValue);
            }
            __instance.enabled = true;
        }
    }
}