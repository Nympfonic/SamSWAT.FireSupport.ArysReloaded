using Comfort.Common;
using EFT.UI;
using System.Threading.Tasks;
using UnityEngine;

namespace SamSWAT.FireSupport
{
    public class FireSupportAudio : ScriptableObject
    {
        [SerializeField] private AudioClip[] _stationReminder;
        [SerializeField] private AudioClip[] _stationAvailable;
        [SerializeField] private AudioClip[] _stationDoesNotHear;
        [SerializeField] private AudioClip[] _stationStrafeRequest;
        [SerializeField] private AudioClip[] _stationStrafeEnd;
        [SerializeField] private AudioClip[] _stationExtractionRequest;
        [SerializeField] private AudioClip[] _jetArriving;
        [SerializeField] private AudioClip[] _jetFiring;
        [SerializeField] private AudioClip[] _jetLeaving;
        [SerializeField] private AudioClip[] _supportHeliArriving;
        [SerializeField] private AudioClip[] _supportHeliPickingUp;
        [SerializeField] private AudioClip[] _supportHeliLeaving;
        public AudioSource AudioSource { get; private set; }
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
            _instance.AudioSource = new GameObject("FireSupportAudio").AddComponent<AudioSource>();
            _instance.AudioSource.volume = (float)Plugin.VoiceoverVolume.Value/100;
        }

        public void PlayVoiceover(EVoiceoverType voiceoverType)
        {
            AudioClip voAudioClip;

            switch (voiceoverType)
            {
                case EVoiceoverType.StationReminder:
                    voAudioClip = _stationReminder[Random.Range(0, _stationReminder.Length)];
                    break;
                case EVoiceoverType.StationAvailable:
                    voAudioClip = _stationAvailable[Random.Range(0, _stationAvailable.Length)];
                    break;
                case EVoiceoverType.StationDoesNotHear:
                    voAudioClip = _stationDoesNotHear[Random.Range(0, _stationDoesNotHear.Length)];
                    break;
                case EVoiceoverType.StationStrafeRequest:
                    voAudioClip = _stationStrafeRequest[Random.Range(0, _stationStrafeRequest.Length)];
                    break;
                case EVoiceoverType.StationStrafeEnd:
                    voAudioClip = _stationStrafeEnd[Random.Range(0, _stationStrafeEnd.Length)];
                    break;
                case EVoiceoverType.StationExtractionRequst:
                    voAudioClip = _stationExtractionRequest[Random.Range(0, _stationExtractionRequest.Length)];
                    break;
                case EVoiceoverType.JetArriving:
                    voAudioClip = _jetArriving[Random.Range(0, _jetArriving.Length)];
                    break;
                case EVoiceoverType.JetFiring:
                    voAudioClip = _jetFiring[Random.Range(0, _jetFiring.Length)];
                    break;
                case EVoiceoverType.JetLeaving:
                    voAudioClip = _jetLeaving[Random.Range(0, _jetLeaving.Length)];
                    break;
                case EVoiceoverType.SupportHeliArriving:
                    voAudioClip = _supportHeliArriving[Random.Range(0, _supportHeliArriving.Length)];
                    break;
                case EVoiceoverType.SupportHeliPickingUp:
                    voAudioClip = _supportHeliPickingUp[Random.Range(0, _supportHeliPickingUp.Length)];
                    break;
                case EVoiceoverType.SupportHeliLeaving:
                    voAudioClip = _supportHeliLeaving[Random.Range(0, _supportHeliLeaving.Length)];
                    break;
                default:
                    voAudioClip = null;
                    break;
            }

            if (voAudioClip != null)
            {
                AudioSource.PlayOneShot(voAudioClip);
            }
        }
    }
}