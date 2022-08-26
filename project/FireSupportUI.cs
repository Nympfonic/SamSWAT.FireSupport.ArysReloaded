using Comfort.Common;
using EFT;
using EFT.UI;
using EFT.UI.Gestures;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace SamSWAT.FireSupport
{
    public class FireSupportUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [SerializeField] private FireSupportAudio _fireSupportAudio;
        [SerializeField] private List<GameObject> _spotterParticles;
        [SerializeField] private FireSupportUIElement[] _supportOptions;
        [SerializeField] private Text _timerText;
        [SerializeField] private HoverTooltipArea _tooltip;
        private A10Behaviour _a10Behaviour;
        private GesturesMenu _gesturesMenu;
        private Player _player;
        private GameObject _inputManager;
        private MonoBehaviour _coroutineStarter;
        private float _menuOffset;
        private bool _isUnderPointer;
        private bool _strafeAvailable = true;
        private int _availableStrafeRequests;
        private int _selectedSupportOption;

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

        public async void Init(GesturesMenu gesturesMenu)
        {
            _menuOffset = Screen.height / 2 - transform.position.y;
            _gesturesMenu = gesturesMenu;
            _availableStrafeRequests = Plugin.AmmountOfRequets.Value;
            _player = Singleton<GameWorld>.Instance.RegisteredPlayers[0];
            _inputManager = GameObject.Find("___Input");
            _coroutineStarter = Camera.main.GetComponent<MonoBehaviour>();
            var a10go = Instantiate(await Utils.LoadAssetAsync<GameObject>("assets/content/vehicles/a10_warthog.bundle"), new Vector3(0, -200, 0), Quaternion.identity);
            a10go.SetActive(false);
            _a10Behaviour = a10go.GetComponent<A10Behaviour>();
            Utils.UnloadBundle("a10_warthog.bundle");
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
                            _selectedSupportOption = i;
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
                            case 2:
                                if (_availableStrafeRequests > 0 && _strafeAvailable)
                                {
                                    _gesturesMenu.Close();
                                    _coroutineStarter.StartCoroutine(SupportRequest());
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

        //need to rewrite this mess later (ツ)
        private IEnumerator SupportRequest()
        {
            GameObject spotterVertical = Instantiate(_spotterParticles[0]);
            yield return new WaitForSecondsRealtime(.1f);
            while (!Input.GetMouseButtonDown(0))
            {
                if (Input.GetMouseButtonDown(1) && Input.GetKey(KeyCode.LeftAlt))
                {
                    Destroy(spotterVertical);
                    yield break;
                }
                var cum = Camera.main.transform;
                Physics.Raycast(cum.position + cum.forward, cum.forward, out RaycastHit hitInfo, 250f, LayerMask.GetMask("Terrain", "LowPolyCollider"));
                spotterVertical.transform.position = hitInfo.point;
                yield return null;
            }

            GameObject spotterHorizontal = Instantiate(_spotterParticles[1], spotterVertical.transform.position, Quaternion.identity);
            Destroy(spotterVertical);
            yield return new WaitForSecondsRealtime(.1f);
            _inputManager.SetActive(false);
            while (!Input.GetMouseButtonDown(0))
            {
                if (Input.GetMouseButtonDown(1) && Input.GetKey(KeyCode.LeftAlt))
                {
                    Destroy(spotterHorizontal);
                    _inputManager.SetActive(true);
                    yield break;
                }
                float rotSpeed = 4f;
                float xAxisRotation = Input.GetAxis("Mouse X") * rotSpeed;
                spotterHorizontal.transform.Rotate(Vector3.down, xAxisRotation);
                yield return null;
            }
            _inputManager.SetActive(true);
            Transform[] transforms = spotterHorizontal.GetComponentsInChildren<Transform>();
            Vector3 startingPosition = transforms.Single(x => x.name == "Spotter Arrow Core (1)").transform.position;
            Vector3 endPosition = transforms.Single(x => x.name == "Spotter Arrow Core (6)").transform.position;
            
            GameObject spotterConfirmation = Instantiate(_spotterParticles[2], spotterHorizontal.transform.position + Vector3.up, Quaternion.identity);
            yield return new WaitForSecondsRealtime(.8f);
            Destroy(spotterHorizontal);
            Destroy(spotterConfirmation);
            
            switch(_selectedSupportOption)
            {
                case 2:
                    _coroutineStarter.StartCoroutine(StrafeRequest(startingPosition, endPosition));
                    break;
            }
        }

        private IEnumerator StrafeRequest(Vector3 startingPosition, Vector3 endPosition)
        {
            _availableStrafeRequests--;
            _coroutineStarter.StartCoroutine(Timer(Plugin.RequestCooldown.Value, result => _strafeAvailable = result));
            FireSupportAudio.Instance.PlayVoiceover(VoiceoverType.StationStrafeRequest);
            yield return new WaitForSecondsRealtime(8f);
            FireSupportAudio.Instance.PlayVoiceover(VoiceoverType.JetArriving);
            yield return new WaitForSecondsRealtime(3f);
            _a10Behaviour.StartStrafe(startingPosition, endPosition);
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
