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
using UnityEngine.UI;
using Dissonance;

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
            __instance.endgameStatsAnimator = UnityEngine.Object.Instantiate(__instance.endgameStatsAnimator);//lmao trolled
            // NOTE: __instance.playerLevels.Length is 5 by default!
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
            for (int j = 0; j < Plugin.MaxPlayers; j++)
            {
                __instance.playerVoiceMixers[j] = __instance.diageticMixer.outputAudioMixerGroup;

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
        [HarmonyPatch(typeof(NetworkSceneManager), "PopulateScenePlacedObjects")]
        [HarmonyPrefix]
        public static bool AddPlayers(NetworkSceneManager __instance)
        {
            startOfRound = StartOfRound.Instance;
            if (startOfRound.allPlayerObjects[Plugin.MaxPlayers - 1] != null)
            {
                return(true);
            }
            referencePlayer = startOfRound.allPlayerObjects[0].GetComponent<PlayerControllerB>();
            var playerPrefab = startOfRound.playerPrefab;
            var playerContainer = startOfRound.playersContainer.transform;
            FieldInfo GlobalObjectIdHash = (typeof(NetworkObject).GetField("GlobalObjectIdHash", BindingFlags.NonPublic | BindingFlags.Instance));
            PropertyInfo NetworkObjectId = (typeof(NetworkObject).GetProperty("NetworkObjectId", BindingFlags.Public | BindingFlags.Instance));
            FieldInfo SceneObjs = (typeof(NetworkSceneManager).GetField("ScenePlacedObjects", BindingFlags.NonPublic | BindingFlags.Instance));
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
                NetworkObjectId.SetValue(netObject, hash2);
                GlobalObjectIdHash.SetValue(plrphysbox, hash3);
                NetworkObjectId.SetValue(plrphysbox, hash6);
                GlobalObjectIdHash.SetValue(itemholder, hash4);
                NetworkObjectId.SetValue(itemholder, hash7);
                GlobalObjectIdHash.SetValue(itemholder2, hash5);
                NetworkObjectId.SetValue(itemholder2, hash8);
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
        [HarmonyPatch(typeof(QuickMenuManager), "AddUserToPlayerList")]
        [HarmonyPrefix]
        public static bool AddUserToPlayerList(QuickMenuManager __instance, ulong steamId, string playerName, int playerObjectId)
        {
            if (playerObjectId >= 0 && playerObjectId <= Plugin.MaxPlayers)
            {
                __instance.playerListSlots[playerObjectId].KickUserButton.SetActive(StartOfRound.Instance.IsServer);
                __instance.playerListSlots[playerObjectId].slotContainer.SetActive(value: true);
                __instance.playerListSlots[playerObjectId].isConnected = true;
                __instance.playerListSlots[playerObjectId].playerSteamId = steamId;
                __instance.playerListSlots[playerObjectId].usernameHeader.text = playerName.Replace("bizzlemip", "<color=#008282>bizzlemip</color>"); ;
                if (GameNetworkManager.Instance.localPlayerController != null)
                {
                    __instance.playerListSlots[playerObjectId].volumeSliderContainer.SetActive(playerObjectId != (int)GameNetworkManager.Instance.localPlayerController.playerClientId);
                }
            }
            return (false);
        }
        [HarmonyPatch(typeof(QuickMenuManager), "Update")]
        [HarmonyPrefix]
        private static bool Update(QuickMenuManager __instance)
        {
            for (int i = 0; i < __instance.playerListSlots.Length; i++)
            {
                if (__instance.playerListSlots[i].isConnected)
                {
                    float num = __instance.playerListSlots[i].volumeSlider.value / __instance.playerListSlots[i].volumeSlider.maxValue;
                    if (num == -1f)
                    {
                        SoundManager.Instance.playerVoiceVolumes[i] = -1f;//-70f;//dude what the fuck? why? dont do that (joke)
                    }
                    else
                    {
                        SoundManager.Instance.playerVoiceVolumes[i] = num;
                    }
                }
            }
            return (false); 
        }
        [HarmonyPatch(typeof(QuickMenuManager), "Start")]
        [HarmonyPrefix]

        public static bool FixPlayerList(ref QuickMenuManager __instance)
        {
            GameObject OldMask = null;
            GameObject ogparent = __instance.playerListPanel.transform.Find("Image").gameObject;
            if (ogparent.transform.Find("Mask"))
            {
                OldMask = ogparent.transform.Find("Mask").gameObject;
            }
            GameObject Mask = new GameObject("Mask");
            GameObject newFrame = new GameObject("ScrollViewport");
            GameObject bgCollision = new GameObject("BGCollision");
            GameObject newFrame2 = new GameObject("ScrollContent");
            Mask.transform.SetParent(ogparent.transform);
            newFrame.transform.SetParent(Mask.transform);
            bgCollision.transform.SetParent(newFrame.transform);
            newFrame2.transform.SetParent(newFrame.transform);
            Mask.transform.localScale = UnityEngine.Vector3.one;
            newFrame.transform.localScale = UnityEngine.Vector3.one;
            newFrame2.transform.localScale = UnityEngine.Vector3.one;
            Mask.AddComponent<RectTransform>().sizeDelta = new UnityEngine.Vector2(300, 280f);
            Mask.transform.localPosition = new UnityEngine.Vector3(-10, 110, 0);
            newFrame.transform.localPosition = new UnityEngine.Vector3(0, -10, 0);
            newFrame2.AddComponent<RectTransform>().pivot = new UnityEngine.Vector2(0.5f, 1);
            Mask.GetComponent<RectTransform>().pivot = new UnityEngine.Vector2(0.5f, 1);
            Mask.transform.localPosition = new UnityEngine.Vector3(-10, 110, 0);
            Mask.AddComponent<RectMask2D>();
            VerticalLayoutGroup VLG = newFrame2.AddComponent<VerticalLayoutGroup>();
            ContentSizeFitter CSF = newFrame2.AddComponent<ContentSizeFitter>();
            ScrollRect SR = newFrame.AddComponent<ScrollRect>();
            SR.viewport = newFrame.AddComponent<RectTransform>();
            SR.content = newFrame2.GetComponent<RectTransform>();
            SR.horizontal = false;
            UnityEngine.UI.Image image = bgCollision.AddComponent<UnityEngine.UI.Image>();
            bgCollision.GetComponent<RectTransform>().anchorMin = new UnityEngine.Vector2(0, 0);
            bgCollision.GetComponent<RectTransform>().anchorMax = new UnityEngine.Vector2(1, 1);
            image.color = new UnityEngine.Color(255, 255, 255, 0);
            VLG.spacing = 50;
            CSF.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
            CSF.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            __instance.playerListSlots = Helper.ResizeArray(__instance.playerListSlots, Plugin.MaxPlayers);
            for (int i = 0; i < Plugin.MaxPlayers; i++)
            {
                if (i < 4)
                {
                    __instance.playerListSlots[i].slotContainer.transform.SetParent(newFrame2.transform);
                }
                else
                {
                    PlayerListSlot NewSlot = new PlayerListSlot();
                    GameObject NewSlot2 = UnityEngine.Object.Instantiate(__instance.playerListSlots[0].slotContainer);
                    NewSlot.slotContainer = NewSlot2;
                    NewSlot.volumeSliderContainer = NewSlot2.transform.Find("VoiceVolumeSlider").gameObject;
                    NewSlot.KickUserButton = NewSlot2.transform.Find("KickButton").gameObject;
                    QuickMenuManager yeahoriginal = __instance;
                    int localI = i;
                    NewSlot.KickUserButton.GetComponent<UnityEngine.UI.Button>().onClick.AddListener(() => {
                        yeahoriginal.KickUserFromServer(localI);
                    });
                    NewSlot.isConnected = false;
                    NewSlot.usernameHeader = NewSlot2.transform.Find("PlayerNameButton").Find("PName").gameObject.GetComponent<TextMeshProUGUI>();
                    NewSlot.volumeSlider = NewSlot2.transform.Find("VoiceVolumeSlider").Find("Slider").gameObject.GetComponent<UnityEngine.UI.Slider>();
                    NewSlot.playerSteamId = __instance.playerListSlots[0].playerSteamId;
                    NewSlot2.transform.SetParent(newFrame2.transform,false);
                    __instance.playerListSlots[i] = NewSlot;
                }
                
            }
            if (OldMask != null){
                GameObject.Destroy(OldMask);
            }
            return (true);
        }

        [HarmonyPatch(typeof(StartOfRound), "SyncShipUnlockablesClientRpc")]
        [HarmonyPrefix]

        public static bool SyncShipUnlockablesClientRpc(StartOfRound __instance, int[] playerSuitIDs, bool shipLightsOn, UnityEngine.Vector3[] placeableObjectPositions, UnityEngine.Vector3[] placeableObjectRotations, int[] placeableObjects, int[] storedItems, int[] scrapValues, int[] itemSaveData)
        {
            Debug.Log("INITIAL ARRAY LENGTHS");
            Debug.Log(__instance.allPlayerScripts.Length);
            Debug.Log(playerSuitIDs.Length);
            return true;
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
            if (!instantiating) return (true);
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