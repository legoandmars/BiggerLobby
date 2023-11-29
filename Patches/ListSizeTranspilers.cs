using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using GameNetcodeStuff;
using HarmonyLib;
using BepInEx;
using UnityEngine;
using System.Reflection;

namespace BiggerLobby.Patches
{
    [HarmonyPatch]
    public class ListSizeTranspilers
    {
        private static void CheckAndReplace(List<CodeInstruction> codes, int index)
        {
            if (codes[index].opcode == OpCodes.Ldc_I4_4)
            {
                Debug.Log("ok gunna do it");
                Debug.Log((typeof(Plugin).GetField("MaxPlayers")));
                codes[index].opcode = OpCodes.Ldsfld;
                codes[index].operand = (typeof(Plugin).GetField("MaxPlayers"));
                Debug.Log("ok gunna did it");
            }
        }
        [HarmonyPatch(typeof(HUDManager), "SyncAllPlayerLevelsServerRpc")]
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
        [HarmonyPatch(typeof(StartOfRound), "EndOfGameClientRpc")]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> EndOfGameClientRpc(IEnumerable<CodeInstruction> instructions)
        {
            var codes = new List<CodeInstruction>(instructions);
            for (int i = 0; i < codes.Count; i++)
            {
                if (codes[i].opcode == OpCodes.Brfalse && codes[i - 1].opcode == OpCodes.Ldfld && codes[i - 2].opcode == OpCodes.Ldfld && codes[i - 3].opcode == OpCodes.Ldarg_0)
                {
                    Debug.Log(codes[i - 1].opcode);
                    Debug.Log(codes[i - 2].opcode);
                    Debug.Log(codes[i - 3].opcode);

                    codes[i-1].opcode = OpCodes.Nop;
                    codes[i-2].opcode = OpCodes.Nop;
                    CheckAndReplace(codes, i - 3);
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
                    codes[i].opcode = OpCodes.Ldc_I4_S;
                    codes[i].operand = 39;//lmfao
                    Debug.Log("Kick Fix Applied");
                    break;
                }
            }
            return codes.AsEnumerable();
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
                    codes[i].opcode = OpCodes.Ldc_I4_S;
                    codes[i].operand = Plugin.MaxPlayers;
                }
            }
            return codes.AsEnumerable();
        }

    }
}