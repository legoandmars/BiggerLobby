using HarmonyLib;
using Steamworks;
using Steamworks.Data;
using UnityEngine;

namespace BigLobby.Patches
{
    [HarmonyPatch(typeof(SteamMatchmaking))]
    internal class SteamLobbySize
    {
        [HarmonyPatch(nameof(SteamMatchmaking.CreateLobbyAsync))]
        [HarmonyPrefix]
        public static void SetMaxMembers(ref int maxMembers) {
            maxMembers = Plugin.MaxPlayers;
        }
    }
    [HarmonyPatch(typeof(GameNetworkManager))]
    internal class LobbyJoinable {
        [HarmonyPatch(nameof(GameNetworkManager.LobbyDataIsJoinable))]
        [HarmonyPrefix]
        public static bool SkipLobbySizeCheck(ref GameNetworkManager __instance, ref bool __result, Lobby lobby) {
            string data = lobby.GetData("vers");
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