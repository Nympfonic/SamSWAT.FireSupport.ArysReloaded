using System.Collections;
using System.Threading.Tasks;
using Comfort.Common;
using EFT;
using EFT.UI;
using EFT.UI.Gestures;
using SamSWAT.FireSupport.Utils;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace SamSWAT.FireSupport.Unity
{
    public class FireSupportUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        public GameObject SpotterNotice;
        public GameObject SpotterHeliNotice;
        [SerializeField] private FireSupportUIElement[] supportOptions;
        [SerializeField] private Text timerText;
        [SerializeField] private HoverTooltipArea tooltip;
        private GesturesMenu _gesturesMenu;
        private Player _player;
        private ESupportType _selectedSupportOption;
        private float _menuOffset;
        private bool _requestAvailable = true;
        private int _availableStrafeRequests;
        private int _availableExtractRequests;
        private Coroutine _timerCoroutine;

        public static FireSupportUI Instance { get; private set; }
        public bool IsUnderPointer { get; set; }

        public static async Task Load(GesturesMenu gesturesMenu)
        {
            Instance = Instantiate(await UtilsClass.LoadAssetAsync<GameObject>("assets/content/ui/firesupport_ui.bundle")).GetComponent<FireSupportUI>();
            var instanceTransform = Instance.transform;
            instanceTransform.parent = gesturesMenu.transform;
            instanceTransform.localPosition = new Vector3(0, -255, 0);
            instanceTransform.localScale = new Vector3(1.4f, 1.4f, 1);
            Instance._menuOffset = Screen.height / 2f - instanceTransform.position.y;
            Instance._gesturesMenu = gesturesMenu;
            Instance._availableStrafeRequests = Plugin.AmountOfStrafeRequests.Value;
            Instance._availableExtractRequests = Plugin.AmountOfExtractionRequests.Value;
            Instance._player = Singleton<GameWorld>.Instance.RegisteredPlayers[0];
            var infoPanelTransform = Instance.SpotterNotice.transform.parent;
            infoPanelTransform.parent = Singleton<GameUI>.Instance.transform;
            infoPanelTransform.localPosition = new Vector3(0, -370f, 0);
            infoPanelTransform.localScale = Vector3.one;
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
            var rangefinderInHands = _player.HandsController.GetType() == UtilsClass.RangefinderControllerType;
            //var rangefinderInHands = _player.HandsController.Item.TemplateId == "61605e13ffa6e502ac5e7eef";

            tooltip.SetUnlockStatus(rangefinderInHands);
            
            if (!rangefinderInHands)
            {
                return;
            }

            if (_requestAvailable && _availableStrafeRequests > 0)
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

            if (_requestAvailable && _availableExtractRequests > 0)
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

            supportOptions[2].AmountText.text = _availableStrafeRequests.ToString();
            supportOptions[4].AmountText.text = _availableExtractRequests.ToString();
        }

        private void HandleInput()
        {
            if (!IsUnderPointer) return;
            
            var rangefinderInHands = _player.HandsController.Item.TemplateId == "61605e13ffa6e502ac5e7eef";
            
            if (!rangefinderInHands) return;
            
            float angle = CalculateAngle();

            for (int i = 0; i < supportOptions.Length; i++)
            {
                if (angle > i * 45 && angle < (i + 1) * 45 && (_availableStrafeRequests > 0 || _availableExtractRequests > 0) && _requestAvailable)
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
            
            switch (_selectedSupportOption)
            {
                case ESupportType.Strafe:
                    if (_availableStrafeRequests > 0 && _requestAvailable)
                    {
                        _gesturesMenu.Close();
                        StaticManager.BeginCoroutine(FireSupportSpotter.Instance.SpotterSequence(ESupportType.Strafe));
                    }
                    break;
                case ESupportType.Extract:
                    if (_availableExtractRequests > 0 && _requestAvailable)
                    {
                        _gesturesMenu.Close();
                        StaticManager.BeginCoroutine(FireSupportSpotter.Instance.SpotterSequence(ESupportType.Extract));
                    }
                    break;
            }
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

        public IEnumerator StrafeRequest(Vector3 startingPosition, Vector3 endPosition)
        {
            _availableStrafeRequests--;
            _timerCoroutine = StaticManager.BeginCoroutine(Timer(Plugin.RequestCooldown.Value));
            FireSupportAudio.Instance.PlayVoiceover(EVoiceoverType.StationStrafeRequest);
            yield return new WaitForSecondsRealtime(8f);
            FireSupportAudio.Instance.PlayVoiceover(EVoiceoverType.JetArriving);
            yield return new WaitForSecondsRealtime(4f);
            A10Behaviour.Instance.StartStrafe(startingPosition, endPosition);
        }

        public IEnumerator ExtractionRequest(Vector3 position, Vector3 rotation)
        {
            _availableExtractRequests--;
            _requestAvailable = false;
            FireSupportAudio.Instance.PlayVoiceover(EVoiceoverType.StationExtractionRequest);
            yield return new WaitForSecondsRealtime(8f);
            UH60Behaviour.Instance.StartExtraction(position, rotation);
            FireSupportAudio.Instance.PlayVoiceover(EVoiceoverType.SupportHeliArrivingToPickup);
            yield return new WaitForSecondsRealtime(35f + Plugin.HelicopterWaitTime.Value);
            if (Instance == null) yield break;
            _timerCoroutine = StaticManager.BeginCoroutine(Timer(Plugin.RequestCooldown.Value));
        }

        private IEnumerator Timer(float time)
        {
            timerText.enabled = true;
            _requestAvailable = false;
            while (time > 0)
            {
                time -= Time.deltaTime;
                if (time < 0)
                    time = 0;

                float minutes = Mathf.FloorToInt(time / 60);
                float seconds = Mathf.FloorToInt(time % 60);
                timerText.text = $"{minutes:00}.{seconds:00}";
                yield return null;
            }
            _requestAvailable = true;
            timerText.enabled = false;
            if (_availableStrafeRequests > 0)
                FireSupportAudio.Instance.PlayVoiceover(EVoiceoverType.StationAvailable);
        }

        private void OnDestroy()
        {
            StaticManager.KillCoroutine(ref _timerCoroutine);
            Destroy(FireSupportAudio.Instance);
            Destroy(FireSupportSpotter.Instance);
            UtilsClass.UnloadBundle("firesupport_audio.bundle", true);
            UtilsClass.UnloadBundle("firesupport_spotter.bundle", true);
            UtilsClass.UnloadBundle("firesupport_ui.bundle", true);
            UtilsClass.UnloadBundle("a10_warthog.bundle", true);
            UtilsClass.UnloadBundle("uh60_blackhawk.bundle", true);
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