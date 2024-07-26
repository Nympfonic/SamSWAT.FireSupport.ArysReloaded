using Comfort.Common;
using EFT;
using SamSWAT.FireSupport.ArysReloaded.Utils;
using System;
using System.Collections;
using System.Threading.Tasks;
using UnityEngine;

namespace SamSWAT.FireSupport.ArysReloaded.Unity
{
    public class FireSupportSpotter : ScriptableObject
    {
        [SerializeField] private GameObject[] spotterParticles;
        private Vector3 _spotterPosition;
        private Vector3 _strafeStartPosition;
        private Vector3 _strafeEndPosition;
        private Vector3 _colliderRotation;
        private bool _requestCancelled;
        private GameObject _inputManager;
        private Player _player;

        public static async Task<FireSupportSpotter> Load()
        {
            var instance = await AssetLoader.LoadAssetAsync<FireSupportSpotter>("assets/content/ui/firesupport_spotter.bundle");

            while (InputManagerUtil.GetInputManager() == null)
            {
                await Task.Yield();
            }

            instance._inputManager = InputManagerUtil.GetInputManager().gameObject;
            instance._player = Singleton<GameWorld>.Instance.MainPlayer;
            return instance;
        }

        public IEnumerator SpotterSequence(ESupportType supportType, Action<bool, Vector3, Vector3> confirmation)
        {
            switch (supportType)
            {
                case ESupportType.Strafe:
                    yield return StaticManager.BeginCoroutine(SpotterVertical(false));
                    yield return StaticManager.BeginCoroutine(SpotterHorizontal());
                    yield return StaticManager.BeginCoroutine(SpotterConfirmation());
                    confirmation(_requestCancelled, _strafeStartPosition, _strafeEndPosition);
                    break;
                case ESupportType.Extract:
                    yield return StaticManager.BeginCoroutine(SpotterVertical(true));
                    yield return StaticManager.BeginCoroutine(SpotterConfirmation());
                    confirmation(_requestCancelled, _spotterPosition, _colliderRotation);
                    break;
            }
        }

        private IEnumerator SpotterVertical(bool checkSpace)
        {
            _requestCancelled = false;
            var spotterVertical = Instantiate(spotterParticles[0]);
            var colliderChecker = spotterVertical.GetComponentInChildren<ColliderReporter>();
            yield return new WaitForSeconds(.1f);
            while (!Input.GetMouseButtonDown(0))
            {
                if (IsRequestCancelled())
                {
                    Destroy(spotterVertical);
                    _requestCancelled = true;
                    FireSupportUI.Instance.SpotterNotice.SetActive(false);
                    FireSupportUI.Instance.SpotterHeliNotice.SetActive(false);
                    yield break;
                }

                var cameraT = _player.CameraPosition;
                Physics.Raycast(cameraT.position + cameraT.forward, cameraT.forward, out var hitInfo, 500,
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
                _requestCancelled = true;
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
            if (_requestCancelled) yield break;

            var spotterHorizontal = Instantiate(spotterParticles[1], _spotterPosition, Quaternion.identity);
            yield return new WaitForSeconds(.1f);
            _inputManager.SetActive(false);
            while (!Input.GetMouseButtonDown(0))
            {
                if (IsRequestCancelled())
                {
                    Destroy(spotterHorizontal);
                    _inputManager.SetActive(true);
                    _requestCancelled = true;
                    yield break;
                }
                float xAxisRotation = Input.GetAxis("Mouse X") * 5;
                spotterHorizontal.transform.Rotate(Vector3.down, xAxisRotation);
                yield return null;
            }

            _inputManager.SetActive(true);
            _strafeStartPosition = spotterHorizontal.transform.Find("Spotter Arrow Core (6)").position;
            _strafeEndPosition = spotterHorizontal.transform.Find("Spotter Arrow Core (1)").position;
            Destroy(spotterHorizontal);
        }

        private IEnumerator SpotterConfirmation()
        {
            if (_requestCancelled) yield break;

            var spotterConfirmation = Instantiate(spotterParticles[2], _spotterPosition + Vector3.up, Quaternion.identity);
            yield return new WaitForSeconds(.8f);
            Destroy(spotterConfirmation);
        }

        private bool IsRequestCancelled()
        {
            return (Input.GetMouseButtonDown(1) && Input.GetKey(KeyCode.LeftAlt))
                || (_player != null && _player.HandsController.Item.TemplateId != ItemConstants.RANGEFINDER_TPL);
        }
    }
}
