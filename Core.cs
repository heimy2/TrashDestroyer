﻿using System;
using System.Collections;
using System.Collections.Generic;
using HarmonyLib;
using Il2CppScheduleOne.Money;
using Il2CppScheduleOne.Trash;
using Il2CppScheduleOne.UI;
using MelonLoader;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using Il2CppTMPro;

[assembly: MelonInfo(typeof(TrashGrabPlus.Core), "TrashDestroyer", "2.0.0", "heimy", null)]
[assembly: MelonGame("TVGS", "Schedule I")]

namespace TrashGrabPlus
{
    [HarmonyPatch(typeof(TrashItem))]
    [HarmonyPatch("AddTrash")]
    [HarmonyPatch(new Type[]
                                {
                                typeof(TrashItem),
                            })]
    public class Core : MelonMod
    {
        private Dictionary<string, string> trashTypeMap = new Dictionary<string, string>
            {
                { "waterbottle", "plastic" },
                { "cigarette", "others" },
                { "energydrink", "metal" },
                { "usedcigarette", "others" },
                { "cigarette_used", "others" },
                { "litter1", "others" },
                { "crushedcuke", "metal" },
                { "pipe", "glass" },
                { "cuke", "metal" },
                { "plantscrap", "others" },
                { "cigarettebox", "others" },
                { "motoroil", "metal" },
                { "motoroil_used", "metal" },
                { "bong", "glass" },
                { "syringe", "plastic" },
                { "addy", "plastic" },
                { "glassbottle", "glass" },
                { "coffeecup", "others" },
                { "m1911mag", "metal" },
                { "revolvercylinder", "metal" },
                { "chemicaljug", "plastic" },
                { "gasoline", "plastic" },
                { "fertilizer", "plastic" },
                { "pgr", "plastic" },
                { "speedgrow", "plastic" },
                { "seedvial", "plastic" },
                { "acid", "plastic" },
                { "soilbag", "others" },
                { "soilbag2", "others" },
                { "soilbag3", "others" },
                { "extralonglifesoil", "others" },
                { "longlifesoil", "others" },
                { "soil", "others" }
            };

        private Dictionary<string, int> trashValueMap = new Dictionary<string, int>
            {
                { "glass", 30 },
                { "plastic", 2 },
                { "metal", 2 },
                { "others", 1 }
            };

        private GameObject _localPlayer;
        private MoneyManager _moneyManager;

        private static GameObject trashDestroyerButton;
        private static int trashDestroyerInterval = 5;
        private static Coroutine trashDestroyerCoroutine;

        public static Core Instance { get; private set; }

        public override void OnInitializeMelon()
        {
            Instance = this;
            LoggerInstance.Msg("Initialized.");

        }

        public override void OnUpdate()
        {
            if (Input.GetKeyDown(KeyCode.RightBracket)) // ']' key
            {
                UseTrashGrabber();
            }

            if (Input.GetKeyDown(KeyCode.LeftBracket)) // '[' key
            {
                UseTrashGrabberNearPlayer();
            }
        }

        public override void OnSceneWasLoaded(int buildIndex, string sceneName)
        {
            MelonCoroutines.Start(WaitForMoneyManager());
            CreateTrashDestroyerButton();
        }

        private IEnumerator WaitForMoneyManager()
        {
            // Initial delay of 10 seconds
            yield return new WaitForSeconds(10f);

            while (_moneyManager == null)
            {
                GameObject moneyManagerObject = GameObject.Find("Managers/@Money");
                if (moneyManagerObject != null)
                {
                    _moneyManager = moneyManagerObject.GetComponent<MoneyManager>();
                    LoggerInstance.Msg("MoneyManager component found and assigned.");
                }
                yield return new WaitForSeconds(5f);
            }
        }

        private void UseTrashGrabber()
        {
            Vector3 barnPosition = new Vector3(190.2984f, 1.065f, -11.6897f);
            Vector3 docksPosition = new Vector3(-86.7962f, -1.255f, -48.1173f);
            Vector3 sweatshopPosition = new Vector3(-61.8378f, 0.715f, 138.1508f);
            Vector3 housePosition = new Vector3(-172.1976f, -2.735f, 114.9906f);
            float radius = 20.0f;

            int totalCashAdded = 0;

            int barnCount = CheckAndPickUpTrashItems(barnPosition, radius, ref totalCashAdded);
            LoggerInstance.Msg($"Deleted {barnCount} trash items at the barn.");
            int docksCount = CheckAndPickUpTrashItems(docksPosition, radius, ref totalCashAdded);
            LoggerInstance.Msg($"Deleted {docksCount} trash items at the docks.");

            int sweatshopCount = CheckAndPickUpTrashItems(sweatshopPosition, radius, ref totalCashAdded);
            LoggerInstance.Msg($"Deleted {sweatshopCount} trash items at the sweatshop.");

            int houseCount = CheckAndPickUpTrashItems(housePosition, radius, ref totalCashAdded);
            LoggerInstance.Msg($"Deleted {houseCount} trash items at the house.");

            LoggerInstance.Msg($"Total cash added to the player: {totalCashAdded}");
        }

        private void UseTrashGrabberNearPlayer()
        {
            if (_localPlayer == null)
            {
                _localPlayer = GameObject.Find("Player_Local");
            }

            if (_localPlayer == null)
            {
                LoggerInstance.Msg("Player_Local GameObject not found.");
                return;
            }

            Vector3 playerPosition = _localPlayer.transform.position;
            float radius = 10.0f;

            int totalCashAdded = 0;

            int playerCount = CheckAndPickUpTrashItems(playerPosition, radius, ref totalCashAdded);
            LoggerInstance.Msg($"Deleted {playerCount} trash items near the player.");

            LoggerInstance.Msg($"Total cash added to the player: {totalCashAdded}");
        }

