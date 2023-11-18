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

namespace BiggerLobby 
{
    [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    public class Plugin : BaseUnityPlugin
    {
        public static bool oldhastime;
        public static int MaxPlayers = 20;
        public static bool instantiating;
        public static NetworkObject[] PlayerObjects = new NetworkObject[]{ };
        //public static UnnamedStringMessageHandler MainCommunication;
        public static Harmony _harmony;
        public static Harmony _harmony2;
        private void Awake()
        {
            _harmony = new Harmony(PluginInfo.PLUGIN_GUID);//todo: patch non menu changes only when lobby joined, then unpatch them after.
            _harmony2 = new Harmony(PluginInfo.PLUGIN_GUID + "A");
            _harmony.PatchAll(typeof(Patches.NonGamePatches));
            _harmony.PatchAll(typeof(Patches.NonGamePatches.InternalPatch2));
            _harmony.PatchAll(typeof(Patches.NonGamePatches.InternalPatches));
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
