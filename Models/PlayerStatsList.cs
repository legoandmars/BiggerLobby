using System;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BiggerLobby.Models
{
    public class PlayerStatsList
    { 
        public Transform transform; // lowercase so it's the same as the unity API
        public GameObject gameObject => transform.gameObject;

        // TODO: Setup project for nullables and make these properly null
        public List<TextMeshProUGUI> Names = new();
        public List<Image> States = new();
        public List<TextMeshProUGUI> Notes = new();

        public PlayerStatsList(Transform transform)
        {
            this.transform = transform;
        }

        public void AddPlayerSlotTransform(Transform playerSlot)
        {
            var playerName = playerSlot.GetChild(0).GetComponent<TextMeshProUGUI>();
            var playerSymbol = playerSlot.GetChild(1).GetComponent<Image>();
            var playerNotes = playerSlot.Find("Notes").GetComponent<TextMeshProUGUI>();

            Names.Add(playerName);
            States.Add(playerSymbol);
            Notes.Add(playerNotes);
        }

        public void AddPlayerSlotTransforms(List<Transform> playerSlots)
        {
            foreach (var playerSlot in playerSlots)
            {
                AddPlayerSlotTransform(playerSlot);
            }
        }
    }
}
