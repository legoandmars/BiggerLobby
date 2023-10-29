using BepInEx;
using HarmonyLib;
using System.Reflection;

namespace BigLobby
{
    [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    public class Plugin : BaseUnityPlugin
    {
        public static int MaxPlayers = 8;

        private Harmony _harmony;
        private void Awake()
        {
            _harmony = new Harmony(PluginInfo.PLUGIN_GUID);
            _harmony.PatchAll();

            Logger.LogInfo($"{PluginInfo.PLUGIN_GUID} loaded");
        }
    }
}
