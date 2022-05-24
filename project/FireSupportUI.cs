using UnityEngine;
using EFT.UI.Gestures;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Threading.Tasks;
using EFT;
using Comfort.Common;
using UnityEngine.UI;
using EFT.UI;

namespace SamSWAT.FireSupport
{
    public class FireSupportUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        public AudioClip[] StationReminder;
        public AudioClip[] StationStrafeRequest;
        public AudioClip[] StationExtractionRequest;
        public AudioClip[] JetArriving;
        public AudioClip[] JetFiring;
        public AudioClip[] JetLeaving;
        [SerializeField] private List<GameObject> spotterPatricles;
        [SerializeField] private FireSupportUIElement[] supportOptions;
        [SerializeField] private Text timerText;
        private GameObject _a10GameObject;
        private A10Behaviour _a10Behaviour;
        private GesturesMenu _gesturesMenu;
        private Player _player;
        private GameObject _inputManager;
        private MonoBehaviour _coroutineStarter;
        private Vector2 _mouse;
        private bool _isUnderPointer;
        private bool _timerActive;
        private int _availableRequests;
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
            _gesturesMenu = gesturesMenu;
            _availableRequests = Plugin.AmmountOfRequets.Value;
            _player = Singleton<GameWorld>.Instance.RegisteredPlayers[0];
            _inputManager = GameObject.Find("___Input");
            _coroutineStarter = Camera.main.GetComponent<MonoBehaviour>();
            GameObject a10go = await Utils.LoadAssetAsync("assets/content/vehicles/a10_warthog.bundle");
            _a10GameObject = Instantiate(a10go, new Vector3(0, -100, 0), Quaternion.identity);
            _a10GameObject.SetActive(false);
            _a10Behaviour = _a10GameObject.GetComponent<A10Behaviour>();
            //a10bundle.Unload(false);
        }

        private void Update()
        {
            if (gameObject.activeInHierarchy && IsUnderPointer)
            {
                bool rangefinderInHands = _player.HandsController.Item.TemplateId == "61605e13ffa6e502ac5e7eef";
                _mouse.x = Input.mousePosition.x - (Screen.width / 2f);
                _mouse.y = Input.mousePosition.y - (Screen.height / 2f) + 200;
                _mouse.Normalize();

                if (_mouse != Vector2.zero)
                {
                    float angle = Mathf.Atan2(_mouse.y, -_mouse.x) / Mathf.PI;
                    angle *= 180;
                    angle += 111;

                    if (angle < 0)
                    {
                        angle += 360;
                    }

                    for (int i = 0; i < supportOptions.Length; i++)
                    {
                        if (angle > i * 45 && angle < (i + 1) * 45 && _availableRequests > 0 && rangefinderInHands && !_timerActive)
                        {
                            supportOptions[i].IsUnderPointer = true;
                            supportOptions[i].AmountText?.color.SetAlpha(1f);
                            supportOptions[i].Icon?.color.SetAlpha(1f);
                            _selectedSupportOption = i;
                        }
                        else if (_timerActive || _availableRequests == 0)
                        {
                            supportOptions[i].IsUnderPointer = false;
                            supportOptions[i].AmountText?.color.SetAlpha(0.3f);
                            supportOptions[i].Icon?.color.SetAlpha(0.3f);
                        }
                        else
                        {
                            supportOptions[i].IsUnderPointer = false;
                        }
                    }
                }

                if (Input.GetMouseButtonDown(0) && rangefinderInHands)
                {
                    switch (_selectedSupportOption)
                    {
                        case 2:
                            if (_availableRequests > 0)
                                _gesturesMenu.Close();
                                _coroutineStarter.StartCoroutine(AutoCannonStrafeRequest());
                            break;
                    }
                }

            }
        }

        private IEnumerator AutoCannonStrafeRequest()
        {
            GameObject spotterVertical = Instantiate(spotterPatricles[0]);
            spotterVertical.SetActive(true);
            yield return new WaitForSecondsRealtime(.1f);
            while (!Input.GetMouseButtonDown(0))
            {
                if (Input.GetMouseButtonDown(1))
                {
                    Destroy(spotterVertical);
                    yield break;
                }
                var cum = Camera.main.transform;
                Physics.Raycast(cum.position + cum.forward, cum.forward, out RaycastHit hitInfo);
                spotterVertical.transform.position = hitInfo.point;
                yield return null;
            }

            GameObject spotterHorizontal = Instantiate(spotterPatricles[1]);
            spotterHorizontal.transform.position = spotterVertical.transform.position;
            Destroy(spotterVertical);
            spotterHorizontal.SetActive(true);
            yield return new WaitForSecondsRealtime(.1f);
            _inputManager.SetActive(false);
            while (!Input.GetMouseButtonDown(0))
            {
                if (Input.GetMouseButtonDown(1))
                {
                    Destroy(spotterHorizontal);
                    _inputManager.SetActive(true);
                    yield break;
                }
                float rotSpeed = 4f;
                float xaxisRotation = Input.GetAxis("Mouse X") * rotSpeed;
                spotterHorizontal.transform.Rotate(Vector3.down, xaxisRotation);
                yield return null;
            }
            _inputManager.SetActive(true);
            Transform[] transforms = spotterHorizontal.GetComponentsInChildren<Transform>();
            Vector3 startingPosition = transforms.Single(x => x.name == "Spotter Arrow Core (1)").transform.position;
            Vector3 endPosition = transforms.Single(x => x.name == "Spotter Arrow Core (6)").transform.position;

            GameObject spotterConfirmation = Instantiate(spotterPatricles[2]);
            spotterConfirmation.transform.position = spotterHorizontal.transform.position + new Vector3(0, 1.15f, 0);
            spotterConfirmation.SetActive(true);
            yield return new WaitForSecondsRealtime(.8f);
            Destroy(spotterHorizontal);
            Destroy(spotterConfirmation);
            _availableRequests--;
            supportOptions[_selectedSupportOption].AmountText.text = _availableRequests.ToString();
            AudioClip stationRequest = StationStrafeRequest[4];
            Singleton<GUISounds>.Instance.PlaySound(stationRequest);
            yield return new WaitForSecondsRealtime(stationRequest.length + 5f);
            Singleton<GUISounds>.Instance.PlaySound(JetArriving[7]);
            _a10Behaviour.StartStrafe(startingPosition, endPosition);
            _coroutineStarter.StartCoroutine(Timer(Plugin.RequestCooldown.Value));
        }

        private IEnumerator Timer(float time)
        {
            timerText.enabled = true;
            _timerActive = true;
            while (time > 0)
            {
                time -= Time.deltaTime;
                if (time < 0)
                    time = 0;

                float minutes = Mathf.FloorToInt(time / 60);
                float seconds = Mathf.FloorToInt(time % 60);
                timerText.text = string.Format("{0:00}.{1:00}", minutes, seconds);
                yield return null;
            }
            _timerActive = false;
            timerText.enabled = false;
        }
        void IPointerEnterHandler.OnPointerEnter(PointerEventData data)
        {
            IsUnderPointer = true;
        }

        void IPointerExitHandler.OnPointerExit(PointerEventData data)
        {
            IsUnderPointer = false;

            for (int i = 0; i < supportOptions.Length; i++)
            {
                supportOptions[i].IsUnderPointer = false;
            }
        }
    }

}
