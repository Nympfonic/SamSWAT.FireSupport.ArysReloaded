using Comfort.Common;
using EFT;
using SamSWAT.FireSupport.ArysReloaded.Unity.Interface;
using SamSWAT.FireSupport.ArysReloaded.Utils;
using System.Collections;
using UnityEngine;

namespace SamSWAT.FireSupport.ArysReloaded.Unity.Vehicles.UH60
{
    public class UH60Behaviour : VehicleBehaviour
    {
        private static readonly int FlySpeedMultiplier = Animator.StringToHash("FlySpeedMultiplier");
        private static readonly int FlyAway = Animator.StringToHash("FlyAway");
        [SerializeField] private Animator helicopterAnimator;
        [SerializeField] private AnimationCurve volumeCurve;
        public AudioSource engineCloseSource;
        public AudioSource engineDistantSource;
        public AudioSource rotorsCloseSource;
        public AudioSource rotorsDistantSource;
        private GameObject _extractionPoint;
        private bool _isLeaving = false;

        public override string VehicleName => ModHelper.UH60_NAME;

        public override void ProcessRequest(Vector3 position, Vector3 direction, Vector3 rotation)
        {
            transform.position = position;
            transform.eulerAngles = rotation;
            gameObject.SetActive(true);

            var audioMixerGroup = Singleton<BetterAudio>.Instance.OutEnvironment;
            rotorsCloseSource.outputAudioMixerGroup = audioMixerGroup;
            rotorsDistantSource.outputAudioMixerGroup = audioMixerGroup;
            engineCloseSource.outputAudioMixerGroup = audioMixerGroup;
            engineDistantSource.outputAudioMixerGroup = audioMixerGroup;
            helicopterAnimator.SetFloat(FlySpeedMultiplier, Plugin.HelicopterSpeedMultiplier.Value);
        }

        public override void BatchUpdate()
        {
            CrossFadeAudio();
        }

        public bool IsLeaving()
        {
            return _isLeaving;
        }

        private void Start()
        {
            UpdateManager.Instance.RegisterSlicedUpdate(this, UpdateManager.UpdateMode.Always);
        }

        private void OnDestroy()
        {
            UpdateManager.Instance.DeregisterSlicedUpdate(this);
        }

        private void CrossFadeAudio()
        {
            var player = ModHelper.MainPlayer;
            if (player == null) return;

            float distance = Vector3.Distance(player.CameraPosition.position, rotorsCloseSource.transform.position);
            float volume = volumeCurve.Evaluate(distance);
            float distantVolume = 0.35f * volumeCurve.Evaluate(distance * 0.5f);

            rotorsCloseSource.volume = volume;
            engineCloseSource.volume = volume - 0.2f;
            rotorsDistantSource.volume = distantVolume;
            engineDistantSource.volume = distantVolume;
        }

        private IEnumerator OnHelicopterArrive()
        {
            var fsAudio = FireSupportAudio.Instance;
            fsAudio.PlayVoiceover(VoiceoverType.SupportHeliPickingUp);
            CreateExfilPoint();
            var waitTime = Plugin.HelicopterWaitTime.Value * 0.75f;
            yield return new WaitForSeconds(waitTime);
            fsAudio.PlayVoiceover(VoiceoverType.SupportHeliHurry);
            yield return new WaitForSeconds(Plugin.HelicopterWaitTime.Value - waitTime);
            _isLeaving = true;
            helicopterAnimator.SetTrigger(FlyAway);
            Destroy(_extractionPoint);
            fsAudio.PlayVoiceover(VoiceoverType.SupportHeliLeavingNoPickup);
        }

        private void OnHelicopterLeft()
        {
            ReturnToPool();
        }

        private void CreateExfilPoint()
        {
            _extractionPoint = new GameObject
            {
                name = "HeliExfilPoint",
                layer = 13,
                transform =
                {
                    position = transform.position,
                    eulerAngles = new Vector3(-90,0,0),
                }
            };
            var extractionCollider = _extractionPoint.AddComponent<BoxCollider>();
            extractionCollider.size = new Vector3(16.5f, 20f, 15);
            extractionCollider.isTrigger = true;
            _extractionPoint.AddComponent<HeliExfiltrationPoint>();
        }
    }
}