using Comfort.Common;
using EFT.UI;
using System.Threading.Tasks;
using UnityEngine;

namespace SamSWAT.FireSupport
{
    [CreateAssetMenu(fileName = "FireSupportAudio", menuName = "ScriptableObjects/FireSupportAudio")]
    public class FireSupportAudio : ScriptableObject
    {
        [SerializeField] private AudioClip[] StationReminder;
        [SerializeField] private AudioClip[] StationStrafeRequest;
        [SerializeField] private AudioClip[] StationExtractionRequest;
        [SerializeField] private AudioClip[] JetArriving;
        [SerializeField] private AudioClip[] JetFiring;
        [SerializeField] private AudioClip[] JetLeaving;
        [SerializeField] private AudioClip[] SupportHeliArriving;
        [SerializeField] private AudioClip[] SupportHeliPickingUp;
        [SerializeField] private AudioClip[] SupportHeliLeaving;
        private static FireSupportAudio _instance;

        public static FireSupportAudio Instance
        {
            get
            {
                return _instance;
            }
        }

        public static async Task Load()
        {
            _instance = await Utils.LoadAssetAsync<FireSupportAudio>("assets/content/ui/firesupport_audio.bundle");
        }

        public void PlayVoiceover(EVoiceoverType voiceoverType)
        {
            AudioClip voAudioClip;

            switch (voiceoverType)
            {
                case EVoiceoverType.StationReminder:
                    voAudioClip = StationReminder[Random.Range(0, StationReminder.Length)];
                    break;
                case EVoiceoverType.StationStrafeRequest:
                    voAudioClip = StationStrafeRequest[Random.Range(0, StationStrafeRequest.Length)];
                    break;
                case EVoiceoverType.StationExtractionRequst:
                    voAudioClip = StationExtractionRequest[Random.Range(0, StationExtractionRequest.Length)];
                    break;
                case EVoiceoverType.JetArriving:
                    voAudioClip = JetArriving[Random.Range(0, JetArriving.Length)];
                    break;
                case EVoiceoverType.JetFiring:
                    voAudioClip = JetFiring[Random.Range(0, JetFiring.Length)];
                    break;
                case EVoiceoverType.JetLeaving:
                    voAudioClip = JetLeaving[Random.Range(0, JetLeaving.Length)];
                    break;
                case EVoiceoverType.SupportHeliArriving:
                    voAudioClip = SupportHeliArriving[Random.Range(0, JetLeaving.Length)];
                    break;
                case EVoiceoverType.SupportHeliPickingUp:
                    voAudioClip = SupportHeliPickingUp[Random.Range(0, JetLeaving.Length)];
                    break;
                case EVoiceoverType.SupportHeliLeaving:
                    voAudioClip = SupportHeliLeaving[Random.Range(0, JetLeaving.Length)];
                    break;
                default:
                    voAudioClip = null;
                    break;
            }

            if (voAudioClip != null)
            {
                Singleton<GUISounds>.Instance.PlaySound(voAudioClip);
            }
        }
    }
}