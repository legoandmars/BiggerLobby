using HarmonyLib;
using Steamworks;

namespace BigLobby.Patches
{
    [HarmonyPatch(typeof(SteamMatchmaking), nameof(SteamMatchmaking.CreateLobbyAsync))]
    public static class CreateLobbyAsync
    {
        public static void Prefix(ref int maxMembers) {
            maxMembers = Plugin.MaxPlayers;
        }
    }
}