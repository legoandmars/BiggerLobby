using GameNetcodeStuff;
using System.Numerics;
using HarmonyLib;
using Unity.Netcode;
using UnityEngine;
using System.Reflection;
using System;
using System.Collections.Generic;
using Netcode.Transports.Facepunch;
using System.Linq;
using System.Reflection.Emit;
using UnityEngine.Audio;
using Steamworks.Ugc;
using UnityEngine.Assertions;
using TMPro;
using Steamworks.Data;
using Steamworks;
using BepInEx;
using UnityEngine.UIElements;
using System.Text.RegularExpressions;
using Dissonance.Integrations.Unity_NFGO;
using Unity.Collections;
using System.Runtime.CompilerServices;
using UnityEngine.SceneManagement;

namespace BiggerLobby.Patches
{
    [HarmonyPatch]
    internal class PlayerObjects
    {
        [HarmonyPatch(typeof(StartOfRound), "Awake")]
        [HarmonyPrefix]
        public static void ResizeLists(ref StartOfRound __instance)
        {
            __instance.allPlayerObjects = Helper.ResizeArray(__instance.allPlayerObjects, Plugin.MaxPlayers);
            __instance.allPlayerScripts = Helper.ResizeArray(__instance.allPlayerScripts, Plugin.MaxPlayers);
            __instance.gameStats.allPlayerStats = Helper.ResizeArray(__instance.gameStats.allPlayerStats, Plugin.MaxPlayers);
            __instance.playerSpawnPositions = Helper.ResizeArray(__instance.playerSpawnPositions, Plugin.MaxPlayers);
            for (int j = 4; j < Plugin.MaxPlayers; j++)
            {
                __instance.gameStats.allPlayerStats[j] = new PlayerStats();
                __instance.playerSpawnPositions[j] = __instance.playerSpawnPositions[0];
            }
        }
        [HarmonyPatch(typeof(ForestGiantAI), "Start")]
        [HarmonyPrefix]
        public static bool ResizeLists2(ref ForestGiantAI __instance)
        {
            __instance.playerStealthMeters = Helper.ResizeArray(__instance.playerStealthMeters, Plugin.MaxPlayers);
            return(true);
        }
        [HarmonyPatch(typeof(HUDManager), "Awake")]
        [HarmonyPostfix]
        public static void ResizeLists2(ref HUDManager __instance)
        {
            __instance.playerLevels = Helper.ResizeArray(__instance.playerLevels, Plugin.MaxPlayers);
            for (int j = 4; j < Plugin.MaxPlayers; j++)
            {
                __instance.playerLevels[j] = new PlayerLevel();
            }
        }

        [HarmonyPatch(typeof(SoundManager), "Awake")]
        [HarmonyPostfix]
        public static void SoundWake(ref SoundManager __instance)
        {
            __instance.playerVoiceMixers = Helper.ResizeArray(__instance.playerVoiceMixers, Plugin.MaxPlayers);
            for (int j = 4; j < Plugin.MaxPlayers; j++)
            {
                __instance.playerVoiceMixers[j] = UnityEngine.Object.Instantiate(__instance.playerVoiceMixers[0]);

            }
        }
        
