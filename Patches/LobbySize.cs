using BepInEx;
using HarmonyLib;
using Steamworks;
using Steamworks.Data;
using System;
using UnityEngine;

namespace BigLobby.Patches
{
    [HarmonyPatch]
    internal class LobbySize
    {
        [HarmonyPatch(typeof(SteamMatchmaking), nameof(SteamMatchmaking.CreateLobbyAsync))]
        [HarmonyPrefix]
        public static void SetMaxMembers(ref int maxMembers) {
            maxMembers = Plugin.MaxPlayers;
        }
        [HarmonyPatch(typeof(GameNetworkManager), nameof(GameNetworkManager.LobbyDataIsJoinable))]
        [HarmonyPrefix]
        public static bool SkipLobbySizeCheck(ref GameNetworkManager __instance, ref bool __result, Lobby lobby) {
            string data = lobby.GetData("vers");
            string text = lobby.GetData("MaxPlayers");
            int number;
            if (!text.IsNullOrWhiteSpace() || !(int.TryParse(text, out number)))
            {
                number = 20;
            }
            number = Math.Min(Math.Max(number, 4),20);
            if (lobby.MemberCount >= number || lobby.MemberCount < 1)
            {
                Debug.Log($"Lobby join denied! Too many members in lobby! {lobby.Id}");
                UnityEngine.Object.FindObjectOfType<MenuManager>().SetLoadingScreen(isLoading: false, RoomEnter.Full, "The server is full!");
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
            // Lobby member count check is skipped here, see original method
            __result = true;
            return false;
        }
    }
}
