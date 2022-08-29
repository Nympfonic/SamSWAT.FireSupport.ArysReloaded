using Comfort.Common;
using EFT;
using EFT.UI;
using EFT.UI.Gestures;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace SamSWAT.FireSupport
{
    public class FireSupportUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        public GameObject SpotterNotice;
        [SerializeField] private FireSupportUIElement[] _supportOptions;
        [SerializeField] private Text _timerText;
        [SerializeField] private HoverTooltipArea _tooltip;
        private GesturesMenu _gesturesMenu;
        private Player _player;
        private ESupportType _selectedSupportOption;
        private float _menuOffset;
        private bool _isUnderPointer;
        private bool _strafeAvailable = true;
        private int _availableStrafeRequests;
        private static FireSupportUI _instance;

        public static FireSupportUI Instance
        {
            get
            {
                return _instance;
            }
        }

        public bool IsUnderPointer
        {
            get
            {
                return _isUnderPointer;
            }
            set
            {
                if (_isUnderPointer == value)
                {
                    return;
                }
                _isUnderPointer = value;
            }
        }

        public static async void Load(GesturesMenu gesturesMenu)
        {
            _instance = Instantiate(await Utils.LoadAssetAsync<GameObject>("assets/content/ui/firesupport_ui.bundle")).GetComponent<FireSupportUI>();
            _instance.transform.parent = gesturesMenu.transform;
            _instance.transform.localPosition = new Vector3(0, -255, 0);
            _instance.transform.localScale = new Vector3(1.4f, 1.4f, 1);
            _instance._menuOffset = Screen.height / 2 - _instance.transform.position.y;
            _instance._gesturesMenu = gesturesMenu;
            _instance._availableStrafeRequests = Plugin.AmmountOfRequets.Value;
            _instance._player = Singleton<GameWorld>.Instance.RegisteredPlayers[0];
            _instance.SpotterNotice.transform.parent = Singleton<GameUI>.Instance.transform;
            _instance.SpotterNotice.transform.localPosition = new Vector3(0, -370f, 0);
            _instance.SpotterNotice.transform.localScale = Vector3.one;
        }

        private void Update()
        {
            if (gameObject.activeInHierarchy)
            {
                RenderUI();
                HandleInput();
            }
        }

        private void RenderUI()
        {
            Color enabledColor = new Color(1, 1, 1, 1);
            Color disabledColor = new Color(1, 1, 1, 0.4f);
            bool rangefinderInHands = _player.HandsController.Item.TemplateId == "61605e13ffa6e502ac5e7eef";

            if (rangefinderInHands)
            {
                _tooltip.SetUnlockStatus(true);
                if (_strafeAvailable && _availableStrafeRequests > 0)
                {
                    _supportOptions[2].AmountText.color = enabledColor;
                    _supportOptions[2].Icon.color = enabledColor;
                }
                if (!_strafeAvailable || _availableStrafeRequests == 0)
                {
                    _supportOptions[2].IsUnderPointer = false;
                    _supportOptions[2].AmountText.color = disabledColor;
                    _supportOptions[2].Icon.color = disabledColor;
                }
                _supportOptions[2].AmountText.text = _availableStrafeRequests.ToString();
            }
            else
            {
                _tooltip.SetUnlockStatus(false);
            }
        }

        private void HandleInput()
        {
            if (IsUnderPointer)
            {
                bool rangefinderInHands = _player.HandsController.Item.TemplateId == "61605e13ffa6e502ac5e7eef";

                if (rangefinderInHands)
                {
                    float angle = CalculateAngle();

                    for (int i = 0; i < _supportOptions.Length; i++)
                    {
                        if (angle > i * 45 && angle < (i + 1) * 45 && _availableStrafeRequests > 0 && _strafeAvailable)
                        {
                            _supportOptions[i].IsUnderPointer = true;
                            _selectedSupportOption = (ESupportType)i;
                        }
                        else
                        {
                            _supportOptions[i].IsUnderPointer = false;
                        }
                    }

                    if (Input.GetMouseButtonDown(0))
                    {
                        switch (_selectedSupportOption)
                        {
                            case ESupportType.Strafe:
                                if (_availableStrafeRequests > 0 && _strafeAvailable)
                                {
                                    _gesturesMenu.Close();
                                    StaticManager.BeginCoroutine(FireSupportSpotter.Instance.SpotterSequence(ESupportType.Strafe));
                                }
                                break;
                        }
                    }
                }
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
            StaticManager.BeginCoroutine(Timer(Plugin.RequestCooldown.Value, result => _strafeAvailable = result));
            FireSupportAudio.Instance.PlayVoiceover(EVoiceoverType.StationStrafeRequest);
            yield return new WaitForSecondsRealtime(8f);
            FireSupportAudio.Instance.PlayVoiceover(EVoiceoverType.JetArriving);
            yield return new WaitForSecondsRealtime(4f);
            A10Behaviour.Instance.StartStrafe(startingPosition, endPosition);
        }

        private IEnumerator Timer(float time, Action<bool> requestAvailable)
        {
            _timerText.enabled = true;
            requestAvailable(false);
            while (time > 0)
            {
                time -= Time.deltaTime;
                if (time < 0)
                    time = 0;

                float minutes = Mathf.FloorToInt(time / 60);
                float seconds = Mathf.FloorToInt(time % 60);
                _timerText.text = string.Format("{0:00}.{1:00}", minutes, seconds);
                yield return null;
            }
            requestAvailable(true);
            _timerText.enabled = false;
            if (_availableStrafeRequests > 0)
                FireSupportAudio.Instance.PlayVoiceover(EVoiceoverType.StationAvailable);
        }

        void OnDestroy()
        {
            Destroy(FireSupportAudio.Instance);
            Destroy(FireSupportSpotter.Instance);
            Utils.UnloadBundle("firesupport_audio.bundle", true);
            Utils.UnloadBundle("firesupport_spotter.bundle", true);
            Utils.UnloadBundle("firesupport_ui.bundle", true);
            Utils.UnloadBundle("a10_warthog.bundle", true);
        }

        void IPointerEnterHandler.OnPointerEnter(PointerEventData data)
        {
            IsUnderPointer = true;
        }

        void IPointerExitHandler.OnPointerExit(PointerEventData data)
        {
            IsUnderPointer = false;

            for (int i = 0; i < _supportOptions.Length; i++)
            {
                _supportOptions[i].IsUnderPointer = false;
            }
        }
    }

}