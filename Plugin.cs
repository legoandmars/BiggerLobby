using BepInEx;
using HarmonyLib;
using System.Reflection;
using LC_API;
using GameNetcodeStuff;
using UnityEngine;
using UnityEngine.SceneManagement;
using Unity.Netcode;
using System.Collections.Generic;
using UnityEngine.Audio;
using Unity.Collections;
using BepInEx.Configuration;
using System.Linq;
using BepInEx.Logging;

namespace BiggerLobby 
{
    [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    public class Plugin : BaseUnityPlugin
    {
        public static Plugin Instance;
        public static bool oldhastime;
        public static int MaxPlayers = 16;
        public static bool instantiating;
        public static NetworkObject[] PlayerObjects = new NetworkObject[]{ };
        //public static UnnamedStringMessageHandler MainCommunication;
        public static Harmony _harmony;
        public static Harmony _harmony2;
        public static ConfigEntry<int>? _LoudnessMultiplier;
        public static bool Initialized = false;

        internal new static ManualLogSource? Logger { get; set; }

        public static IDictionary<uint, NetworkObject> CustomNetObjects = new Dictionary<uint, NetworkObject> { };
        private void Awake()
        {
            Instance = this;
            _LoudnessMultiplier =
            Config.Bind("General", "Player loudness", 1, "Default player loudness");
            Logger = base.Logger;
            _harmony = new Harmony(PluginInfo.PLUGIN_GUID);//todo: patch non menu changes only when lobby joined, then unpatch them after.
            _harmony2 = new Harmony(PluginInfo.PLUGIN_GUID + "A");
            _harmony.PatchAll(typeof(Patches.NonGamePatches));
            _harmony.PatchAll(typeof(Patches.NonGamePatches.InternalPatches));
            _harmony.PatchAll(typeof(Patches.NonGamePatches.InternalPatches2));
            Plugin.CustomNetObjects.Clear();
            Plugin._harmony2.PatchAll(typeof(Patches.ChatPatches));
            Plugin._harmony2.PatchAll(typeof(Patches.ListSizeTranspilers));
            Plugin._harmony2.PatchAll(typeof(Patches.PlayerObjects));
            Logger.LogInfo($"{PluginInfo.PLUGIN_GUID} loaded");
        }

        private void Start()
        {
            Initialize();
        }

        // Legacy behaviour for if BepInEx.cfg's "HideManagerGameObject" is set to false
        private void OnDestroy()
        {
            Initialize();
        }

        private void Initialize()
        {
            if (!Initialized)
            {
                Initialized = true;
                LC_API.ServerAPI.ModdedServer.SetServerModdedOnly();
            }
        }

        public static int GetPlayerCount()
        {
            return MaxPlayers;
        }

        public static int GetPlayerCountMinusOne()
        {
            return MaxPlayers - 1;
        }

        public static PlayerControllerB[] GetRealPlayerScripts(StartOfRound startOfRound)
        {
            if (startOfRound == null || startOfRound.allPlayerScripts == null)
            {
                return new PlayerControllerB[0]; // ??
            }
            // should probably be replaced with something smarter so this method doesn't have to run like 5 times every EndOfGameStats
            return startOfRound.allPlayerScripts.Where(x => x.isPlayerDead || x.isPlayerControlled).ToArray();
        }
    }
}
