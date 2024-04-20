using Comfort.Common;
using EFT;
using EFT.InputSystem;
using EFT.InventoryLogic;
using EFT.UI;
using EFT.UI.Gestures;
using HarmonyLib;
using SamSWAT.FireSupport.ArysReloaded.Unity.Interface;
using SamSWAT.FireSupport.ArysReloaded.Unity.Vehicles;
using SamSWAT.FireSupport.ArysReloaded.Unity.Vehicles.A10;
using SamSWAT.FireSupport.ArysReloaded.Unity.Vehicles.AH64;
using SamSWAT.FireSupport.ArysReloaded.Unity.Vehicles.UH60;
using SamSWAT.FireSupport.ArysReloaded.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using UnityEngine;

namespace SamSWAT.FireSupport.ArysReloaded.Unity
{
    public class FireSupportController : UIInputNode
    {
        private bool _requestAvailable = true;
        private Coroutine _timerCoroutine;
        private FireSupportAudio _audio;
        private FireSupportUI _ui;
        private FireSupportSpotter _spotter;
        private GesturesMenu _gesturesMenu;

        private A10FireSupport _a10FireSupport;
        private UH60FireSupport _uh60FireSupport;
        private AH64FireSupport _ah64FireSupport;

        public event Action<Player, string> OnEnemyKilledByFireSupport;

        public static FireSupportController Instance { get; private set; }
        public int PlayerTotalMoney { get; private set; }
        public int AvailableStrafeRequests { get; set; }
        public int AvailableExtractRequests { get; set; }
        public int AvailableApacheRequests { get; set; }
        public Dictionary<Player, string> TargetsKilledByApache { get; set; }

        public bool StrafeRequestAvailable =>
            _requestAvailable && AvailableStrafeRequests > 0 && PlayerTotalMoney >= Plugin.StrafeCost.Value;
        public bool ExtractRequestAvailable =>
            _requestAvailable && AvailableExtractRequests > 0 && PlayerTotalMoney >= Plugin.HelicopterCost.Value;
        public bool ApacheRequestAvailable =>
            _requestAvailable && AvailableApacheRequests > 0 && PlayerTotalMoney >= Plugin.ApacheCost.Value;
        public bool AnyRequestAvailable =>
            _requestAvailable && (AvailableStrafeRequests > 0 || AvailableExtractRequests > 0 || AvailableApacheRequests > 0);

        public static async Task<FireSupportController> Init(GesturesMenu gesturesMenu)
        {
            Instance = new GameObject("FireSupportController").AddComponent<FireSupportController>();

            Instance._audio = await FireSupportAudio.Load();
            Instance._spotter = await FireSupportSpotter.Load();
            Instance._ui = await FireSupportUI.Load(gesturesMenu);
            Instance._gesturesMenu = gesturesMenu;

            await InitFireSupport();
            WeaponClass.Init();

            Instance.TargetsKilledByApache = new Dictionary<Player, string>();

            Instance.AvailableStrafeRequests = Plugin.AmountOfStrafeRequests.Value;
            Instance.AvailableExtractRequests = Plugin.AmountOfExtractionRequests.Value;
            Instance.AvailableApacheRequests = Plugin.AmountOfApacheRequests.Value;

            Instance._audio.PlayVoiceover(VoiceoverType.StationReminder);

            Instance._ui.SupportRequested += Instance.OnSupportRequested;

            return Instance;
        }

        public void UpdateInventoryMoney(Player player, string currencyName)
        {
            var currencyTpl = ModHelper.GetCurrencyByName(currencyName);
            var total = player.Profile.Inventory.AllRealPlayerItems
                .Where(item => item.TemplateId == currencyTpl)
                .Sum(item => item.StackObjectsCount);

            Instance.PlayerTotalMoney = total;
        }

