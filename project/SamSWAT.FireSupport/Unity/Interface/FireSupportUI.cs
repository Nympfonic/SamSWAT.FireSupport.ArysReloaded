using Comfort.Common;
using EFT.UI;
using EFT.UI.Gestures;
using SamSWAT.FireSupport.ArysReloaded.Utils;
using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace SamSWAT.FireSupport.ArysReloaded.Unity.Interface
{
    public class FireSupportUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IBatchUpdate
    {
        public GameObject SpotterNotice;
        public GameObject SpotterHeliNotice;
        public Text timerText;
        [SerializeField] private FireSupportUIElement[] supportOptions;
        [SerializeField] private HoverTooltipArea tooltip;
        private SupportType _selectedSupportOption;
        private float _menuOffset;

        public event Action<SupportType> SupportRequested;

        public static FireSupportUI Instance { get; private set; }
        public bool IsUnderPointer { get; set; }

        public static async Task<FireSupportUI> Load(GesturesMenu gesturesMenu)
        {
            Instance = Instantiate(await AssetLoader.LoadAssetAsync("assets/content/ui/firesupport_ui.bundle")).GetComponent<FireSupportUI>();

            var fireSupportUiT = Instance.transform;
            fireSupportUiT.parent = gesturesMenu.transform;
            fireSupportUiT.localPosition = new Vector3(0, -255, 0);
            fireSupportUiT.localScale = new Vector3(1.4f, 1.4f, 1);
            Instance._menuOffset = Screen.height / 2f - fireSupportUiT.position.y;

            var infoPanelTransform = Instance.SpotterNotice.transform.parent;
            infoPanelTransform.parent = Singleton<GameUI>.Instance.transform;
            infoPanelTransform.localPosition = new Vector3(0, -370f, 0);
            infoPanelTransform.localScale = Vector3.one;

            return Instance;
        }

        public void BatchUpdate()
        {
            if (!gameObject.activeInHierarchy) 
            {
                return;
            }

            RenderUI();
            HandleInput();
        }

        private void Start()
        {
            UpdateManager.Instance.RegisterSlicedUpdate(this, UpdateManager.UpdateMode.Always);
        }

        private void OnDestroy()
        {
            UpdateManager.Instance.DeregisterSlicedUpdate(this);
        }

        private void RenderUI()
        {
            var rangefinderInHands = ModHelper.HasRangefinderInHands();

            tooltip.SetUnlockStatus(rangefinderInHands);

            if (!rangefinderInHands)
            {
                return;
            }

            var fsController = FireSupportController.Instance;
            if (fsController == null)
            {
                return;
            }

            RenderSupportOption(2, fsController.StrafeRequestAvailable, fsController.AvailableStrafeRequests, Plugin.StrafeCost.Value);
            RenderSupportOption(4, fsController.ExtractRequestAvailable, fsController.AvailableExtractRequests, Plugin.HelicopterCost.Value);
            RenderSupportOption(6, fsController.ApacheRequestAvailable, fsController.AvailableApacheRequests, Plugin.ApacheCost.Value);
        }

        private void RenderSupportOption(int index, bool requestAvailable, int availableRequests, int requestCost)
        {
            var enabledColor = new Color(1, 1, 1, 1);
            var disabledColor = new Color(1, 1, 1, 0.4f);

            if (requestAvailable)
            {
                supportOptions[index].AmountText.color = enabledColor;
                supportOptions[index].Icon.color = enabledColor;
            }
            else
            {
                supportOptions[index].IsUnderPointer = false;
                supportOptions[index].AmountText.color = disabledColor;
                supportOptions[index].Icon.color = disabledColor;
            }

            supportOptions[index].AmountText.text = $"Available: {availableRequests}\nCost: {requestCost} {Plugin.SupportCostCurrency.Value}";
        }

        private void HandleInput()
        {
            if (!IsUnderPointer) return;
            if (!ModHelper.HasRangefinderInHands()) return;
            float angle = CalculateAngle();

            for (int i = 0; i < supportOptions.Length; i++)
            {
                if (angle > i * 45 && angle < (i + 1) * 45)
                {
                    if (i == 2 && FireSupportController.Instance.StrafeRequestAvailable
                        || i == 4 && FireSupportController.Instance.ExtractRequestAvailable
                        || i == 6 && FireSupportController.Instance.ApacheRequestAvailable)
                    {
                        supportOptions[i].IsUnderPointer = true;
                        _selectedSupportOption = (SupportType)i;
                    }
                }
                else
                {
                    supportOptions[i].IsUnderPointer = false;
                }
            }

            if (!(Input.GetMouseButtonDown(0) && )) return;
            SupportRequested?.Invoke(_selectedSupportOption);
        }

        private float CalculateAngle()
        {
            Vector2 mouse;
            mouse.x = Input.mousePosition.x - Screen.width / 2f;
            mouse.y = Input.mousePosition.y - Screen.height / 2f + _menuOffset;
            mouse.Normalize();

            if (mouse == Vector2.zero)
            {
                return 0;
            }

            float angle = Mathf.Atan2(mouse.y, -mouse.x) / Mathf.PI;
            angle *= 180;
            angle += 111;

            if (angle < 0)
            {
                angle += 360;
            }

            return angle;
        }

        void IPointerEnterHandler.OnPointerEnter(PointerEventData eventData)
        {
            IsUnderPointer = true;
        }

        void IPointerExitHandler.OnPointerExit(PointerEventData eventData)
        {
            IsUnderPointer = false;

            foreach (var t in supportOptions)
            {
                t.IsUnderPointer = false;
            }
        }
    }

}