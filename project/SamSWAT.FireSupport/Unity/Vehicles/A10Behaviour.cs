﻿using Comfort.Common;
using EFT;
using SamSWAT.FireSupport.ArysReloaded.Utils;
using System.Collections;
using UnityEngine;
using Random = UnityEngine.Random;

namespace SamSWAT.FireSupport.ArysReloaded.Unity
{
    public class A10Behaviour : MonoBehaviour, IComponent, IFireSupportOption
    {
        public AudioSource engineSource;
        public AudioClip[] engineSounds;
        [SerializeField] private AudioClip[] gau8Sound;
        [SerializeField] private AudioClip[] gau8ExpSounds;
        [SerializeField] private Transform gau8Transform;
        [SerializeField] private GameObject gau8Particles;
        [SerializeField] private GameObject flareCountermeasure;
        private GameObject _flareCountermeasureInstance;

        public void ProcessRequest(Vector3 position, Vector3 direction, Vector3 rotation)
        {
            Vector3 a10StartPos = position + direction * 2650 + 320 * Vector3.up;
            Vector3 a10Heading = position - a10StartPos;
            float a10YAngle = Mathf.Atan2(a10Heading.x, a10Heading.z) * Mathf.Rad2Deg;
            transform.SetPositionAndRotation(a10StartPos, Quaternion.Euler(0, a10YAngle, 0));
            _flareCountermeasureInstance = Instantiate(flareCountermeasure, null);
            StartCoroutine(FlySequence(position));
        }

        public void ManualUpdate()
        {
            if (!gameObject.activeSelf) return;
            if (_flareCountermeasureInstance == null) return;

            var t = transform;
            _flareCountermeasureInstance.transform.position = t.position - t.forward * 6.5f;
            _flareCountermeasureInstance.transform.eulerAngles = new Vector3(90, t.eulerAngles.y, 0);
            transform.Translate(0, 0, 148 * Time.deltaTime, Space.Self);
        }

        public void ReturnToPool()
        {
            gameObject.SetActive(false);
        }

        private void Start()
        {
            FireSupportPlugin.RegisterComponent(this);
        }

        private void OnDestroy()
        {
            FireSupportPlugin.DeregisterComponent(this);
        }

        //My main motto for next 2 methods is: if it works - it works (ツ)
        private IEnumerator FlySequence(Vector3 strafePos)
        {
            var betterAudio = Singleton<BetterAudio>.Instance;
            var fireSupportAudio = FireSupportAudio.Instance;
            var player = Singleton<GameWorld>.Instance.MainPlayer;

            yield return new WaitForSeconds(3f);
            engineSource.clip = engineSounds[Random.Range(0, engineSounds.Length)];
            engineSource.outputAudioMixerGroup = Singleton<BetterAudio>.Instance.OutEnvironment;
            engineSource.Play();

            yield return new WaitForSeconds(1f);
            _flareCountermeasureInstance.SetActive(false);

            yield return new WaitForSeconds(3f);
            gau8Particles.SetActive(true);
            fireSupportAudio.PlayVoiceover(EVoiceoverType.JetFiring);

            yield return new WaitForSeconds(1f);
            StartCoroutine(Gau8Sequence(strafePos));

            yield return new WaitForSeconds(2f);
            if (player == null || !player.ActiveHealthController.IsAlive) yield break;
            betterAudio.PlayAtPoint(
                strafePos,
                GetRandomClip(gau8ExpSounds),
                Vector3.Distance(player.CameraPosition.position, strafePos),
                BetterAudio.AudioSourceGroupType.Gunshots,
                1200,
                1,
                EOcclusionTest.Regular
            );
            gau8Particles.SetActive(false);

            yield return new WaitForSeconds(3.5f);
            if (player == null || !player.ActiveHealthController.IsAlive) yield break;
            betterAudio.PlayAtPoint(
                gau8Transform.position - gau8Transform.forward * 100 - gau8Transform.up * 100,
                GetRandomClip(gau8Sound),
                Vector3.Distance(player.CameraPosition.position, gau8Transform.position),
                BetterAudio.AudioSourceGroupType.Gunshots,
                3200,
                2
            );

            yield return new WaitForSeconds(1.5f);
            _flareCountermeasureInstance.SetActive(true);

            yield return new WaitForSeconds(8f);
            FireSupportAudio.Instance.PlayVoiceover(EVoiceoverType.JetLeaving);

            yield return new WaitForSeconds(4f);
            FireSupportAudio.Instance.PlayVoiceover(EVoiceoverType.StationStrafeEnd);

            yield return new WaitForSeconds(4f);
            ReturnToPool();
        }

        private IEnumerator Gau8Sequence(Vector3 strafePos)
        {
            var player = Singleton<GameWorld>.Instance.MainPlayer;

            Vector3 gau8Pos = gau8Transform.position + gau8Transform.forward * 515;
            Vector3 gau8Dir = Vector3.Normalize(strafePos - gau8Pos);
            Vector3 gau8LeftDir = Vector3.Cross(gau8Dir, Vector3.up).normalized;
            int counter = 50;
            while (counter > 0)
            {
                if (player == null || !player.ActiveHealthController.IsAlive)
                {
                    yield break;
                }

                var projectile = WeaponClass.GetAmmo(ItemConstants.GAU8_AMMO_TPL);
                Vector3 leftRightSpread = gau8LeftDir * Random.Range(-0.007f, 0.007f);
                gau8Dir = Vector3.Normalize(gau8Dir + new Vector3(0, 0.00037f, 0));
                Vector3 projectileDir = Vector3.Normalize(gau8Dir + leftRightSpread);
                WeaponClass.FireProjectile(projectile, gau8Pos, projectileDir);
                counter--;
                yield return new WaitForSeconds(0.043f);
            }
        }

        private AudioClip GetRandomClip(AudioClip[] audioClips)
        {
            return audioClips[Random.Range(0, audioClips.Length)];
        }
    }
}
