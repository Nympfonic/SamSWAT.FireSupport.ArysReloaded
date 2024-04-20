using SamSWAT.FireSupport.ArysReloaded.Unity.Vehicles;
using SamSWAT.FireSupport.ArysReloaded.Utils;
using System.Threading.Tasks;
using UnityEngine;

namespace SamSWAT.FireSupport.ArysReloaded.Unity.Interface
{
    public class FireSupportAudio : ScriptableObject
    {
        [SerializeField] private AudioClip[] stationReminder;
        [SerializeField] private AudioClip[] stationAvailable;
        [SerializeField] private AudioClip[] stationDoesNotHear;
        [SerializeField] private AudioClip[] stationStrafeRequest;
        [SerializeField] private AudioClip[] stationStrafeEnd;
        [SerializeField] private AudioClip[] stationExtractionRequest;
        [SerializeField] private AudioClip[] stationApacheRequest;
        [SerializeField] private AudioClip[] stationApacheEnd;
        [SerializeField] private AudioClip[] jetArriving;
        [SerializeField] private AudioClip[] jetFiring;
        [SerializeField] private AudioClip[] jetLeaving;
        [SerializeField] private AudioClip[] supportHeliArrivingToPickup;
        [SerializeField] private AudioClip[] supportHeliPickingUp;
        [SerializeField] private AudioClip[] supportHeliHurry;
        [SerializeField] private AudioClip[] supportHeliLeavingAfterPickup;
        [SerializeField] private AudioClip[] supportHeliLeavingNoPickup;
        [SerializeField] private AudioClip[] apacheArriving;
        [SerializeField] private AudioClip[] apacheFiringM230;
        [SerializeField] private AudioClip[] apacheFiringRockets;
        [SerializeField] private AudioClip[] apacheConfirmKills;
        [SerializeField] private AudioClip[] apachePause;
        [SerializeField] private AudioClip[] apacheLeaving;
        [SerializeField] private AudioClip[] apacheTakingFire;
        [SerializeField] private AudioClip[] apacheReceivingDamage;
        [SerializeField] private AudioClip[] apacheCrashing;

        public static FireSupportAudio Instance { get; private set; }

        public static async Task<FireSupportAudio> Load()
        {
            Instance = await AssetLoader.LoadAssetAsync<FireSupportAudio>("assets/content/ui/firesupport_audio.bundle");
            return Instance;
        }

        public void PlayVoiceover(VoiceoverType voiceoverType)
        {
            AudioClip voAudioClip;

            switch (voiceoverType)
            {
                case VoiceoverType.StationReminder:
                    voAudioClip = ModHelper.GetRandomClip(stationReminder);
                    break;
                case VoiceoverType.StationAvailable:
                    voAudioClip = ModHelper.GetRandomClip(stationAvailable);
                    break;
                case VoiceoverType.StationDoesNotHear:
                    voAudioClip = ModHelper.GetRandomClip(stationDoesNotHear);
                    break;
                case VoiceoverType.StationStrafeRequest:
                    voAudioClip = ModHelper.GetRandomClip(stationStrafeRequest);
                    break;
                case VoiceoverType.StationStrafeEnd:
                    voAudioClip = ModHelper.GetRandomClip(stationStrafeEnd);
                    break;
                case VoiceoverType.StationExtractionRequest:
                    voAudioClip = ModHelper.GetRandomClip(stationExtractionRequest);
                    break;
                case VoiceoverType.StationApacheRequest:
                    voAudioClip = ModHelper.GetRandomClip(stationApacheRequest);
                    break;
                case VoiceoverType.StationApacheEnd:
                    voAudioClip = ModHelper.GetRandomClip(stationApacheEnd);
                    break;
                case VoiceoverType.JetArriving:
                    voAudioClip = ModHelper.GetRandomClip(jetArriving);
                    break;
                case VoiceoverType.JetFiring:
                    voAudioClip = ModHelper.GetRandomClip(jetFiring);
                    break;
                case VoiceoverType.JetLeaving:
                    voAudioClip = ModHelper.GetRandomClip(jetLeaving);
                    break;
                case VoiceoverType.SupportHeliArrivingToPickup:
                    voAudioClip = ModHelper.GetRandomClip(supportHeliArrivingToPickup);
                    break;
                case VoiceoverType.SupportHeliPickingUp:
                    voAudioClip = ModHelper.GetRandomClip(supportHeliPickingUp);
                    break;
                case VoiceoverType.SupportHeliHurry:
                    voAudioClip = ModHelper.GetRandomClip(supportHeliHurry);
                    break;
                case VoiceoverType.SupportHeliLeavingAfterPickup:
                    voAudioClip = ModHelper.GetRandomClip(supportHeliLeavingAfterPickup);
                    break;
                case VoiceoverType.SupportHeliLeavingNoPickup:
                    voAudioClip = ModHelper.GetRandomClip(supportHeliLeavingNoPickup);
                    break;
                case VoiceoverType.ApacheArriving:
                    voAudioClip = ModHelper.GetRandomClip(apacheArriving);
                    break;
                case VoiceoverType.ApacheFiringM230:
                    voAudioClip = ModHelper.GetRandomClip(apacheFiringM230);
                    break;
                case VoiceoverType.ApacheFiringRockets:
                    voAudioClip = ModHelper.GetRandomClip(apacheFiringRockets);
                    break;
                case VoiceoverType.ApacheConfirmKills:
                    voAudioClip = ModHelper.GetRandomClip(apacheConfirmKills);
                    break;
                case VoiceoverType.ApachePause:
                    voAudioClip = ModHelper.GetRandomClip(apachePause);
                    break;
                case VoiceoverType.ApacheLeaving:
                    voAudioClip = ModHelper.GetRandomClip(apacheLeaving);
                    break;
                case VoiceoverType.ApacheTakingFire:
                    voAudioClip = ModHelper.GetRandomClip(apacheTakingFire);
                    break;
                case VoiceoverType.ApacheReceivingDamage:
                    voAudioClip = ModHelper.GetRandomClip(apacheReceivingDamage);
                    break;
                case VoiceoverType.ApacheCrashing:
                    voAudioClip = ModHelper.GetRandomClip(apacheCrashing);
                    break;
                default:
                    return;
            }

            if (voAudioClip == null) return;

            var sourceGroup = BetterAudio.AudioSourceGroupType.Nonspatial;
            var volume = Plugin.VoiceoverVolume.Value / 100f;
            ModHelper.            BetterAudio?.PlayNonspatial(voAudioClip, sourceGroup, 0, volume);
        }
    }
}