        [HarmonyPatch(typeof(SoundManager), "Start")]
        [HarmonyPostfix]
        public static void ResizeSoundManagerLists(ref SoundManager __instance)
        {
            __instance. playerVoicePitchLerpSpeed = new float[Plugin.MaxPlayers + 1];
            __instance.playerVoicePitchTargets = new float[Plugin.MaxPlayers + 1];
            __instance.playerVoiceVolumes = new float[Plugin.MaxPlayers + 1];
            __instance.playerVoicePitches = new float[Plugin.MaxPlayers+1];
            for (int i = 1; i < Plugin.MaxPlayers+1; i++)
            {
                __instance.playerVoicePitchLerpSpeed[i] = 3f;
                __instance.playerVoicePitchTargets[i] = 1f;
                __instance.playerVoicePitches[i] = 1f;
                __instance.playerVoiceVolumes[i] = 1f;
            }
        }
        [HarmonyPatch(typeof(StartOfRound), "EndOfGame")]
        [HarmonyPrefix]
        public static void EOG(ref StartOfRound __instance, int bodiesInsured = 0, int connectedPlayersOnServer = 0)
        {
            Plugin.oldhastime = __instance.currentLevel.planetHasTime;
            __instance.currentLevel.planetHasTime = false;
        }
        [HarmonyPatch(typeof(StartOfRound), "ReviveDeadPlayers")]
        [HarmonyPrefix]
        public static void RDP(ref StartOfRound __instance)
        {
            __instance.currentLevel.planetHasTime = Plugin.oldhastime;
        }
        private static StartOfRound startOfRound;
        private static bool instantiating = false;
        private static int nextClientId = 0;
        private static PlayerControllerB referencePlayer;
        [HarmonyPatch(typeof(EnemyAI),"EnableEnemyMesh")]
        [HarmonyPrefix]
        public static bool EnableEnemyMesh(EnemyAI __instance, bool enable, bool overrideDoNotSet = false)
        {
            int layer = ((!enable) ? 23 : 19);
            for (int i = 0; i < __instance.skinnedMeshRenderers.Length; i++)
            {
                if (__instance.skinnedMeshRenderers[i] && (!__instance.skinnedMeshRenderers[i].CompareTag("DoNotSet") || overrideDoNotSet))
                {
                    __instance.skinnedMeshRenderers[i].gameObject.layer = layer;
                }
            }
            for (int j = 0; j < __instance.meshRenderers.Length; j++)
            {
                if (__instance.meshRenderers[j] && (!__instance.meshRenderers[j].CompareTag("DoNotSet") || overrideDoNotSet))
                {
                    __instance.meshRenderers[j].gameObject.layer = layer;
                }
            }
            return (false);
        }
        [HarmonyPatch(typeof(ShipTeleporter), "Awake")]
        [HarmonyPrefix]
        public static bool Awake2(ShipTeleporter __instance)
        {
            int[] playersBeingTeleported2 = new int[Plugin.MaxPlayers];
            for (int i = 0; i < Plugin.MaxPlayers; i++)
            {
                playersBeingTeleported2[i] = -1;
            }
            typeof(ShipTeleporter).GetField("playersBeingTeleported", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(__instance, playersBeingTeleported2);
            __instance.buttonTrigger.interactable = false;
            typeof(ShipTeleporter).GetField("cooldownTime", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(__instance, __instance.cooldownAmount);
            return false;
        }
        [HarmonyPatch(typeof(StartOfRound), "SceneManager_OnLoadComplete1")]
        [HarmonyPrefix]
        public static bool AddPlayers()
        {
            Debug.Log("Ran");
            NetworkSceneManager __instance = NetworkManager.Singleton.SceneManager;
            startOfRound = StartOfRound.Instance;
            if (startOfRound.allPlayerObjects[Plugin.MaxPlayers - 1] != null)
            {
                return(true);
            }
            Debug.Log("Adding players");
            referencePlayer = startOfRound.allPlayerObjects[0].GetComponent<PlayerControllerB>();
            var playerPrefab = startOfRound.playerPrefab;
            var playerContainer = startOfRound.playersContainer.transform;
            FieldInfo GlobalObjectIdHash = (typeof(NetworkObject).GetField("GlobalObjectIdHash", BindingFlags.NonPublic | BindingFlags.Instance));
            PropertyInfo NetworkObjectId = (typeof(NetworkObject).GetProperty("NetworkObjectId", BindingFlags.Public | BindingFlags.Instance));
            FieldInfo[] OBJFields = (typeof(NetworkObject).GetFields(BindingFlags.NonPublic | BindingFlags.Instance));
            FieldInfo NetManager = (typeof(NetworkSceneManager).GetField("NetworkManager", BindingFlags.NonPublic | BindingFlags.Instance));
            FieldInfo NetManager2 = (typeof(NetworkObject).GetField("NetworkManagerOwner", BindingFlags.NonPublic | BindingFlags.Instance));
            FieldInfo PrefabHandler = (typeof(NetworkManager).GetField("m_PrefabHandler", BindingFlags.NonPublic | BindingFlags.Instance));
            NetworkPrefabHandler PrefabHandler2 = PrefabHandler.GetValue(NetworkManager.Singleton) as NetworkPrefabHandler;
            FieldInfo SceneObject = NetManager2;

            foreach (FieldInfo Field in OBJFields)
            {
                if (Field.Name == "<IsSceneObject>k__BackingField")
                {
                    SceneObject = Field;
                }
            }
            instantiating = true;
            var spawnMethod = typeof(NetworkSpawnManager).GetMethod(
                "SpawnNetworkObjectLocally",
                BindingFlags.Instance | BindingFlags.NonPublic,
                null,
                CallingConventions.Any,
                new Type[] { typeof(NetworkObject), typeof(ulong), typeof(bool), typeof(bool), typeof(ulong), typeof(bool) },
                null
            );
            for (int i = 4; i < Plugin.MaxPlayers; i++)
            {
                
                nextClientId = i;
                var newPlayer = GameObject.Instantiate<GameObject>(playerPrefab, playerContainer);
                var newScript = newPlayer.GetComponent<PlayerControllerB>();
                var netObject = newPlayer.GetComponent<NetworkObject>();
                var plrphysbox = newPlayer.transform.Find("PlayerPhysicsBox").gameObject.GetComponent<NetworkObject>();
                var itemholder = newPlayer.transform.Find("ScavengerModel/metarig/ScavengerModelArmsOnly/metarig/spine.003/shoulder.R/arm.R_upper/arm.R_lower/hand.R/LocalItemHolder").gameObject.GetComponent<NetworkObject>();
                var itemholder2 = newPlayer.transform.Find("ScavengerModel/metarig/spine/spine.001/spine.002/spine.003/shoulder.R/arm.R_upper/arm.R_lower/hand.R/ServerItemHolder").gameObject.GetComponent<NetworkObject>();
                newScript.TeleportPlayer(StartOfRound.Instance.notSpawnedPosition.position);
                startOfRound.allPlayerObjects[i] = newPlayer;
                startOfRound.allPlayerScripts[i] = newScript;
                uint hash = 6942069u + (uint)i;
                ulong hash2 = 6942069ul + (ulong)i;
                uint hash3 = 123456789u + (uint)i;
                uint hash4 = 987654321u + (uint)i;
                uint hash5 = 124585949u + (uint)i;
                ulong hash6 = 123456789ul + (ulong)i;
                ulong hash7 = 987654321ul + (ulong)i;
                ulong hash8 = 124585949ul + (ulong)i;
                int handle = netObject.gameObject.scene.handle;
                GlobalObjectIdHash.SetValue(netObject, hash);
                GlobalObjectIdHash.SetValue(plrphysbox, hash3);
                GlobalObjectIdHash.SetValue(itemholder, hash4);
                GlobalObjectIdHash.SetValue(itemholder2, hash5);
                NetworkObjectId.SetValue(netObject, hash2);
                NetworkObjectId.SetValue(plrphysbox, hash6);
                NetworkObjectId.SetValue(itemholder, hash7);
                NetworkObjectId.SetValue(itemholder2, hash8);
                SceneObject.SetValue(netObject, true);
                NetManager2.SetValue(netObject, NetManager.GetValue(__instance));
                SceneObject.SetValue(plrphysbox, true);
                NetManager2.SetValue(plrphysbox, NetManager.GetValue(__instance));
                SceneObject.SetValue(itemholder, true);
                NetManager2.SetValue(itemholder, NetManager.GetValue(__instance));
                SceneObject.SetValue(itemholder2, true);
                NetManager2.SetValue(itemholder2, NetManager.GetValue(__instance));
                Plugin.CustomNetObjects.Add(hash, netObject);
                if (!netObject.IsSpawned) { 
                /*spawnMethod.Invoke(NetworkManager.Singleton.SpawnManager, new object[]{
                    netObject,
                    hash2,
                    true,//this needs to  be true or everything fucking shits itself. i think this might be the problem aswell. partciularly weird cuz apparently theres nested netobjs but i dont see any?
                    true,
                    netObject.OwnerClientId,
                    false
                });*/
                }

                ManualCameraRenderer[] deezlist = UnityEngine.Object.FindObjectsByType<ManualCameraRenderer>(FindObjectsInactive.Include, FindObjectsSortMode.None);
                for (int j = 0; j < deezlist.Length; j++)
                {
                    ManualCameraRenderer CR = deezlist[j];
                    CR.AddTransformAsTargetToRadar(newScript.transform, "Player #" + j.ToString(), false);
                }
            }
            instantiating = false;
            return (true);
        }
        [HarmonyPatch(typeof(QuickMenuManager), "Start")]
        [HarmonyPrefix]

        public static bool RemovePlayerlist(ref QuickMenuManager __instance)
        {
            __instance.playerListSlots = Helper.ResizeArray(__instance.playerListSlots, Plugin.MaxPlayers);
            for (int i = 4; i < Plugin.MaxPlayers; i++)
            {
                PlayerListSlot NewSlot = new PlayerListSlot();
                NewSlot.slotContainer = __instance.playerListSlots[0].slotContainer;
                NewSlot.volumeSliderContainer = __instance.playerListSlots[0].volumeSliderContainer;
                NewSlot.KickUserButton = __instance.playerListSlots[0].KickUserButton;
                NewSlot.isConnected = false;
                NewSlot.usernameHeader = __instance.playerListSlots[0].usernameHeader;
                NewSlot.volumeSlider = __instance.playerListSlots[0].volumeSlider;
                NewSlot.playerSteamId = __instance.playerListSlots[0].playerSteamId;
                __instance.playerListSlots[i] = NewSlot;
            }
            __instance.playerListPanel.SetActive(false);
            return (true);
        }
        [HarmonyPatch(typeof(ManualCameraRenderer), "Awake")]
        [HarmonyPrefix]

        public static bool Mawake(ref ManualCameraRenderer __instance)
        {
            for (int i = 0; i < 4; i++)
            {
                __instance.radarTargets.Add(new TransformAndName(StartOfRound.Instance.allPlayerScripts[i].transform, StartOfRound.Instance.allPlayerScripts[i].playerUsername));
            }
            __instance.targetTransformIndex = 0;
            __instance.targetedPlayer = StartOfRound.Instance.allPlayerScripts[0];
            return (false);
        }//I got a glock in my rari

        [HarmonyPatch(typeof(PlayerControllerB), "Awake")]
        [HarmonyPrefix]

        public static bool FixPlayerObject(ref PlayerControllerB __instance)
        {
            if (!instantiating) return(true);
            __instance.gameObject.name = $"ExtraPlayer{nextClientId}";
            __instance.playerClientId = (ulong)nextClientId;
            __instance.actualClientId = (ulong)nextClientId;

            StartOfRound.Instance.allPlayerObjects[nextClientId] = __instance.transform.parent.gameObject;
            StartOfRound.Instance.allPlayerScripts[nextClientId] = __instance;
            var fields = typeof(PlayerControllerB).GetFields();
            foreach (FieldInfo field in fields) 
            {
                var myValue = field.GetValue(__instance);
                var referenceValue = field.GetValue(referencePlayer);
                if (myValue == null && referenceValue != null)
                    field.SetValue(__instance, referenceValue);
            }
            __instance.enabled = true;
            return (true);
        }

        [HarmonyPatch(typeof(StartOfRound), "GetPlayerSpawnPosition")]
        [HarmonyTranspiler]

        public static IEnumerable<CodeInstruction> GetPlayerSpawnPosition(IEnumerable<CodeInstruction> instructions)
        {
            var codes = new List<CodeInstruction>(instructions);
            codes[0].opcode = OpCodes.Ldc_I4_1;
            return codes.AsEnumerable();
        }
    }
}
