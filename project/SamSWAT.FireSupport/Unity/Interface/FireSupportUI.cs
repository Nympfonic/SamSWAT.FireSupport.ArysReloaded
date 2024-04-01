using Comfort.Common;
using EFT;
using EFT.UI;
using EFT.UI.Gestures;
using SamSWAT.FireSupport.ArysReloaded.Utils;
using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace SamSWAT.FireSupport.ArysReloaded.Unity
{
    public class FireSupportUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        private const string RANGEFINDER_TPL = "61605e13ffa6e502ac5e7eef";
        public GameObject SpotterNotice;
        public GameObject SpotterHeliNotice;
        public Text timerText;
        [SerializeField] private FireSupportUIElement[] supportOptions;
        [SerializeField] private HoverTooltipArea tooltip;
        private Player _player;
        private ESupportType _selectedSupportOption;
        private float _menuOffset;

        public static FireSupportUI Instance { get; private set; }
        public bool IsUnderPointer { get; set; }
        public event Action<ESupportType> SupportRequested;

        public static async Task<FireSupportUI> Load(GesturesMenu gesturesMenu)
        {
            Instance = Instantiate(await AssetLoader.LoadAssetAsync("assets/content/ui/firesupport_ui.bundle")).GetComponent<FireSupportUI>();
            Instance._player = Singleton<GameWorld>.Instance.MainPlayer;

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

        private void Update()
        {
            if (!gameObject.activeInHierarchy) return;
            RenderUI();
            HandleInput();
        }

        private void RenderUI()
        {
            var enabledColor = new Color(1, 1, 1, 1);
            var disabledColor = new Color(1, 1, 1, 0.4f);
            var rangefinderInHands = _player.HandsController.Item.TemplateId == RANGEFINDER_TPL;

            tooltip.SetUnlockStatus(rangefinderInHands);

            if (!rangefinderInHands)
            {
                return;
            }

            var fireSupportController = FireSupportController.Instance;
            if (fireSupportController.StrafeRequestAvailable)
            {
                supportOptions[2].AmountText.color = enabledColor;
                supportOptions[2].Icon.color = enabledColor;
            }
            else
            {
                supportOptions[2].IsUnderPointer = false;
                supportOptions[2].AmountText.color = disabledColor;
                supportOptions[2].Icon.color = disabledColor;
            }

            if (fireSupportController.ExtractRequestAvailable)
            {
                supportOptions[4].AmountText.color = enabledColor;
                supportOptions[4].Icon.color = enabledColor;
            }
            else
            {
                supportOptions[4].IsUnderPointer = false;
                supportOptions[4].AmountText.color = disabledColor;
                supportOptions[4].Icon.color = disabledColor;
            }

            supportOptions[2].AmountText.text = fireSupportController.AvailableStrafeRequests.ToString();
            supportOptions[4].AmountText.text = fireSupportController.AvailableExtractRequests.ToString();
        }

        private void HandleInput()
        {
            if (!IsUnderPointer) return;
            var rangefinderInHands = _player.HandsController.Item.TemplateId == RANGEFINDER_TPL;
            if (!rangefinderInHands) return;
            float angle = CalculateAngle();

            for (int i = 0; i < supportOptions.Length; i++)
            {
                if (angle > i * 45 && angle < (i + 1) * 45 && FireSupportController.Instance.AnyRequestAvailable)
                {
                    supportOptions[i].IsUnderPointer = true;
                    _selectedSupportOption = (ESupportType)i;
                }
                else
                {
                    supportOptions[i].IsUnderPointer = false;
                }
            }

            if (!Input.GetMouseButtonDown(0)) return;
            SupportRequested?.Invoke(_selectedSupportOption);
        }

        private float CalculateAngle()
        {
            Vector2 mouse;
            mouse.x = Input.mousePosition.x - (Screen.width / 2f);
            mouse.y = Input.mousePosition.y - (Screen.height / 2f) + _menuOffset;
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

        void IPointerEnterHandler.OnPointerEnter(PointerEventData data)
        {
            IsUnderPointer = true;
        }

        void IPointerExitHandler.OnPointerExit(PointerEventData data)
        {
            IsUnderPointer = false;

            foreach (var t in supportOptions)
            {
                t.IsUnderPointer = false;
            }
        }
    }

}