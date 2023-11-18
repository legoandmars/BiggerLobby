using BepInEx;
using GameNetcodeStuff;
using HarmonyLib;
using Steamworks.Data;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UIElements;

namespace BiggerLobby.Patches
{
    [HarmonyPatch(typeof(HUDManager))]
    internal class InternalPatch3
    {
        static MethodInfo TargetMethod()
        {
            return typeof(HUDManager)
                .GetMethod("AddChatMessage",
                           BindingFlags.NonPublic | BindingFlags.Instance);
        }
        [HarmonyPrefix]
        static void Prefix(HUDManager __instance, string chatMessage, string nameOfUserWhoTyped = "")
        {
            if (!(__instance.lastChatMessage == chatMessage))
            {
                __instance.lastChatMessage = chatMessage;
                __instance.PingHUDElement(__instance.Chat, 4f);
                if (__instance.ChatMessageHistory.Count >= 4)
                {
                    __instance.chatText.text.Remove(0, __instance.ChatMessageHistory[0].Length);
                    __instance.ChatMessageHistory.Remove(__instance.ChatMessageHistory[0]);
                }
                StringBuilder stringBuilder = new StringBuilder(chatMessage);
                for (int i = 1; i < StartOfRound.Instance.allPlayerScripts.Length; i++)
                {
                    stringBuilder.Replace("[playerNum" + i.ToString() + "]", StartOfRound.Instance.allPlayerScripts[i].playerUsername);
                }
                stringBuilder.Replace("bizzlemip", "<color=#008282>bizzlemip</color>");
                chatMessage = stringBuilder.ToString();
                nameOfUserWhoTyped = nameOfUserWhoTyped.Replace("bizzlemip", "<color=#008282>bizzlemip</color>");
                string item = ((!string.IsNullOrEmpty(nameOfUserWhoTyped)) ? ("<color=#FF0000>" + nameOfUserWhoTyped + "</color>: <color=#FFFF00>'" + chatMessage + "'</color>") : ("<color=#7069ff>" + chatMessage + "</color>"));
                __instance.ChatMessageHistory.Add(item);
                __instance.chatText.text = "";
                for (int i = 0; i < __instance.ChatMessageHistory.Count; i++)
                {
                    TextMeshProUGUI textMeshProUGUI = __instance.chatText;
                    textMeshProUGUI.text = textMeshProUGUI.text + "\n" + __instance.ChatMessageHistory[i];
                }
            }
        } //etc
    }
}
