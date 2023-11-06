using GameNetcodeStuff;
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

namespace BigLobby.Patches
{
    [HarmonyPatch]
    internal class PlayerObjects
    {
        [HarmonyPatch(typeof(StartOfRound), "Awake")]
        [HarmonyPostfix]
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
            Debug.Log(__instance.playerSpawnPositions.Length);
            Debug.Log("Yeahg");
            Debug.Log(__instance.allPlayerScripts.Length);
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
        [HarmonyPatch(typeof(MenuManager), "OnEnable")]
        [HarmonyPostfix]
        public static void CustomMenu(ref MenuManager __instance)
        {
            if (__instance.isInitScene)
            {
                return;
            }
            GameObject p = __instance.HostSettingsOptionsNormal.transform.parent.parent.gameObject;
            RectTransform rt = p.GetComponent(typeof(RectTransform)) as RectTransform;
            GameObject p2 = p.transform.Find("PrivatePublicDescription").gameObject;
            RectTransform rt2 = p2.GetComponent(typeof(RectTransform)) as RectTransform;
            GameObject p3 = __instance.HostSettingsOptionsNormal.transform.Find("EnterAName").gameObject;
            RectTransform rt3 = p3.GetComponent(typeof(RectTransform)) as RectTransform;
            GameObject p4 = __instance.HostSettingsOptionsNormal.transform.Find("ServerNameField").gameObject;
            RectTransform rt4 = p4.GetComponent(typeof(RectTransform)) as RectTransform;
            GameObject p5 = p.transform.Find("Confirm").gameObject;
            RectTransform rt5 = p5.GetComponent(typeof(RectTransform)) as RectTransform;
            GameObject p6 = p.transform.Find("Back").gameObject;
            RectTransform rt6 = p6.GetComponent(typeof(RectTransform)) as RectTransform;
            GameObject p7 = __instance.HostSettingsOptionsNormal.transform.Find("Public").gameObject;
            RectTransform rt7 = p7.GetComponent(typeof(RectTransform)) as RectTransform;
            GameObject p8 = __instance.HostSettingsOptionsNormal.transform.Find("Private").gameObject;
            RectTransform rt8 = p8.GetComponent(typeof(RectTransform)) as RectTransform;
            GameObject p9 = UnityEngine.Object.Instantiate(p4,p4.transform.parent);
            RectTransform rt9 = p9.GetComponent(typeof(RectTransform)) as RectTransform;
            Debug.Log("yeah!!");
            rt.sizeDelta = new Vector2(rt.sizeDelta.x, 200);
            rt2.anchoredPosition = new Vector2(rt2.anchoredPosition.x, -50);
            rt3.anchoredPosition = new Vector2(rt3.anchoredPosition.x, 40);
            rt4.anchoredPosition = new Vector2(rt4.anchoredPosition.x, 55);
            rt5.anchoredPosition = new Vector2(rt5.anchoredPosition.x, -60);
            rt6.anchoredPosition = new Vector2(rt6.anchoredPosition.x, -85);
            rt7.anchoredPosition = new Vector2(rt7.anchoredPosition.x, -23);
            rt8.anchoredPosition = new Vector2(rt8.anchoredPosition.x, -23);
            rt9.anchoredPosition = new Vector2(rt9.anchoredPosition.x, 21);
            rt9.name = "ServerPlayersField";
            rt9.transform.Find("Text Area").Find("Placeholder").gameObject.GetComponent<TextMeshProUGUI>().text = "Max players...";
            rt9.transform.parent = __instance.HostSettingsOptionsNormal.transform;
            Debug.Log("ok!");
        }
        [HarmonyPatch(typeof(MenuManager), "StartHosting")]
        [HarmonyPrefix]
        public static bool StartHost(MenuManager __instance)
        {
            GameObject SPF = __instance.HostSettingsOptionsNormal.transform.Find("ServerPlayersField").gameObject;
            Debug.Log(SPF);
            GameObject Input = SPF.transform.Find("Text Area").Find("Text").gameObject;
            Debug.Log(Input);
            TextMeshProUGUI iTextMeshProUGUI = Input.GetComponent<TextMeshProUGUI>();
            Debug.Log(iTextMeshProUGUI);
            string text = Regex.Replace(iTextMeshProUGUI.text, "[^0-9]", "");
            Debug.Log(text);
            int newnumber;
            if (!(int.TryParse(text, out newnumber)))
            {
                Debug.Log(newnumber);
                newnumber = 20;
            }
            newnumber = Math.Min(Math.Max(newnumber, 4),20);
            Debug.Log(newnumber);
            Lobby lobby = GameNetworkManager.Instance.currentLobby ?? new Lobby();//fuck ittttt
            lobby.SetData("MaxPlayers", newnumber.ToString());
            return (true);

        }
        [HarmonyPatch(typeof(SteamLobbyManager),"LoadServerList")]
        [HarmonyPrefix]
        public async static void LoadServerList(SteamLobbyManager __instance )
        {
            Debug.Log("hai");
            if (GameNetworkManager.Instance.waitingForLobbyDataRefresh)
            {
                return;
            }
            Debug.Log(typeof(SteamLobbyManager).GetField("refreshServerListTimer", BindingFlags.NonPublic | BindingFlags.Instance));
            typeof(SteamLobbyManager).GetField("refreshServerListTimer", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(__instance, 0f);
            __instance.serverListBlankText.text = "Loading server list...";
            FieldInfo LL = typeof(SteamLobbyManager).GetField("currentLobbyList", BindingFlags.NonPublic | BindingFlags.Instance);
            FieldInfo LP = typeof(SteamLobbyManager).GetField("lobbySlotPositionOffset", BindingFlags.NonPublic | BindingFlags.Instance);
            LL.SetValue(__instance, null);
            LobbySlot[] array = UnityEngine.Object.FindObjectsOfType<LobbySlot>();
            for (int i = 0; i < array.Length; i++)
            {
                UnityEngine.Object.Destroy(array[i].gameObject);
            }
            switch (__instance.sortByDistanceSetting)
            {
                case 0:
                    SteamMatchmaking.LobbyList.FilterDistanceClose();
                    break;
                case 1:
                    SteamMatchmaking.LobbyList.FilterDistanceFar();
                    break;
                case 2:
                    SteamMatchmaking.LobbyList.FilterDistanceWorldwide();
                    break;
            }
            Debug.Log("Requested serv!er list");
            Debug.Log("WHY");

            GameNetworkManager.Instance.waitingForLobbyDataRefresh = true;
            Debug.Log("ok");
            Lobby[] results = await SteamMatchmaking.LobbyList.WithSlotsAvailable(1).WithKeyValue("vers", GameNetworkManager.Instance.gameVersionNum.ToString()).RequestAsync();
            Debug.Log(results);
            LL.SetValue(__instance, results);
            GameNetworkManager.Instance.waitingForLobbyDataRefresh = false;
            Debug.Log("ok");
            if (LL.GetValue(__instance) != null)
            {
                Debug.Log("Got lobby list!");
                if ((LL.GetValue(__instance) as Array).Length == 0)
                {
                    __instance.serverListBlankText.text = "No available servers to join.\n\n\nBizzlemip wuz here :3";
                }
                else
                {
                    __instance.serverListBlankText.text = "";
                }
                LP.SetValue(__instance,0f);
                for (int j = 0; j < (LL.GetValue(__instance) as Lobby[]).Length; j++)
                {
                    GameObject obj = UnityEngine.Object.Instantiate(__instance.LobbySlotPrefab, __instance.levelListContainer);
                    obj.GetComponent<RectTransform>().anchoredPosition = new Vector2(0f, (float)LP.GetValue(__instance));
                    LP.SetValue(__instance, (float)((float)LP.GetValue(__instance)) - 42f);
                    LobbySlot componentInChildren = obj.GetComponentInChildren<LobbySlot>();
                    componentInChildren.LobbyName.text = (LL.GetValue(__instance) as Lobby[])[j].GetData("name");
                    string text = (LL.GetValue(__instance) as Lobby[])[j].GetData("MaxPlayers");
                    int number;
                    Debug.Log(text);
                    if (!(int.TryParse(text, out number)))
                    {
                        number = 4;
                    }
                    number = Math.Min(Math.Max(number, 4), 20);
                    componentInChildren.playerCount.text = $"{(LL.GetValue(__instance) as Lobby[])[j].MemberCount} / " + number.ToString();
                    componentInChildren.lobbyId = (LL.GetValue(__instance) as Lobby[])[j].Id;
                    componentInChildren.thisLobby = (LL.GetValue(__instance) as Lobby[])[j];
                }
            }
            else
            {
                Debug.Log("Lobby list is null after request.");
                __instance.serverListBlankText.text = "No available servers to join.\n\n\nBizzlemip wuz here :3";
            }
            return;
        }
        [HarmonyPatch(typeof(SoundManager), "Awake")]
        [HarmonyPostfix]
        public static void SoundWake(ref SoundManager __instance)
        {
            __instance.playerVoiceMixers = Helper.ResizeArray(__instance.playerVoiceMixers, Plugin.MaxPlayers);
            for (int j = 4; j < Plugin.MaxPlayers; j++)
            {
                __instance.playerVoiceMixers[j] = UnityEngine.Object.Instantiate(__instance.playerVoiceMixers[0]);
                //__instance.playerVoiceMixers[j].
            }
        }
        [HarmonyPatch(typeof(SoundManager), "Start")]
        [HarmonyPostfix]
        public static void ResizeSoundManagerLists(ref SoundManager __instance)
        {
            __instance. playerVoicePitchLerpSpeed = new float[Plugin.MaxPlayers + 1];
            __instance.playerVoicePitchTargets = new float[Plugin.MaxPlayers + 1];
            __instance.playerVoicePitches = new float[Plugin.MaxPlayers+1];
            for (int i = 1; i < Plugin.MaxPlayers+1; i++)
            {
                __instance.playerVoicePitchLerpSpeed[i] = 3f;
                __instance.playerVoicePitchTargets[i] = 1f;
                __instance.playerVoicePitches[i] = 1f;
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
        [HarmonyPatch(typeof(StartOfRound), "Start")]
        [HarmonyPrefix]
        public static void AddPlayers(ref StartOfRound __instance)
        {
            startOfRound = __instance;
            referencePlayer = __instance.allPlayerObjects[0].GetComponent<PlayerControllerB>();
            var playerPrefab = __instance.playerPrefab;
            var playerContainer = __instance.allPlayerObjects[0].transform.parent;
            var spawnMethod = typeof(NetworkSpawnManager).GetMethod(
                "SpawnNetworkObjectLocally",
                BindingFlags.Instance | BindingFlags.NonPublic,
                null,
                CallingConventions.Any,
                new Type[] { typeof(NetworkObject), typeof(ulong), typeof(bool), typeof(bool), typeof(ulong), typeof(bool) },
                null
            );
                instantiating = true;
            for (int i = 4; i < Plugin.MaxPlayers; i++)
            {
                nextClientId = i;
                var newPlayer = GameObject.Instantiate<GameObject>(playerPrefab, playerContainer);
                var newScript = newPlayer.GetComponent<PlayerControllerB>();
                var netObject = newPlayer.GetComponent<NetworkObject>();
                newScript.TeleportPlayer(StartOfRound.Instance.notSpawnedPosition.position);
                Debug.Log(netObject.OwnerClientId);
                Debug.Log("[BigLobby] Trying to spawn new player");
                __instance.allPlayerObjects[i] = newPlayer;
                __instance.allPlayerScripts[i] = newScript;
                (typeof(NetworkObject)).GetProperty("NetworkObjectId", BindingFlags.Instance | BindingFlags.Public).SetValue(netObject, (uint)1234567890ul + (ulong)i);
                spawnMethod.Invoke(NetworkManager.Singleton.SpawnManager, new object[]{
                        netObject,
                        1234567890ul + (ulong)i,
                        true,//this needs to  be true or everything fucking shits itself. i think this might be the problem aswell. partciularly weird cuz apparently theres nested netobjs but i dont see any?
                        true,
                        netObject.OwnerClientId,
                        false
                    });
            }
            instantiating = false;
        }
        [HarmonyPatch(typeof(PlayerControllerB), "Update")]
        [HarmonyPrefix]

        public static bool ShitAssFix(ref PlayerControllerB __instance)
        {
            if (__instance.transform.parent.gameObject.name == "HangarShip" && !__instance.disconnectedMidGame) {
                __instance.isPlayerControlled = true;
            }
            return (true);
        }//Bizzlemip rolls worlds shittiest PATCH. Asked to leave MODDING COMMUNITY

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
