using EFT;
using EFT.InputSystem;
using System.Collections;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

namespace SamSWAT.FireSupport
{
    public class FireSupportSpotter : ScriptableObject
    {
        [SerializeField] private GameObject[] _spotterParticles;
        private Vector3 _spotterPosition;
        private Vector3 _strafeStartPosition;
        private Vector3 _strafeEndPosition;
        private bool _requestCanceled;
        private GameObject _inputManager;
        private static FireSupportSpotter _instance;

        public static FireSupportSpotter Instance
        {
            get
            {
                return _instance;
            }
        }

        public static async Task Load()
        {
            _instance = await Utils.LoadAssetAsync<FireSupportSpotter>("assets/content/ui/firesupport_spotter.bundle");
            _instance._inputManager = FindObjectOfType<InputManager>().gameObject;
        }

        public IEnumerator SpotterSequence(ESupportType supportType)
        {
            if (supportType == ESupportType.Strafe)
            {
                yield return StaticManager.BeginCoroutine(SpotterVertical());
                yield return StaticManager.BeginCoroutine(SpotterHorizontal());
                yield return StaticManager.BeginCoroutine(SpotterConfirmation());
                if (!_requestCanceled)
                    FireSupportUI.Instance.StrafeRequest(_strafeStartPosition, _strafeEndPosition);
            }
            
        }

        private IEnumerator SpotterVertical()
        {
            _requestCanceled = false;
            GameObject spotterVertical = Instantiate(_spotterParticles[0]);
            yield return new WaitForSecondsRealtime(.1f);
            while (!Input.GetMouseButtonDown(0))
            {
                if (Input.GetMouseButtonDown(1) && Input.GetKey(KeyCode.LeftAlt))
                {
                    Destroy(spotterVertical);
                    _requestCanceled = true;
                    yield break;
                }
                var cum = Camera.main.transform;
                Physics.Raycast(cum.position + cum.forward, cum.forward, out RaycastHit hitInfo, 250f, LayerMask.GetMask("Terrain", "LowPolyCollider"));
                spotterVertical.transform.position = hitInfo.point;
                yield return null;
            }
            _spotterPosition = spotterVertical.transform.position;
            Destroy(spotterVertical);
        }

        private IEnumerator SpotterHorizontal()
        {
            if (!_requestCanceled)
            {
                GameObject spotterHorizontal = Instantiate(_spotterParticles[1], _spotterPosition, Quaternion.identity);
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
                    float rotSpeed = 4f;
                    float xAxisRotation = Input.GetAxis("Mouse X") * rotSpeed;
                    spotterHorizontal.transform.Rotate(Vector3.down, xAxisRotation);
                    yield return null;
                }
                _inputManager.SetActive(true);
                Transform[] transforms = spotterHorizontal.GetComponentsInChildren<Transform>();
                _strafeStartPosition = transforms.Single(x => x.name == "Spotter Arrow Core (1)").transform.position;
                _strafeEndPosition = transforms.Single(x => x.name == "Spotter Arrow Core (6)").transform.position;
                Destroy(spotterHorizontal);
            }
        }

        private IEnumerator SpotterConfirmation()
        {
            if (!_requestCanceled)
            {
                GameObject spotterConfirmation = Instantiate(_spotterParticles[2], _spotterPosition + Vector3.up, Quaternion.identity);
                yield return new WaitForSecondsRealtime(.8f);
                Destroy(spotterConfirmation);
            }
        }
    }
}
