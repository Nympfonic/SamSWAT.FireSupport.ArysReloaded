using System.Threading.Tasks;
using UnityEngine;

namespace SamSWAT.FireSupport
{
    public class FireSupportAudio : ScriptableObject
    {
        [SerializeField] private AudioClip[] stationReminder;
        [SerializeField] private AudioClip[] stationAvailable;
        [SerializeField] private AudioClip[] stationDoesNotHear;
        [SerializeField] private AudioClip[] stationStrafeRequest;
        [SerializeField] private AudioClip[] stationStrafeEnd;
        [SerializeField] private AudioClip[] stationExtractionRequest;
        [SerializeField] private AudioClip[] jetArriving;
        [SerializeField] private AudioClip[] jetFiring;
        [SerializeField] private AudioClip[] jetLeaving;
        [SerializeField] private AudioClip[] supportHeliArriving;
        [SerializeField] private AudioClip[] supportHeliArrivingToPickup;
        [SerializeField] private AudioClip[] supportHeliPickingUp;
        [SerializeField] private AudioClip[] supportHeliHurry;
        [SerializeField] private AudioClip[] supportHeliLeaving;
        [SerializeField] private AudioClip[] supportHeliLeavingAfterPickup;
        [SerializeField] private AudioClip[] supportHeliLeavingNoPickup;
        
        public AudioSource AudioSource { get; private set; }
        public static FireSupportAudio Instance { get; private set; }

        public static async Task Load()
        {
            Instance = await Utils.LoadAssetAsync<FireSupportAudio>("assets/content/ui/firesupport_audio.bundle");
            Instance.AudioSource = new GameObject("FireSupportAudio").AddComponent<AudioSource>();
            Instance.AudioSource.volume = Plugin.VoiceoverVolume.Value/100f;
        }

        public void PlayVoiceover(EVoiceoverType voiceoverType)
        {
            AudioClip voAudioClip;

            switch (voiceoverType)
            {
                case EVoiceoverType.StationReminder:
                    voAudioClip = stationReminder[Random.Range(0, stationReminder.Length)];
                    break;
                case EVoiceoverType.StationAvailable:
                    voAudioClip = stationAvailable[Random.Range(0, stationAvailable.Length)];
                    break;
                case EVoiceoverType.StationDoesNotHear:
                    voAudioClip = stationDoesNotHear[Random.Range(0, stationDoesNotHear.Length)];
                    break;
                case EVoiceoverType.StationStrafeRequest:
                    voAudioClip = stationStrafeRequest[Random.Range(0, stationStrafeRequest.Length)];
                    break;
                case EVoiceoverType.StationStrafeEnd:
                    voAudioClip = stationStrafeEnd[Random.Range(0, stationStrafeEnd.Length)];
                    break;
                case EVoiceoverType.StationExtractionRequest:
                    voAudioClip = stationExtractionRequest[Random.Range(0, stationExtractionRequest.Length)];
                    break;
                case EVoiceoverType.JetArriving:
                    voAudioClip = jetArriving[Random.Range(0, jetArriving.Length)];
                    break;
                case EVoiceoverType.JetFiring:
                    voAudioClip = jetFiring[Random.Range(0, jetFiring.Length)];
                    break;
                case EVoiceoverType.JetLeaving:
                    voAudioClip = jetLeaving[Random.Range(0, jetLeaving.Length)];
                    break;
                case EVoiceoverType.SupportHeliArriving:
                    voAudioClip = supportHeliArriving[Random.Range(0, supportHeliArriving.Length)];
                    break;
                case EVoiceoverType.SupportHeliArrivingToPickup:
                    voAudioClip = supportHeliArrivingToPickup[Random.Range(0, supportHeliArrivingToPickup.Length)];
                    break;
                case EVoiceoverType.SupportHeliPickingUp:
                    voAudioClip = supportHeliPickingUp[Random.Range(0, supportHeliPickingUp.Length)];
                    break;
                case EVoiceoverType.SupportHeliHurry:
                    voAudioClip = supportHeliHurry[Random.Range(0, supportHeliHurry.Length)];
                    break;
                case EVoiceoverType.SupportHeliLeaving:
                    voAudioClip = supportHeliLeaving[Random.Range(0, supportHeliLeaving.Length)];
                    break;
                case EVoiceoverType.SupportHeliLeavingAfterPickup:
                    voAudioClip = supportHeliLeavingAfterPickup[Random.Range(0, supportHeliLeavingAfterPickup.Length)];
                    break;
                case EVoiceoverType.SupportHeliLeavingNoPickup:
                    voAudioClip = supportHeliLeavingNoPickup[Random.Range(0, supportHeliLeavingNoPickup.Length)];
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