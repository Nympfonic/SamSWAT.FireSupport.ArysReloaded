using Comfort.Common;
using EFT;
using System.Collections;
using UnityEngine;

namespace SamSWAT.FireSupport.ArysReloaded.Unity
{
    public class UH60Behaviour : MonoBehaviour, IFireSupportOption
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

        public void ProcessRequest(Vector3 position, Vector3 direction, Vector3 rotation)
        {
            var heliTransform = transform;
            heliTransform.position = position;
            heliTransform.eulerAngles = rotation;
            helicopterAnimator.SetFloat(FlySpeedMultiplier, Plugin.HelicopterSpeedMultiplier.Value);
        }

        public void ReturnToPool()
        {
            gameObject.SetActive(false);
        }

        private void Update()
        {
            CrossFadeAudio();
        }
        
        private void CrossFadeAudio()
        {
            var player = Singleton<GameWorld>.Instance?.MainPlayer;
            if (player == null) return;

            float distance = Vector3.Distance(player.CameraPosition.position, rotorsCloseSource.transform.position);
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