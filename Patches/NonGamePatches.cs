using BepInEx;
using HarmonyLib;
using Steamworks.Data;
using Steamworks;
using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using Unity.Netcode;
using UnityEngine;
using System.Text.RegularExpressions;
using TMPro;
using System.Security.Cryptography;
using LC_API;
using System.Security.Permissions;
using UnityEngine.SceneManagement;
using System.Linq;
using GameNetcodeStuff;
using System.Runtime.CompilerServices;

namespace BiggerLobby.Patches
{
    [HarmonyPatch]
    public class NonGamePatches
    {
        private static PropertyInfo _playbackVolumeProperty = typeof(Dissonance.Audio.Playback.VoicePlayback).GetInterface("IVoicePlaybackInternal").GetProperty("PlaybackVolume");

        [HarmonyPatch(typeof(StartOfRound), "UpdatePlayerVoiceEffects")]
        [HarmonyPrefix]
        public static void UpdatePlayerVoiceEffects(StartOfRound __instance)
        {
            if (GameNetworkManager.Instance == null || GameNetworkManager.Instance.localPlayerController == null)
            {
                return;
            }
            (typeof(StartOfRound)).GetField("updatePlayerVoiceInterval", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(__instance,2f);
            PlayerControllerB playerControllerB = ((!GameNetworkManager.Instance.localPlayerController.isPlayerDead || !(GameNetworkManager.Instance.localPlayerController.spectatedPlayerScript != null)) ? GameNetworkManager.Instance.localPlayerController : GameNetworkManager.Instance.localPlayerController.spectatedPlayerScript);
            for (int i = 0; i < __instance.allPlayerScripts.Length; i++)
            {
                PlayerControllerB playerControllerB2 = __instance.allPlayerScripts[i];
                if ((!playerControllerB2.isPlayerControlled && !playerControllerB2.isPlayerDead) || playerControllerB2 == GameNetworkManager.Instance.localPlayerController)
                {
                    continue;
                }
                if (playerControllerB2.voicePlayerState == null || playerControllerB2.currentVoiceChatIngameSettings._playerState == null || playerControllerB2.currentVoiceChatAudioSource == null)
                {
                    __instance.RefreshPlayerVoicePlaybackObjects();
                    if (playerControllerB2.voicePlayerState == null || playerControllerB2.currentVoiceChatAudioSource == null)
                    {
                        Debug.Log($"Was not able to access voice chat object for player #{i}; {playerControllerB2.voicePlayerState == null}; {playerControllerB2.currentVoiceChatAudioSource == null}");
                        continue;
                    }
                }
                AudioSource currentVoiceChatAudioSource = __instance.allPlayerScripts[i].currentVoiceChatAudioSource;
                bool flag = playerControllerB2.speakingToWalkieTalkie && playerControllerB.holdingWalkieTalkie && playerControllerB2 != playerControllerB;
                if (playerControllerB2.isPlayerDead)
                {
                    currentVoiceChatAudioSource.GetComponent<AudioLowPassFilter>().enabled = false;
                    currentVoiceChatAudioSource.GetComponent<AudioHighPassFilter>().enabled = false;
                    currentVoiceChatAudioSource.panStereo = 0f;
                    SoundManager.Instance.playerVoicePitchTargets[playerControllerB2.playerClientId] = 1f;
                    SoundManager.Instance.SetPlayerPitch(1f, (int)playerControllerB2.playerClientId);
                    if (GameNetworkManager.Instance.localPlayerController.isPlayerDead)
                    {
                        currentVoiceChatAudioSource.spatialBlend = 0f;
                        playerControllerB2.currentVoiceChatIngameSettings.set2D = true;
                        if (playerControllerB2.currentVoiceChatIngameSettings != null && playerControllerB2.currentVoiceChatIngameSettings._playbackComponent != null)
                        {
                            _playbackVolumeProperty.SetValue(playerControllerB2.currentVoiceChatIngameSettings._playbackComponent, Mathf.Clamp((SoundManager.Instance.playerVoiceVolumes[i] + 1) * (2 * Plugin._LoudnessMultiplier.Value), 0f, 1f));
                        }
                    }
                    else
                    {
                        currentVoiceChatAudioSource.spatialBlend = 1f;
                        playerControllerB2.currentVoiceChatIngameSettings.set2D = false;
                        //playerControllerB2.voicePlayerState.Volume = 0f;
                        if (playerControllerB2.currentVoiceChatIngameSettings != null && playerControllerB2.currentVoiceChatIngameSettings._playbackComponent != null)
                        {
                            _playbackVolumeProperty.SetValue(playerControllerB2.currentVoiceChatIngameSettings._playbackComponent, 0);
                        }
                    }
                    continue;
                }
                AudioLowPassFilter component = currentVoiceChatAudioSource.GetComponent<AudioLowPassFilter>();
                OccludeAudio component2 = currentVoiceChatAudioSource.GetComponent<OccludeAudio>();
                component.enabled = true;
                component2.overridingLowPass = flag || __instance.allPlayerScripts[i].voiceMuffledByEnemy;
                currentVoiceChatAudioSource.GetComponent<AudioHighPassFilter>().enabled = flag;
                if (!flag)
                {
                    currentVoiceChatAudioSource.spatialBlend = 1f;
                    playerControllerB2.currentVoiceChatIngameSettings.set2D = false;
                    currentVoiceChatAudioSource.bypassListenerEffects = false;
                    currentVoiceChatAudioSource.bypassEffects = false;
                    currentVoiceChatAudioSource.outputAudioMixerGroup = SoundManager.Instance.playerVoiceMixers[playerControllerB2.playerClientId];
                    component.lowpassResonanceQ = 1f;
                }
                else
                {
                    currentVoiceChatAudioSource.spatialBlend = 0f;
                    playerControllerB2.currentVoiceChatIngameSettings.set2D = true;
                    if (GameNetworkManager.Instance.localPlayerController.isPlayerDead)
                    {
                        currentVoiceChatAudioSource.panStereo = 0f;
                        currentVoiceChatAudioSource.outputAudioMixerGroup = SoundManager.Instance.playerVoiceMixers[playerControllerB2.playerClientId];
                        currentVoiceChatAudioSource.bypassListenerEffects = false;
                        currentVoiceChatAudioSource.bypassEffects = false;
                    }
                    else
                    {
                        currentVoiceChatAudioSource.panStereo = 0.4f;
                        currentVoiceChatAudioSource.bypassListenerEffects = false;
                        currentVoiceChatAudioSource.bypassEffects = false;
                        currentVoiceChatAudioSource.outputAudioMixerGroup = SoundManager.Instance.playerVoiceMixers[playerControllerB2.playerClientId];
                    }
                    component2.lowPassOverride = 4000f;
                    component.lowpassResonanceQ = 3f;
                }
                /*if (GameNetworkManager.Instance.localPlayerController.isPlayerDead)
                {
                    playerControllerB2.voicePlayerState.Volume = 0.8f;
                }
                else
                {*/
                if (playerControllerB2.currentVoiceChatIngameSettings != null && playerControllerB2.currentVoiceChatIngameSettings._playbackComponent != null)
                {
                    _playbackVolumeProperty.SetValue(playerControllerB2.currentVoiceChatIngameSettings._playbackComponent, Mathf.Clamp((SoundManager.Instance.playerVoiceVolumes[i] + 1) * (2 * Plugin._LoudnessMultiplier.Value)), 0f, 1f);
                }
            }
        }
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
        
        [HarmonyPatch(typeof(SoundManager), "SetPlayerVoiceFilters")]
        [HarmonyPrefix]
        public static bool SetPlayerVoiceFilters(ref SoundManager __instance)
        {
            for (int j = 0; j < StartOfRound.Instance.allPlayerScripts.Length; j++)
            {
                if (!StartOfRound.Instance.allPlayerScripts[j].isPlayerControlled && !StartOfRound.Instance.allPlayerScripts[j].isPlayerDead)
                {
                    __instance.playerVoicePitches[j] = 1f;
                    __instance.playerVoiceVolumes[j] = 1f;
                    continue;
                }
                //Debug.Log(__instance.playerVoiceVolumes[j].ToString() + $"PlayerVolume{j}"); dont do this shit its annoying 
                //__instance.diageticMixer.SetFloat($"PlayerVolume{j}", 16f * __instance.playerVoiceVolumes[j]);
                if (StartOfRound.Instance.allPlayerScripts[j].voicePlayerState != null) {
                    (typeof(Dissonance.Audio.Playback.VoicePlayback).GetProperty("Dissonance.Audio.Playback.IVoicePlaybackInternal.PlaybackVolume", BindingFlags.NonPublic | BindingFlags.Instance)).SetValue(StartOfRound.Instance.allPlayerScripts[j].currentVoiceChatIngameSettings._playbackComponent, Mathf.Clamp((SoundManager.Instance.playerVoiceVolumes[j] + 1) * (2 * Plugin._LoudnessMultiplier.Value)), 0f, 1f);
                }
                if (Mathf.Abs(__instance.playerVoicePitches[j] - __instance.playerVoicePitchTargets[j]) > 0.025f)
                {
                    __instance.playerVoicePitches[j] = Mathf.Lerp(__instance.playerVoicePitches[j], __instance.playerVoicePitchTargets[j], 3f * Time.deltaTime);
                }
                else if (__instance.playerVoicePitches[j] != __instance.playerVoicePitchTargets[j])
                {
                    __instance.playerVoicePitches[j] = __instance.playerVoicePitchTargets[j];
                }
            }
            return (false);
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
            GameObject p9 = UnityEngine.Object.Instantiate(p4, p4.transform.parent);
            RectTransform rt9 = p9.GetComponent(typeof(RectTransform)) as RectTransform;
            rt.sizeDelta = new UnityEngine.Vector2(rt.sizeDelta.x, 200);
            rt2.anchoredPosition = new UnityEngine.Vector2(rt2.anchoredPosition.x, -50);
            rt3.anchoredPosition = new UnityEngine.Vector2(rt3.anchoredPosition.x, 40);
            rt4.anchoredPosition = new UnityEngine.Vector2(rt4.anchoredPosition.x, 55);
            rt5.anchoredPosition = new UnityEngine.Vector2(rt5.anchoredPosition.x, -60);
            rt6.anchoredPosition = new UnityEngine.Vector2(rt6.anchoredPosition.x, -85);
            rt7.anchoredPosition = new UnityEngine.Vector2(rt7.anchoredPosition.x, -23);
            rt8.anchoredPosition = new UnityEngine.Vector2(rt8.anchoredPosition.x, -23);
            rt9.anchoredPosition = new UnityEngine.Vector2(rt9.anchoredPosition.x, 21);
            rt9.name = "ServerPlayersField";
            rt9.GetComponent<TMP_InputField>().contentType = TMP_InputField.ContentType.IntegerNumber;
            rt9.transform.Find("Text Area").Find("Placeholder").gameObject.GetComponent<TextMeshProUGUI>().text = "Max players (16)...";
            rt9.transform.parent = __instance.HostSettingsOptionsNormal.transform;
            void OnChange()
            {
                string text = Regex.Replace(rt9.GetComponent<TMP_InputField>().text, "[^0-9]", "");
                Debug.Log(text);
                int newnumber;
                if (!(int.TryParse(text, out newnumber)))
                {
                    newnumber = 16;
                }
                newnumber = Math.Min(Math.Max(newnumber, 4), 40);
                Debug.Log(newnumber);
                if (newnumber > 16)
                {
                    p2.GetComponent<TextMeshProUGUI>().text = "Notice: High max player counts\nmay cause lag.";
                }
                else
                {
                    if (p2.GetComponent<TextMeshProUGUI>().text == "Notice: High max player counts\nmay cause lag.")
                    {
                        p2.GetComponent<TextMeshProUGUI>().text = "yeah you should be good now lol";
                    }
                }

            }
            rt9.GetComponent<TMP_InputField>().onValueChanged.AddListener(delegate { OnChange();  });
        }
        [HarmonyPatch(typeof(MenuManager), "StartHosting")]
        [HarmonyPrefix]
        public static bool StartHost(MenuManager __instance)
        {
            if (GameNetworkManager.Instance.currentLobby == null)
            {
                return (true);
            }
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
                newnumber = 16;
            }
            newnumber = Math.Min(Math.Max(newnumber, 4), 40);
            Debug.Log(newnumber);
            Lobby lobby = GameNetworkManager.Instance.currentLobby ?? new Lobby();
            lobby.SetData("MaxPlayers", newnumber.ToString());
            Debug.Log("SETTING MAX PLAYERS===");
            Plugin.MaxPlayers = newnumber;
            Debug.Log("SetMax");
            Debug.Log(newnumber);
            return (true);
        }
        [HarmonyPatch(typeof(HUDManager), "FillEndGameStats")]
        [HarmonyPrefix]
        public static bool FillEndGameStats()
        {
            return false;
        }
        [HarmonyPatch(typeof(GameNetworkManager),"StartHost")]
        [HarmonyPrefix]
        public static bool DoTheThe()
        {
            Plugin.CustomNetObjects.Clear();
            return (true);
        }
        [HarmonyPatch(typeof(GameNetworkManager), "StartClient")]
        [HarmonyPrefix]
        public static bool StartClient()
        {
            Plugin.CustomNetObjects.Clear();
            return (true);
        }
        [HarmonyPatch(typeof(MenuManager), "StartAClient")]
        [HarmonyPrefix]
        public static bool StartAClient()
        {
            Plugin.CustomNetObjects.Clear();
            Debug.Log("LanRunningggg!");
            return (true);
        }
        [HarmonyPatch(typeof(SteamLobbyManager), "LoadServerList")]
        [HarmonyPrefix]
        public async static void LoadServerList(SteamLobbyManager __instance)
        {
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
            GameNetworkManager.Instance.waitingForLobbyDataRefresh = true;
            Lobby[] results = await SteamMatchmaking.LobbyList.WithSlotsAvailable(1).WithKeyValue("vers", GameNetworkManager.Instance.gameVersionNum.ToString()).RequestAsync();
            Debug.Log(results);
            LL.SetValue(__instance, results);
            GameNetworkManager.Instance.waitingForLobbyDataRefresh = false;
            if (LL.GetValue(__instance) != null)
            {
                if ((LL.GetValue(__instance) as Array).Length == 0)
                {
                    __instance.serverListBlankText.text = "No available servers to join.\n\n\nBizzlemip wuz here :3";
                }
                else
                {
                    __instance.serverListBlankText.text = "";
                }
                LP.SetValue(__instance, 0f);
                for (int j = 0; j < (LL.GetValue(__instance) as Lobby[]).Length; j++)
                {
                    GameObject obj = UnityEngine.Object.Instantiate(__instance.LobbySlotPrefab, __instance.levelListContainer);
                    obj.GetComponent<RectTransform>().anchoredPosition = new UnityEngine.Vector2(0f, (float)LP.GetValue(__instance));
                    LP.SetValue(__instance, (float)((float)LP.GetValue(__instance)) - 42f);
                    LobbySlot componentInChildren = obj.GetComponentInChildren<LobbySlot>();
                    componentInChildren.LobbyName.text = (LL.GetValue(__instance) as Lobby[])[j].GetData("name").Replace("[BiggerLobby]","[BL]");
                    string text = (LL.GetValue(__instance) as Lobby[])[j].GetData("MaxPlayers");
                    int number;
                    Debug.Log(text);
                    if (!(int.TryParse(text, out number)))
                    {
                        number = 4;
                    }
                    number = Math.Min(Math.Max(number, 4), 40);
                    componentInChildren.playerCount.text = $"{(LL.GetValue(__instance) as Lobby[])[j].MemberCount} / " + number.ToString();
                    componentInChildren.lobbyId = (LL.GetValue(__instance) as Lobby[])[j].Id;
                    componentInChildren.thisLobby = (LL.GetValue(__instance) as Lobby[])[j];
                }
            }
            else
            {
                __instance.serverListBlankText.text = "No available servers to join.\n\n\nBizzlemip wuz here :3";
            }
            return;
        }
        [HarmonyPatch(typeof(SteamMatchmaking), nameof(SteamMatchmaking.CreateLobbyAsync))]
        [HarmonyPrefix]
        public static void SetMaxMembers(ref int maxMembers)
        {
            maxMembers = Plugin.MaxPlayers;
        }
        [HarmonyPatch(typeof(GameNetworkManager))]
        internal class InternalPatches
        {
            static MethodInfo TargetMethod()
            {
                return typeof(GameNetworkManager)
                    .GetMethod("ConnectionApproval",
                               BindingFlags.NonPublic | BindingFlags.Instance);
            }
            [HarmonyPrefix]
            static bool PostFix(GameNetworkManager __instance, NetworkManager.ConnectionApprovalRequest request, NetworkManager.ConnectionApprovalResponse response)
            {
                Debug.Log("Connection approval callback! Game version of client request: " + Encoding.ASCII.GetString(request.Payload).ToString());
                Debug.Log($"Joining client id: {request.ClientNetworkId}; Local/host client id: {NetworkManager.Singleton.LocalClientId}");
                if (request.ClientNetworkId == NetworkManager.Singleton.LocalClientId)
                {
                    Debug.Log("Stopped connection approval callback, as the client in question was the host!");
                    return (false);
                }
                bool flag = !__instance.disallowConnection;
                if (flag)
                {
                    string @string = Encoding.ASCII.GetString(request.Payload);
                    string[] array = @string.Split(",");
                    if (string.IsNullOrEmpty(@string))
                    {
                        response.Reason = "Unknown; please verify your game files.";
                        flag = false;
                    }
                    else if (__instance.gameHasStarted)
                    {
                        response.Reason = "Game has already started!";
                        flag = false;
                    }
                    else if (__instance.gameVersionNum.ToString() != array[0])
                    {
                        response.Reason = $"Game version mismatch! Their version: {__instance.gameVersionNum}. Your version: {array[0]}";
                        flag = false;
                    }
                    else if (!__instance.disableSteam && (StartOfRound.Instance == null || array.Length < 2 || StartOfRound.Instance.KickedClientIds.Contains((ulong)Convert.ToInt64(array[1]))))
                    {
                        response.Reason = "You cannot rejoin after being kicked.";
                        flag = false;
                    }
                    else if (!(@string.Contains("BiggerLobbyVersion2.4.0")))
                    {
                        response.Reason = "You need to have <color=#008282>BiggerLobby V2.4.0</color> to join this server!";
                        flag = false;
                    }
                }
                else
                {
                    response.Reason = "The host was not accepting connections.";
                }
                Debug.Log($"Approved connection?: {flag}. Connected players #: {__instance.connectedPlayers}");
                Debug.Log("Disapproval reason: " + response.Reason);
                response.CreatePlayerObject = false;
                response.Approved = flag;
                response.Pending = false;
                return (false);
            } //etc
        }
        [HarmonyPatch(typeof(GameNetworkManager))]
        internal class InternalPatches2
        {
            static MethodInfo TargetMethod()
            {
                return typeof(GameNetworkManager)
                    .GetMethod("SteamMatchmaking_OnLobbyCreated",
                               BindingFlags.NonPublic | BindingFlags.Instance);
            }
            [HarmonyPostfix]
            static void PostFix(GameNetworkManager __instance, Result result, Lobby lobby)
            {
                lobby.SetData("name", "[BiggerLobby]" + lobby.GetData("name"));
            } //etc
        }
        [HarmonyPatch(typeof(GameNetworkManager), "SetConnectionDataBeforeConnecting")]
        [HarmonyPrefix]
        public static bool SetConnectionDataBeforeConnecting(GameNetworkManager __instance)
        {
            __instance.localClientWaitingForApproval = true;
            Debug.Log("Game version: " + __instance.gameVersionNum);
            if (__instance.disableSteam)
            {
                NetworkManager.Singleton.NetworkConfig.ConnectionData = Encoding.ASCII.GetBytes(__instance.gameVersionNum.ToString() + "," + "BiggerLobbyVersion2.4.0");//this nonsense ass string exists to tell the server if youre running biggerlobby for some reason. Also she fortnite on my burger till I battle pass
            }
            else
            {
                NetworkManager.Singleton.NetworkConfig.ConnectionData = Encoding.ASCII.GetBytes(__instance.gameVersionNum + "," + (ulong)SteamClient.SteamId + "," + "BiggerLobbyVersion2.4.0");
            }
            return (false);
        }
        [HarmonyPatch(typeof(GameNetworkManager), nameof(GameNetworkManager.LobbyDataIsJoinable))]
        [HarmonyPrefix]
        public static bool SkipLobbySizeCheck(ref GameNetworkManager __instance, ref bool __result, Lobby lobby)
        {
            string data = lobby.GetData("vers");
            string text = lobby.GetData("MaxPlayers");
            int newnumber;
            if (!(int.TryParse(text, out newnumber)))
            {
                newnumber = 16;
            }
            newnumber = Math.Min(Math.Max(newnumber, 4), 40);
            if (lobby.MemberCount >= newnumber || lobby.MemberCount < 1)
            {
                Debug.Log($"Lobby join denied! Too many members in lobby! {lobby.Id}");
                UnityEngine.Object.FindObjectOfType<MenuManager>().SetLoadingScreen(isLoading: false, RoomEnter.Full, "The server is full!");
                __result = false;
                return false;
            }
            if (data != __instance.gameVersionNum.ToString())
            {
                Debug.Log($"Lobby join denied! Attempted to join vers.{data} lobby id: {lobby.Id}");
                UnityEngine.Object.FindObjectOfType<MenuManager>().SetLoadingScreen(isLoading: false, RoomEnter.DoesntExist, $"The server host is playing on version {data} while you are on version {__instance.gameVersionNum}.");
                __result = false;
                return false;
            }
            if (lobby.GetData("joinable") == "false")
            {
                Debug.Log("Lobby join denied! Host lobby is not joinable");
                UnityEngine.Object.FindObjectOfType<MenuManager>().SetLoadingScreen(isLoading: false, RoomEnter.DoesntExist, "The server host has already landed their ship, or they are still loading in.");
                __result = false;
                return false;
            }
            Debug.Log("AEAELOGGINGNUMBER");
            Debug.Log(newnumber);
            Debug.Log(lobby.GetData("MaxPlayers"));
            Debug.Log(newnumber);
            Debug.Log("SETTING MAX PLAYERS===");
            Plugin.MaxPlayers = newnumber;
            // Lobby member count check is skipped here, see original method
            __result = true;
            return false;
        }
    }
}
