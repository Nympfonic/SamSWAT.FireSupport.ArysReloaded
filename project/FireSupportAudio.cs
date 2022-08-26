using Comfort.Common;
using EFT.UI;
using UnityEngine;

namespace SamSWAT.FireSupport
{
    [CreateAssetMenu(fileName = "FireSupportAudio", menuName = "ScriptableObjects/FireSupportAudio")]
    public class FireSupportAudio : ScriptableObject
    {
        public AudioClip[] StationReminder;
        public AudioClip[] StationStrafeRequest;
        public AudioClip[] StationExtractionRequest;
        public AudioClip[] JetArriving;
        public AudioClip[] JetFiring;
        public AudioClip[] JetLeaving;
        public AudioClip[] SupportHeliArriving;
        public AudioClip[] SupportHeliPickingUp;
        public AudioClip[] SupportHeliLeaving;
        private static FireSupportAudio _instance;

        public static FireSupportAudio Instance
        {
            get
            {
                return _instance;
            }
        }

        void Awake()
        {
            _instance = this;
        }

        public void PlayVoiceover(VoiceoverType voiceoverType)
        {
            AudioClip voAudioClip;

            switch (voiceoverType)
            {
                case VoiceoverType.StationReminder:
                    voAudioClip = StationReminder[Random.Range(0, StationReminder.Length)];
                    break;
                case VoiceoverType.StationStrafeRequest:
                    voAudioClip = StationStrafeRequest[Random.Range(0, StationStrafeRequest.Length)];
                    break;
                case VoiceoverType.StationExtractionRequst:
                    voAudioClip = StationExtractionRequest[Random.Range(0, StationExtractionRequest.Length)];
                    break;
                case VoiceoverType.JetArriving:
                    voAudioClip = JetArriving[Random.Range(0, JetArriving.Length)];
                    break;
                case VoiceoverType.JetFiring:
                    voAudioClip = JetFiring[Random.Range(0, JetFiring.Length)];
                    break;
                case VoiceoverType.JetLeaving:
                    voAudioClip = JetLeaving[Random.Range(0, JetLeaving.Length)];
                    break;
                case VoiceoverType.SupportHeliArriving:
                    voAudioClip = SupportHeliArriving[Random.Range(0, JetLeaving.Length)];
                    break;
                case VoiceoverType.SupportHeliPickingUp:
                    voAudioClip = SupportHeliPickingUp[Random.Range(0, JetLeaving.Length)];
                    break;
                case VoiceoverType.SupportHeliLeaving:
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

    public enum VoiceoverType
    {
        StationReminder,
        StationStrafeRequest,
        StationExtractionRequst,
        JetArriving,
        JetFiring,
        JetLeaving,
        SupportHeliArriving,
        SupportHeliPickingUp,
        SupportHeliLeaving
    }
}