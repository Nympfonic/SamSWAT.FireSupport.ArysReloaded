using System;
using EFT;
using System.Collections;
using System.Linq;
using UnityEngine;

namespace SamSWAT.FireSupport
{
    public class FireSupportSpotter : ScriptableObject
    {
        [SerializeField] private GameObject[] spotterParticles;
        private Vector3 _spotterPosition;
        private Vector3 _strafeStartPosition;
        private Vector3 _strafeEndPosition;
        private Vector3 _colliderRotation;
        private bool _requestCanceled;
        private GameObject _inputManager;
        private Transform _mainCamera;

        public static FireSupportSpotter Instance { get; private set; }

        public static async void Load()
        {
            Instance = await Utils.LoadAssetAsync<FireSupportSpotter>("assets/content/ui/firesupport_spotter.bundle");
            Instance._inputManager = GameObject.Find("___Input");
            Instance._mainCamera = Camera.main.transform;
        }

        public IEnumerator SpotterSequence(ESupportType supportType)
        {
            switch (supportType)
            {
                case ESupportType.Strafe:
                    yield return StaticManager.BeginCoroutine(SpotterVertical(false));
                    yield return StaticManager.BeginCoroutine(SpotterHorizontal());
                    yield return StaticManager.BeginCoroutine(SpotterConfirmation());
                    if (!_requestCanceled)
                        StaticManager.BeginCoroutine(FireSupportUI.Instance.StrafeRequest(_strafeStartPosition, _strafeEndPosition));
                    break;
                case ESupportType.Extract:
                    yield return StaticManager.BeginCoroutine(SpotterVertical(true));
                    yield return StaticManager.BeginCoroutine(SpotterConfirmation());
                    if (!_requestCanceled)
                        StaticManager.BeginCoroutine(FireSupportUI.Instance.ExtractionRequest(_spotterPosition, _colliderRotation));
                    break;
            }
        }

        private IEnumerator SpotterVertical(bool checkSpace)
        {
            _requestCanceled = false;
            GameObject spotterVertical = Instantiate(spotterParticles[0]);
            var colliderChecker = spotterVertical.GetComponentInChildren<ColliderReporter>();
            yield return new WaitForSecondsRealtime(.1f);
            while (!Input.GetMouseButtonDown(0))
            {
                if (Input.GetMouseButtonDown(1) && Input.GetKey(KeyCode.LeftAlt))
                {
                    Destroy(spotterVertical);
                    _requestCanceled = true;
                    FireSupportUI.Instance.SpotterNotice.SetActive(false);
                    FireSupportUI.Instance.SpotterHeliNotice.SetActive(false);
                    yield break;
                }
                var forward = _mainCamera.forward;
                Physics.Raycast(_mainCamera.position + forward, forward, out var hitInfo, 500, 
                    LayerMask.GetMask("Terrain", "LowPolyCollider"));
                FireSupportUI.Instance.SpotterNotice.SetActive(hitInfo.point == Vector3.zero);  
                if (checkSpace && hitInfo.point != Vector3.zero)
                {
                    FireSupportUI.Instance.SpotterHeliNotice.SetActive(colliderChecker.HasCollision);
                    
                    if (colliderChecker.HasCollision)
                    {
                        var transform = colliderChecker.transform;
                        transform.Rotate(Vector3.up, 5f);
                        _colliderRotation = transform.eulerAngles;
                    }
                }
                spotterVertical.transform.position = hitInfo.point;
                yield return null;
            }
            if (spotterVertical.transform.position == Vector3.zero || checkSpace && colliderChecker.HasCollision)
            {
                _requestCanceled = true;
                FireSupportAudio.Instance.PlayVoiceover(EVoiceoverType.StationDoesNotHear);
                FireSupportUI.Instance.SpotterNotice.SetActive(false);
                FireSupportUI.Instance.SpotterHeliNotice.SetActive(false);
                Destroy(spotterVertical);
            }
            else
            {
                _spotterPosition = spotterVertical.transform.position;
                Destroy(spotterVertical);
            }
        }

        private IEnumerator SpotterHorizontal()
        {
            if (_requestCanceled) yield break;
            
            GameObject spotterHorizontal = Instantiate(spotterParticles[1], _spotterPosition, Quaternion.identity);
            yield return new WaitForSecondsRealtime(.1f);
            _inputManager.SetActive(false);
            while (!Input.GetMouseButtonDown(0))
            {
                if (Input.GetMouseButtonDown(1) && Input.GetKey(KeyCode.LeftAlt))
                {
                    Destroy(spotterHorizontal);
                    _inputManager.SetActive(true);
                    _requestCanceled = true;
                    yield break;
                }
                //float rotSpeed = 4f;
                float xAxisRotation = Input.GetAxis("Mouse X") * 5;
                spotterHorizontal.transform.Rotate(Vector3.down, xAxisRotation);
                yield return null;
            }
            _inputManager.SetActive(true);
            _strafeStartPosition = spotterHorizontal.transform.Find("Spotter Arrow Core (1)").position;
            _strafeEndPosition = spotterHorizontal.transform.Find("Spotter Arrow Core (6)").position;
            Destroy(spotterHorizontal);
        }

        private IEnumerator SpotterConfirmation()
        {
            if (_requestCanceled) yield break;
            
            GameObject spotterConfirmation = Instantiate(spotterParticles[2], _spotterPosition + Vector3.up, Quaternion.identity);
            yield return new WaitForSecondsRealtime(.8f);
            Destroy(spotterConfirmation);
        }
    }
}
