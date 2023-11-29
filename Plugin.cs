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

namespace BiggerLobby 
{
    [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    public class Plugin : BaseUnityPlugin
    {
        public static bool oldhastime;
        public static int MaxPlayers = 16;
        public static bool instantiating;
        public static NetworkObject[] PlayerObjects = new NetworkObject[]{ };
        //public static UnnamedStringMessageHandler MainCommunication;
        public static Harmony _harmony;
        public static Harmony _harmony2;
        public static ConfigEntry<int>? _LoudnessMultiplier;

        public static IDictionary<uint, NetworkObject> CustomNetObjects = new Dictionary<uint, NetworkObject> { };
        private void Awake()
        {
            _LoudnessMultiplier =
            Config.Bind("General", "Player loudness", 1, "Default player loudness");
            _harmony = new Harmony(PluginInfo.PLUGIN_GUID);//todo: patch non menu changes only when lobby joined, then unpatch them after.
            _harmony2 = new Harmony(PluginInfo.PLUGIN_GUID + "A");
            _harmony.PatchAll(typeof(Patches.NonGamePatches));
            _harmony.PatchAll(typeof(Patches.NonGamePatches.InternalPatches));
            _harmony.PatchAll(typeof(Patches.NonGamePatches.InternalPatches2));
            Plugin.CustomNetObjects.Clear();
            Plugin._harmony2.PatchAll(typeof(Patches.InternalPatch3));
            Plugin._harmony2.PatchAll(typeof(Patches.ListSizeTranspilers));
            Plugin._harmony2.PatchAll(typeof(Patches.PlayerObjects));
            Logger.LogInfo($"{PluginInfo.PLUGIN_GUID} loaded");
            LC_API.BundleAPI.BundleLoader.OnLoadedAssets += OnLoaded;
        }


        private void OnLoaded()
        {
            AudioMixer Mixer = LC_API.BundleAPI.BundleLoader.GetLoadedAsset<AudioMixer>("assets/diagetic.mixer");
            if (!Mixer)
            {
                return;
            }
            
        }
        private void OnDestroy()
        {
            LC_API.ServerAPI.ModdedServer.SetServerModdedOnly();
        }
    }
}
