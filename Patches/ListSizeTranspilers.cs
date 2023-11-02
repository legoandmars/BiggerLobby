using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using GameNetcodeStuff;
using HarmonyLib;

namespace BigLobby.Patches
{
    [HarmonyPatch]
    internal class ListSizeTranspilers {
        [HarmonyPatch(typeof(HUDManager), "SyncAllPlayerLevelsServerRpc")]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> SyncLevelsRpc(IEnumerable<CodeInstruction> instructions) {
            var codes = new List<CodeInstruction>(instructions);
            for (int i = 0; i < codes.Count; i++)
            {
                if (codes[i].opcode == OpCodes.Newarr)
                {
                    if (codes[i - 1].OperandIs(4)) {
                        codes[i - 1].operand = 4;
                        break;
                    }
                }
            }
            return codes.AsEnumerable();
        }
        [HarmonyPatch(typeof(StartOfRound), "SyncShipUnlockablesServerRpc")]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> SyncUnlockablesRpc(IEnumerable<CodeInstruction> instructions) {
            var codes = new List<CodeInstruction>(instructions);
            for (int i = 0; i < codes.Count; i++)
            {
                if (codes[i].opcode == OpCodes.Newarr)
                {
                    if (codes[i - 1].OperandIs(4)) {
                        codes[i - 1].operand = 4;
                        break;
                    }
                }
            }
            for (int i = 0; i < codes.Count; i++)
            {
                if (codes[i].opcode == OpCodes.Blt_S)
                {
                    if (codes[i - 1].OperandIs(4)) {
                        codes[i - 1].operand = 4;
                        break;
                    }
                }
            }
            return codes.AsEnumerable();
        }
        [HarmonyPatch(typeof(PlayerControllerB), "SendNewPlayerValuesServerRpc")]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> SendPlayerValuesRpc(IEnumerable<CodeInstruction> instructions) {
            var codes = new List<CodeInstruction>(instructions);
            for (int i = 0; i < codes.Count; i++)
            {
                if (codes[i].opcode == OpCodes.Blt_S)
                {
                    if (codes[i - 1].OperandIs(4)) {
                        codes[i - 1].operand = 4;
                        break;
                    }
                }
            }
            return codes.AsEnumerable();
        }
        [HarmonyPatch(typeof(PlayerControllerB), "SpectateNextPlayer")]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> SpectateNextPlayer(IEnumerable<CodeInstruction> instructions) {
            var codes = new List<CodeInstruction>(instructions);
            for (int i = 0; i < codes.Count; i++)
            {
                if (codes[i].opcode == OpCodes.Blt_S)
                {
                    if (codes[i - 1].OperandIs(4)) {
                        codes[i - 1].operand = 4;
                        break;
                    }
                }
            }
            return codes.AsEnumerable();
        }
    }
}