        public void PaySupportCost(Player player, int cost)
        {
            if (player == null)
            {
                return;
            }

            int costToSettle = cost;

            string currencyTplId = ModHelper.GetCurrencyByName(Plugin.SupportCostCurrency.Value);

            InventoryControllerClass inventoryController = player.GetInventoryController();

            Item[] moneyInInventory = player.Profile.Inventory.AllRealPlayerItems
                .Where(item => item.TemplateId == currencyTplId)
                .ToArray();

            foreach (Item moneyItem in moneyInInventory)
            {
                if (costToSettle >= moneyItem.StackObjectsCount)
                {
                    // Remove whole stack
                    costToSettle -= moneyItem.StackObjectsCount;

                    InteractionsHandlerClass.Remove(moneyItem, inventoryController);

                    continue;
                }
                
                // Subtract from stack
                costToSettle = 0;

                InteractionsHandlerClass.SplitToNowhere(
                    moneyItem,
                    costToSettle,
                    inventoryController,
                    inventoryController,
                    false
                );

                break;
            }
        }

        public void InvokeEnemyKilledByFireSupport(Player player, string killedBy)
        {
            OnEnemyKilledByFireSupport?.Invoke(player, killedBy);
        }

        public void StartRequestCooldown()
        {
            _timerCoroutine = StaticManager.BeginCoroutine(Timer(Plugin.RequestCooldown.Value));
        }

        public void SetRequestAvailable(bool available)
        {
            _requestAvailable = available;
        }

        public override ETranslateResult TranslateCommand(ECommand command)
        {
            return ETranslateResult.Ignore;
        }

        public override void TranslateAxes(ref float[] axes)
        {
        }

        public override ECursorResult ShouldLockCursor()
        {
            return ECursorResult.Ignore;
        }

        private void OnDestroy()
        {
            AssetLoader.UnloadAllBundles();
            _ui.SupportRequested -= OnSupportRequested;
            StaticManager.KillCoroutine(ref _timerCoroutine);
        }

        private static async Task InitFireSupport()
        {
            Instance._a10FireSupport = new A10FireSupport();
            Instance._uh60FireSupport = new UH60FireSupport();
            Instance._ah64FireSupport = new AH64FireSupport();

            var poolTransform = new GameObject("FireSupportPool").transform;
            poolTransform.position += Vector3.up * 600f;

            if (Plugin.AmountOfStrafeRequests.Value > 0)
            {
                await Instance._a10FireSupport.Load(poolTransform);
            }
            if (Plugin.AmountOfExtractionRequests.Value > 0)
            {
                await Instance._uh60FireSupport.Load(poolTransform);
            }
            if (Plugin.AmountOfApacheRequests.Value > 0)
            {
                await Instance._ah64FireSupport.Load(poolTransform);
            }
        }

        private void OnSupportRequested(SupportType supportType)
        {
            IFireSupport<VehicleBehaviour> support;
            switch (supportType)
            {
                case SupportType.Strafe:
                    support = _a10FireSupport;
                    break;
                case SupportType.Extract:
                    support = _uh60FireSupport;
                    break;
                case SupportType.Apache:
                    support = _ah64FireSupport;
                    break;
                default:
                    return;
            }

            if (support != null && _requestAvailable)
            {
                support.MakeRequest(_gesturesMenu, _spotter);
            }
        }

        private IEnumerator Timer(float time)
        {
            _requestAvailable = false;
            _ui.timerText.enabled = true;
            while (time > 0)
            {
                time -= Time.deltaTime;
                if (time < 0)
                {
                    time = 0;
                }

                float minutes = Mathf.FloorToInt(time / 60);
                float seconds = Mathf.FloorToInt(time % 60);
                _ui.timerText.text = $"{minutes:00}.{seconds:00}";
                yield return null;
            }
            _ui.timerText.enabled = false;
            _requestAvailable = true;
            if (AnyRequestAvailable)
            {
                FireSupportAudio.Instance.PlayVoiceover(VoiceoverType.StationAvailable);
            }
        }
    }
}