        private int CheckAndPickUpTrashItems(Vector3 position, float radius, ref int totalCashAdded)
        {
            TrashItem[] allTrashItems = GameObject.FindObjectsOfType<TrashItem>();
            int count = 0;
            foreach (var trashItem in allTrashItems)
            {
                float distance = Vector3.Distance(position, trashItem.transform.position);
                if (distance <= radius)
                {
                    PickUpTrashItem(trashItem, ref totalCashAdded);
                    count++;
                }
            }
            return count;
        }

        private void PickUpTrashItem(TrashItem trashItem, ref int totalCashAdded)
        {
            string trashID = trashItem.name.ToLower();

            // Remove any '(clone)' suffix if it exists
            if (trashID.Contains("(clone)"))
            {
                trashID = trashID.Replace("(clone)", "").Trim();
            }

            // Remove the '_trash' suffix if it exists
            if (trashID.EndsWith("_trash"))
            {
                trashID = trashID.Substring(0, trashID.Length - 6);
            }

            if (trashTypeMap.TryGetValue(trashID, out string trashType))
            {
                if (trashValueMap.TryGetValue(trashType, out int value))
                {
                    AddCashToPlayer(value);
                    totalCashAdded += value;
                }
            }

            GameObject.Destroy(trashItem.gameObject);
        }

        private void AddCashToPlayer(int amount)
        {
            if (_moneyManager != null)
            {
                _moneyManager.ChangeCashBalance(amount, true, false);
            }
            else
            {
                LoggerInstance.Msg("MoneyManager component not found.");
            }
        }

        private static string GetTrashDestroyerText()
        {
            return $"TrashDestroyer: {trashDestroyerInterval} Minutes";
        }
        private static void StaticOnTrashDestroyerButtonClick()
        {
            if (Instance != null)
            {

                Button buttonComponent = trashDestroyerButton.GetComponent<Button>();
                if (buttonComponent == null)
                {
                    MelonLogger.Error("Button component not found on TrashDestroyer button.");
                    return;
                }
                Instance.OnTrashDestroyerButtonClick();
            }
        }

        private static void CreateTrashDestroyerButton()
        {
            try
            {
                PauseMenu pauseMenu = GameObject.FindObjectOfType<PauseMenu>();
                if (pauseMenu == null) return;

                Transform container = pauseMenu.transform.Find("Container/Container/Bank");
                if (container == null) return;

                Transform quitButton = container.Find("Quit");
                if (quitButton == null) return;

                Transform existingButton = container.Find("TrashDestroyer");
                if (existingButton != null)
                {
                    GameObject.Destroy(existingButton.gameObject);
                }

                trashDestroyerButton = GameObject.Instantiate(quitButton.gameObject, container);
                trashDestroyerButton.name = "TrashDestroyer";

                Instance.buttonTexts = trashDestroyerButton.GetComponentsInChildren<TextMeshProUGUI>(true);
                foreach (TextMeshProUGUI text in Instance.buttonTexts)
                {
                    text.text = GetTrashDestroyerText();
                }

                Button buttonComponent = trashDestroyerButton.GetComponent<Button>();
                if (buttonComponent == null)
                {
                    MelonLogger.Error("Button component not found on TrashDestroyer button.");
                    return;
                }

                buttonComponent.onClick = new Button.ButtonClickedEvent();

                buttonComponent.onClick.AddListener((UnityAction)(() =>
                {
                    StaticOnTrashDestroyerButtonClick();
                }));

                RectTransform rectTransform = trashDestroyerButton.GetComponent<RectTransform>();
                rectTransform.anchoredPosition = new Vector2(rectTransform.anchoredPosition.x, 250f);


                trashDestroyerButton.transform.localScale = new Vector3(0.9f, 0.9f, 0.9f);
                trashDestroyerButton.SetActive(true);

                StartTrashDestroyer();
            }
            catch (Exception ex)
            {
                MelonLogger.Error($"Error adding TrashDestroyer button: {ex}");
            }
        }


        private void OnTrashDestroyerButtonClick()
        {
            UpdateTrashDestroyerInterval();
            foreach (TextMeshProUGUI text in buttonTexts)
            {
                text.text = GetTrashDestroyerText();
            }
        }

        private TextMeshProUGUI[] buttonTexts;



        private static void UpdateTrashDestroyerInterval()
        {
            int[] intervals = { 1, 5, 10, 15 }; // Intervals in minutes
            int currentIndex = Array.IndexOf(intervals, trashDestroyerInterval);
            trashDestroyerInterval = intervals[(currentIndex + 1) % intervals.Length];

            StartTrashDestroyer();
        }

        private static void StartTrashDestroyer()
        {
            if (trashDestroyerCoroutine != null)
            {
                MelonCoroutines.Stop(trashDestroyerCoroutine);
            }

            trashDestroyerCoroutine = MelonCoroutines.Start(TrashDestroyerRoutine()) as Coroutine;
        }

        private static IEnumerator TrashDestroyerRoutine()
        {
            while (true)
            {
                yield return new WaitForSeconds(trashDestroyerInterval * 60);
                if (Instance != null)
                {
                    Instance.UseTrashGrabber();
                }
            }
        }


    }
}
