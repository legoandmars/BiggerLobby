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
        private PlayerStatsList _fourPlayersList;
        private PlayerStatsList _eightPlayersList;
        private PlayerStatsList _moreThanEightPlayersList;

        // TODO: convert this to one variable somewhere, it's also used in the UI capping max player count
        public int UpperPlayerLimit = 40;

        // TODO: reimpl end game UI "checking off" players one at a time
        // this is implemented in an animation and might be a bit annoying to recreate
        public float SecondsPanelVisible = 8.5f;

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
            SetupMoreThanEightPlayersSlots();

            transform.GetChild(1).GetComponent<Image>().sprite = _statsUIReferences.StatsBoxesThin;
            _fourPlayersList.gameObject.SetActive(false);
            _eightPlayersList.gameObject.SetActive(false);
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

        // TODO: this breaks base game animation
        private void SetupFourPlayerSlots()
        {
            _fourPlayersList = new(CreateTransformAtParentOrigin("FourPlayersList", transform.GetChild(2)));
            // move player slots

            for (int i = 0; i < 4; i++)
            {
                var playerSlot = transform.GetChild(2).Find($"PlayerSlot{i + 1}");
                playerSlot.SetParent(_fourPlayersList.transform);
                _fourPlayersList.AddPlayerSlotTransform(playerSlot);
            }
        }

        // only run after isolating 4 player slots in SetupFourPlayerSlots();
        private void SetupEightPlayerSlots()
        {
            _eightPlayersList = new(CreateTransformAtParentOrigin("EightPlayersList", transform.GetChild(2)));

            var playerSlots = SetupEightPlayerPage(_eightPlayersList.transform);
            _eightPlayersList.AddPlayerSlotTransforms(playerSlots);
        }

        private void SetupMoreThanEightPlayersSlots()
        {
            _moreThanEightPlayersList = new(CreateTransformAtParentOrigin("MoreThanEightPlayersList", transform.GetChild(2)));

            int maxPageCount = (int)Math.Ceiling(UpperPlayerLimit / 8f);
            for (int i = 0; i < maxPageCount; i++)
            {
                var page = CreateTransformAtParentOrigin($"Page{i}", _moreThanEightPlayersList.transform);
                var playerSlots = SetupEightPlayerPage(page);
                _moreThanEightPlayersList.AddPlayerSlotTransforms(playerSlots);

                if (i != 0) page.gameObject.SetActive(false);
            }
        }

        private List<Transform> SetupEightPlayerPage(Transform parent)
        {
            List<Transform> playerSlots = new();
            for (int i = 0; i < 8; i++)
            {
                var otherPlayer = Instantiate(_fourPlayersList.transform.GetChild(0), parent, true);
                SetupPlayerSlot(otherPlayer);
                otherPlayer.localPosition = new Vector3(otherPlayer.localPosition.x, -26.1f * i, otherPlayer.localPosition.z);
                playerSlots.Add(otherPlayer);
            }

            return playerSlots;
        }

        private void SetupPlayerSlot(Transform playerSlot)
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
        
        private Transform CreateTransformAtParentOrigin(string name, Transform parent)
        {
            var newTransform = new GameObject(name).transform;
            newTransform.SetParent(parent);
            newTransform.localPosition = Vector3.zero;
            newTransform.localRotation = Quaternion.identity;
            newTransform.localScale = Vector3.one;

            return newTransform;
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
