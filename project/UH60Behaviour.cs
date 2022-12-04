using System;
using System.Collections;
using Comfort.Common;
using UnityEngine;
using Random = UnityEngine.Random;

namespace SamSWAT.FireSupport
{
    public class UH60Behaviour : MonoBehaviour
    {
        [SerializeField] private Animator helicopterAnimator;
        [SerializeField] private AnimationCurve volumeCurve;
        [SerializeField] private AudioSource engineCloseSource;
        [SerializeField] private AudioSource engineDistantSource;
        [SerializeField] private AudioSource rotorsCloseSource;
        [SerializeField] private AudioSource rotorsDistantSource;
        private Transform _mainCamera;
        private GameObject _extractionPoint;
        private static readonly int FlySpeedMultiplier = Animator.StringToHash("FlySpeedMultiplier");
        private static readonly int FlyAway = Animator.StringToHash("FlyAway");

        public static UH60Behaviour Instance { get; private set; }

        public static async void Load()
        {
            Instance = Instantiate(
                await Utils.LoadAssetAsync<GameObject>("assets/content/vehicles/uh60_blackhawk.bundle"), 
                new Vector3(0, -200, 0), 
                Quaternion.identity).GetComponent<UH60Behaviour>();
            Instance.gameObject.SetActive(false);
            Instance._mainCamera = Camera.main.transform;
            var outputAudioMixerGroup = Singleton<BetterAudio>.Instance.OutEnvironment;
            Instance.engineCloseSource.outputAudioMixerGroup = outputAudioMixerGroup;
            Instance.engineDistantSource.outputAudioMixerGroup = outputAudioMixerGroup;
            Instance.rotorsCloseSource.outputAudioMixerGroup = outputAudioMixerGroup;
            Instance.rotorsDistantSource.outputAudioMixerGroup = outputAudioMixerGroup;
        }

        public void StartExtraction(Vector3 epPosition, Vector3 rotation)
        {
            var heliTransform = transform;
            heliTransform.position = epPosition;
            heliTransform.eulerAngles = rotation;
            gameObject.SetActive(true);
            helicopterAnimator.SetFloat(FlySpeedMultiplier, Plugin.HelicopterSpeedMultiplier.Value);
        }
        
        private void Update()
        {
            CrossFadeAudio();
        }
        
        private void CrossFadeAudio()
        {
            float distance = Vector3.Distance(_mainCamera.position, rotorsCloseSource.transform.position);
            float volume = volumeCurve.Evaluate(distance);

            rotorsCloseSource.volume = volume;
            engineCloseSource.volume = volume - 0.2f;
            rotorsDistantSource.volume = 1 - volume;
            engineDistantSource.volume = 1 - volume;
        }
        
        private IEnumerator OnHelicopterArrive()
        {
            FireSupportAudio.Instance.PlayVoiceover(EVoiceoverType.SupportHeliPickingUp);
            CreateExfilPoint();
            var waitTime = Plugin.HelicopterWaitTime.Value * 0.75f;
            yield return new WaitForSeconds(waitTime);
            FireSupportAudio.Instance.PlayVoiceover(EVoiceoverType.SupportHeliHurry);
            yield return new WaitForSeconds(Plugin.HelicopterWaitTime.Value - waitTime);
            helicopterAnimator.SetTrigger(FlyAway);
            Destroy(_extractionPoint);
            FireSupportAudio.Instance.PlayVoiceover(EVoiceoverType.SupportHeliLeavingNoPickup);
        }

        private void OnHelicopterLeft()
        {
            gameObject.SetActive(false);
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