using EFT;
using SamSWAT.FireSupport.ArysReloaded.Unity.Vehicles;
using SamSWAT.FireSupport.ArysReloaded.Utils;
using System.Collections;
using System.Threading.Tasks;
using UnityEngine;

namespace SamSWAT.FireSupport.ArysReloaded.Unity.Interface
{
    public class FireSupportSpotter : ScriptableObject
    {
        [SerializeField] private GameObject[] spotterParticles;
        public Vector3 SpotterPosition { get; private set; }
        public Vector3 StrafeStartPosition { get; private set; }
        public Vector3 StrafeEndPosition { get; private set; }
        public Vector3 ColliderRotation { get; private set; }
        public bool RequestCancelled { get; private set; }
        private GameObject _inputManager;
        private Player _player;

        public static async Task<FireSupportSpotter> Load()
        {
            var instance = await AssetLoader.LoadAssetAsync<FireSupportSpotter>("assets/content/ui/firesupport_spotter.bundle");
            instance._inputManager = GameObject.Find("___Input");
            instance._player = ModHelper.MainPlayer;
            return instance;
        }

        public IEnumerator SpotterVertical(bool checkSpace)
        {
            RequestCancelled = false;
            var spotterVertical = Instantiate(spotterParticles[0]);
            var colliderChecker = spotterVertical.GetComponentInChildren<ColliderReporter>();
            yield return new WaitForSecondsRealtime(0.1f);
            while (!Input.GetMouseButtonDown(0))
            {
                if (IsRequestCancelled())
                {
                    Destroy(spotterVertical);
                    RequestCancelled = true;
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
                        ColliderRotation = transform.eulerAngles;
                    }
                }

                spotterVertical.transform.position = hitInfo.point;
                yield return null;
            }

            if (spotterVertical.transform.position == Vector3.zero || checkSpace && colliderChecker.HasCollision)
            {
                RequestCancelled = true;
                FireSupportAudio.Instance.PlayVoiceover(VoiceoverType.StationDoesNotHear);
                FireSupportUI.Instance.SpotterNotice.SetActive(false);
                FireSupportUI.Instance.SpotterHeliNotice.SetActive(false);
                Destroy(spotterVertical);
            }
            else
            {
                SpotterPosition = spotterVertical.transform.position;
                Destroy(spotterVertical);
            }
        }

        public IEnumerator SpotterHorizontal()
        {
            if (RequestCancelled) yield break;

            var spotterHorizontal = Instantiate(spotterParticles[1], SpotterPosition, Quaternion.identity);
            yield return new WaitForSecondsRealtime(0.1f);
            _inputManager.SetActive(false);
            while (!Input.GetMouseButtonDown(0))
            {
                if (IsRequestCancelled())
                {
                    Destroy(spotterHorizontal);
                    _inputManager.SetActive(true);
                    RequestCancelled = true;
                    yield break;
                }
                float xAxisRotation = Input.GetAxis("Mouse X") * 5;
                spotterHorizontal.transform.Rotate(Vector3.down, xAxisRotation);
                yield return null;
            }

            _inputManager.SetActive(true);
            StrafeStartPosition = spotterHorizontal.transform.Find("Spotter Arrow Core (6)").position;
            StrafeEndPosition = spotterHorizontal.transform.Find("Spotter Arrow Core (1)").position;
            Destroy(spotterHorizontal);
        }

        public IEnumerator SpotterConfirmation()
        {
            if (RequestCancelled) yield break;

            var spotterConfirmation = Instantiate(spotterParticles[2], SpotterPosition + Vector3.up, Quaternion.identity);
            yield return new WaitForSecondsRealtime(0.8f);
            Destroy(spotterConfirmation);
        }

        private bool IsRequestCancelled()
        {
            return Input.GetMouseButtonDown(1)
                && Input.GetKey(KeyCode.LeftAlt)
                || _player.HandsController.Item.TemplateId != ModHelper.RANGEFINDER_TPL;
        }
    }
}
