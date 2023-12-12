using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using GameNetcodeStuff;
using HarmonyLib;
using BepInEx;
using UnityEngine;
using System.Reflection;
using System;

namespace BiggerLobby.Patches
{
    [HarmonyPatch]
    public class ListSizeTranspilers
    {
        private static MethodInfo _playerCountMethod = AccessTools.Method(typeof(Plugin), "GetPlayerCount");
        private static MethodInfo _playerCountMinusOneMethod = AccessTools.Method(typeof(Plugin), "GetPlayerCountMinusOne");
        private static MethodInfo _realPlayerScriptsMethod = AccessTools.Method(typeof(Plugin), "GetRealPlayerScripts");
        /*private static List<CodeInstruction> _playerCountInstructions = new List<CodeInstruction>()
        {
            new CodeInstruction(OpCodes.Call, _playerCountMethod)
        };*/

        private static void CheckAndReplace(List<CodeInstruction> codes, int index)
        {
            if (codes[index].opcode == OpCodes.Ldc_I4_4)
            {
                Debug.Log("ok gunna do it");
                // Debug.Log((typeof(Plugin).GetField("MaxPlayers")));
                codes[index].opcode = OpCodes.Call;
                codes[index].operand = _playerCountMethod;
                Debug.Log("ok gunna did it");
            }
        }

        [HarmonyPatch(typeof(HUDManager), "SyncAllPlayerLevelsServerRpc", new Type[] { })] // SyncAllPlayerLevelsServerRpc(int, int) uses a list instead of an array[4]
        [HarmonyPatch(typeof(DressGirlAI), "ChoosePlayerToHaunt")]
        [HarmonyPatch(typeof(CrawlerAI), "Start")]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> SyncLevelsRpc(IEnumerable<CodeInstruction> instructions)
        {
            var codes = new List<CodeInstruction>(instructions);

            for (int i = 0; i < codes.Count; i++)
            {
                if (codes[i].opcode == OpCodes.Newarr)
                {
                    Debug.Log("newarr");
                    CheckAndReplace(codes, i - 1);
                }
            }
            return codes.AsEnumerable();
        }

        [HarmonyPatch(typeof(PlayerControllerB), "SendNewPlayerValuesServerRpc")]
        [HarmonyPatch(typeof(StartOfRound), "SyncShipUnlockablesClientRpc")]
        [HarmonyPatch(typeof(DressGirlAI), "ChoosePlayerToHaunt")]
        [HarmonyPatch(typeof(EnemyAI), "GetClosestPlayer")]
        [HarmonyPatch(typeof(SpringManAI), "DoAIInterval")]
        [HarmonyPatch(typeof(SpringManAI), "Update")]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> SendNewPlayerValuesServerRpc(IEnumerable<CodeInstruction> instructions) {
            var codes = new List<CodeInstruction>(instructions);
            for (int i = 0; i < codes.Count; i++)
            {
                if (codes[i].opcode == OpCodes.Blt)
                {
                    Debug.Log("blt");
                    CheckAndReplace(codes, i - 1);
                }
            }
            return codes.AsEnumerable();
        }
        [HarmonyPatch(typeof(QuickMenuManager), "ConfirmKickUserFromServer")]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> ConfirmKickUserFromServer(IEnumerable<CodeInstruction> instructions)
        {
            var codes = new List<CodeInstruction>(instructions);
            for (int i = 0; i < codes.Count; i++)
            {
                if (codes[i].opcode == OpCodes.Ldc_I4_3)
                {
                    codes[i].opcode = OpCodes.Call;
                    codes[i].operand = _playerCountMinusOneMethod; //lmfao
                    Debug.Log("Kick Fix Applied");
                    break;
                }
            }
            return codes.AsEnumerable();
        }

        [HarmonyPatch(typeof(HUDManager), "FillEndGameStats")]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> FillEndGameStatsPatch(IEnumerable<CodeInstruction> instructions)
        {
            var codes = new List<CodeInstruction>(instructions);
            for (int i = 0; i < codes.Count; i++)
            {
                if (codes[i].opcode == OpCodes.Ldfld && codes[i].operand is FieldInfo fieldInfo && fieldInfo.Name == "allPlayerScripts")
                {
                    // replace allPlayerScripts call with a devious custom method that will remove fake players
                    codes[i].opcode = OpCodes.Call;
                    codes[i].operand = _realPlayerScriptsMethod;
                }
            }

            foreach(var code in codes)
            {
                Debug.Log(code);
            }
            return codes.Where(x => x.opcode != OpCodes.Nop).AsEnumerable();
        }

        [HarmonyPatch(typeof(StartOfRound), "SyncShipUnlockablesServerRpc")]
        [HarmonyPatch(typeof(StartOfRound), "OnClientConnect")]
        [HarmonyPatch(typeof(PlayerControllerB), "SpectateNextPlayer")]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> SyncShipUnlockablesServerRpc(IEnumerable<CodeInstruction> instructions)
        {
            var codes = new List<CodeInstruction>(instructions);
            for (int i = 0; i < codes.Count; i++)
            {
                if (codes[i].opcode == OpCodes.Ldc_I4_4)
                {
                    codes[i].opcode = OpCodes.Call;
                    codes[i].operand = _playerCountMethod;
                }
            }
            return codes.AsEnumerable();
        }

    }
}