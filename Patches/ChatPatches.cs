using BepInEx;
using GameNetcodeStuff;
using HarmonyLib;
using Steamworks.Data;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UIElements;

namespace BiggerLobby.Patches
{
    [HarmonyPatch(typeof(HUDManager))]
    internal class ChatPatches
    {
        static MethodInfo TargetMethod()
        {
            return typeof(HUDManager)
                .GetMethod("AddChatMessage",
                           BindingFlags.NonPublic | BindingFlags.Instance);
        }

        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var stringBuilderConstructor = AccessTools.Constructor(typeof(StringBuilder), new[] { typeof(string) });
            var applyCustomPlayerNumberMethod = AccessTools.Method(typeof(ChatPatches), nameof(ApplyCustomPlayerNumber));

            var codeMatcher = new CodeMatcher(instructions)
                .MatchForward(false, new CodeMatch(OpCodes.Newobj, stringBuilderConstructor))
                .ThrowIfInvalid("Unable to find StringBuilder constructor.")
                .Advance(1)
                .Insert(new CodeInstruction(OpCodes.Callvirt, applyCustomPlayerNumberMethod))
                .InstructionEnumeration();

            return codeMatcher;
        }

        public static StringBuilder ApplyCustomPlayerNumber(StringBuilder stringBuilder)
        {
            var playerScripts = Plugin.GetRealPlayerScripts(StartOfRound.Instance);

            // if (playerScripts == null || playerScripts.Length < 5) 
            if (playerScripts == null)
                return stringBuilder;

            // Replace all players greater than index playerNum3 as needed
            // Leave playernum0-3 completely untouched to retain vanilla behaviour where possible
            for (int i = 4; i < playerScripts.Length; i++)
            {
                if (playerScripts[i]?.playerUsername == null)
                    continue;

                stringBuilder.Replace($"[playerNum{i}]", playerScripts[i].playerUsername);
            }

            return stringBuilder;
        }
    }
}
