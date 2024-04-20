using SamSWAT.FireSupport.ArysReloaded.Utils;
using UnityEngine;

namespace SamSWAT.FireSupport.ArysReloaded.Unity.Vehicles.AH64
{
    public class AH64AudioController : MonoBehaviour, IBatchUpdate
    {
        public AnimationCurve volumeCurve;
        public AudioSource engineCloseSource;
        public AudioSource engineDistantSource;
        public AudioSource rotorsCloseSource;
        public AudioSource rotorsDistantSource;
        public AudioSource turbineSource;

        public AH64Audio ah64Audio;

        public void EnableAudioSources()
        {
            rotorsCloseSource.Play();
            rotorsDistantSource.Play();
            engineCloseSource.Play();
            engineDistantSource.Play();
            turbineSource.Play();
        }

        public void PlayAudio(Vector3 position, float distance, AH64WeaponSoundType weaponSoundType, int rolloff, float volume = 1, EOcclusionTest occlusionTest = EOcclusionTest.None)
        {
            AudioClip audioClip;

            switch (weaponSoundType)
            {
                case AH64WeaponSoundType.M230FiringClose:
                    audioClip = ModHelper.GetRandomClip(ah64Audio.m230CloseSounds);
                    break;
                case AH64WeaponSoundType.M230ExplosionClose:
                    audioClip = ModHelper.GetRandomClip(ah64Audio.m230CloseExpSounds);
                    break;
                case AH64WeaponSoundType.M230ExplosionReflectClose:
                    audioClip = ModHelper.GetRandomClip(ah64Audio.m230CloseExpReflectSounds);
                    break;
                case AH64WeaponSoundType.M230FiringDistant:
                    audioClip = ModHelper.GetRandomClip(ah64Audio.m230DistantSounds);
                    break;
                case AH64WeaponSoundType.M230ExplosionDistant:
                    audioClip = ModHelper.GetRandomClip(ah64Audio.m230DistantExpSounds);
                    break;
                case AH64WeaponSoundType.M230ExplosionReflectDistant:
                    audioClip = ModHelper.GetRandomClip(ah64Audio.m230DistantExpReflectSounds);
                    break;
                case AH64WeaponSoundType.Hydra70FiringClose:
                    audioClip = ModHelper.GetRandomClip(ah64Audio.hydra70CloseSounds);
                    break;
                case AH64WeaponSoundType.Hydra70ExplosionClose:
                    audioClip = ModHelper.GetRandomClip(ah64Audio.hydra70ExpCloseSounds);
                    break;
                case AH64WeaponSoundType.Hydra70FiringDistant:
                    audioClip = ModHelper.GetRandomClip(ah64Audio.hydra70DistantSounds);
                    break;
                case AH64WeaponSoundType.Hydra70ExplosionDistant:
                    audioClip = ModHelper.GetRandomClip(ah64Audio.hydra70ExpDistantSounds);
                    break;
                case AH64WeaponSoundType.Hydra70ExplosionTailDistant:
                    audioClip = ModHelper.GetRandomClip(ah64Audio.hydra70ExpTailDistantSounds);
                    break;
                default:
                    return;
            }

            if (audioClip == null) return;

            ModHelper.
            BetterAudio?.PlayAtPoint(position, audioClip, distance, BetterAudio.AudioSourceGroupType.Gunshots, rolloff, volume, occlusionTest);
        }

        public void BatchUpdate()
        {
            CrossFadeAudio();
        }

        private void Start()
        {
            var audioMixerGroup = ModHelper.BetterAudio?.OutEnvironment;
            rotorsCloseSource.outputAudioMixerGroup = audioMixerGroup;
            rotorsDistantSource.outputAudioMixerGroup = audioMixerGroup;
            engineCloseSource.outputAudioMixerGroup = audioMixerGroup;
            engineDistantSource.outputAudioMixerGroup = audioMixerGroup;
            turbineSource.outputAudioMixerGroup = audioMixerGroup;

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

            Vector3 camPos = player.CameraPosition.position;

            float rotorsDistance = Vector3.Distance(camPos, rotorsCloseSource.transform.position);
            float rotorsCloseVolume = 0.8f * volumeCurve.Evaluate(rotorsDistance);
            float rotorsDistantVolume = 0.35f * volumeCurve.Evaluate(rotorsDistance * 0.5f);
            rotorsCloseSource.volume = rotorsCloseVolume;
            rotorsDistantSource.volume = rotorsDistantVolume;

            float engineDistance = Vector3.Distance(camPos, engineCloseSource.transform.position);
            float engineCloseVolume = 0.6f * volumeCurve.Evaluate(engineDistance);
            float engineDistantVolume = 0.25f * volumeCurve.Evaluate(rotorsDistance * 0.5f);
            engineCloseSource.volume = engineCloseVolume;
            engineDistantSource.volume = engineDistantVolume;

            float turbineDistance = Vector3.Distance(camPos, turbineSource.transform.position);
            float turbineVolume = 0.2f * volumeCurve.Evaluate(turbineDistance * 12.5f);
            turbineSource.volume = turbineVolume;
        }
    }
}
