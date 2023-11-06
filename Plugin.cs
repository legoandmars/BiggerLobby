using BepInEx;
using HarmonyLib;
using System.Reflection;
using LC_API;
using GameNetcodeStuff;
using UnityEngine;
using UnityEngine.SceneManagement;
using Unity.Netcode;

namespace BiggerLobby 
{
    [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    public class Plugin : BaseUnityPlugin
    {
        public static bool oldhastime;
        public static int MaxPlayers = 20;
        public static bool instantiating;
        private Harmony _harmony;
        private void Awake()
        {
            _harmony = new Harmony(PluginInfo.PLUGIN_GUID);
            _harmony.PatchAll();
            Logger.LogInfo($"{PluginInfo.PLUGIN_GUID} loaded");
        }

        private void OnDestroy()
        {
            LC_API.ServerAPI.ModdedServer.SetServerModdedOnly();
        }
    }
}
