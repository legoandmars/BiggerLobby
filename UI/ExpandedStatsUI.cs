using BiggerLobby.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BiggerLobby.UI
{
    public class ExpandedStatsUI : MonoBehaviour
    {
        private bool _initialized = false;
        private bool _debugStatsUI = true;
        private StatsUIReferences? _statsUIReferences;
        private Transform _fourPlayersList;
        private Transform _eightPlayersList;

        private void Start()
        {
            Debug.Log("Starting EXPANDED STATS UI!");

            if (_initialized) return;
            
            if (_debugStatsUI)
            {
                DebugStats();
            }

            SetupFourPlayerSlots();
            SetupEightPlayerSlots();

            transform.GetChild(1).GetComponent<Image>().sprite = _statsUIReferences.StatsBoxesThin;
            _fourPlayersList.gameObject.SetActive(false);
            _initialized = true;
        }

        private void DebugStats()
        {
            // preview endgame stats here
            gameObject.GetComponent<Animator>().enabled = false;
            transform.GetChild(0).GetComponent<CanvasGroup>().alpha = 1;
            transform.GetChild(1).GetComponent<CanvasGroup>().alpha = 1;
            transform.GetChild(2).GetComponent<CanvasGroup>().alpha = 1;
            transform.GetChild(2).Find("AllDead").gameObject.SetActive(false);
        }

        private void SetupFourPlayerSlots()
        {
            _fourPlayersList = new GameObject("FourPlayersList").transform;
            _fourPlayersList.SetParent(transform.GetChild(2));
            _fourPlayersList.localPosition = Vector3.zero;
            _fourPlayersList.localRotation = Quaternion.identity;
            _fourPlayersList.localScale = Vector3.one;

            // move player slots

            transform.GetChild(2).Find("PlayerSlot1").SetParent(_fourPlayersList);
            transform.GetChild(2).Find("PlayerSlot2").SetParent(_fourPlayersList);
            transform.GetChild(2).Find("PlayerSlot3").SetParent(_fourPlayersList);
            transform.GetChild(2).Find("PlayerSlot4").SetParent(_fourPlayersList);

        }

        // only run after isolating 4 player slots in SetupFourPlayerSlots();
        private void SetupEightPlayerSlots()
        {
            _eightPlayersList = new GameObject("EightPlayersList").transform;
            _eightPlayersList.SetParent(transform.GetChild(2));
            _eightPlayersList.localPosition = Vector3.zero;
            _eightPlayersList.localRotation = Quaternion.identity;
            _eightPlayersList.localScale = Vector3.one;

            for(int i = 0; i < 8; i++)
            {
                var otherPlayer = Instantiate(_fourPlayersList.GetChild(0), _eightPlayersList, true);
                SetupEightPlayerSlot(otherPlayer);
                otherPlayer.localPosition = new Vector3(otherPlayer.localPosition.x, -26.1f * i, otherPlayer.localPosition.z);
            }
        }

        private void SetupEightPlayerSlot(Transform playerSlot)
        {
            var playerNotes = playerSlot.Find("Notes").GetComponent<TextMeshProUGUI>();
            var playerName = playerSlot.GetChild(0).GetComponent<TextMeshProUGUI>();
            var playerNameRect = playerName.GetComponent<RectTransform>();
            var playerSymbol = playerSlot.GetChild(1).GetComponent<Image>();
            var playerSymbolRect = playerSymbol.GetComponent<RectTransform>();

            playerNotes.text = "* Most lazy employee\n* Most paranoid employee\n* Sustained the most injuries";
            playerNotes.fontSize = 9;

            playerName.text = "CrazyDude12WW";
            playerNameRect.localPosition = new Vector3(playerNameRect.localPosition.x, 101.5f, playerNameRect.localPosition.z);

            playerSymbol.sprite = _statsUIReferences.CheckmarkThin;
            playerSymbolRect.sizeDelta = new Vector2(playerSymbolRect.sizeDelta.x, 31.235f);
            playerSymbolRect.localPosition = new Vector3(playerSymbolRect.localPosition.x, 101.5f, playerSymbolRect.localPosition.z);
        }

        public void LoadStatsUIBundle()
        {
            var bundlePath = Path.Join(Path.GetDirectoryName(Plugin.Instance.Info.Location), "statsuireferences");
            var bundle = AssetBundle.LoadFromFile(bundlePath);
            var asset = bundle.LoadAsset<GameObject>("assets/prefabs/statsuireferences.prefab");
            _statsUIReferences = asset.GetComponent<StatsUIReferences>();
        }

        public static ExpandedStatsUI GetFromAnimator(Animator endgameStatsAnimator)
        {
            if (endgameStatsAnimator.TryGetComponent<ExpandedStatsUI>(out ExpandedStatsUI component))
            {
                return component;
            }
            else
            {
                var statsUI = endgameStatsAnimator.gameObject.AddComponent<ExpandedStatsUI>();
                statsUI.LoadStatsUIBundle();
                return statsUI;
            }
        }
    }
}
