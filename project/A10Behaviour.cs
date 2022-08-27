using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Comfort.Common;
using System.Collections;
using UnityEngine;
using EFT;
using EFT.EnvironmentEffect;
using Random = UnityEngine.Random;
using EFT.UI;

namespace SamSWAT.FireSupport
{
    public class A10Behaviour : MonoBehaviour
    {
        [SerializeField] private AudioSource engineSource;
        [SerializeField] private AudioClip[] engineSounds;
        [SerializeField] private AudioClip[] gau8Sound;
        [SerializeField] private AudioClip[] gau8ExpSounds;
        [SerializeField] private Transform gau8Transform;
        [SerializeField] private GameObject gau8Particles;
        [SerializeField] private GameObject _flareCountermeasure;
        private bool _strafeRequested;
        private Vector3 _strafeStartPos;
        private Vector3 _strafeEndPos;
        private Vector3 _strafeMiddlePos;
        private float correction = 0;
        private float constantup = 0.037f;
        private float delaybs = 0.043f;
        private float rngmin = -0.007f;
        private float rngmax = 0.007f;
        private float gau8forward = 515;
        private float wait1 = 3;
        private float wait12 = 1;
        private float wait2 = 2;
        private float wait3 = 5;
        private float wait4 = 0;
        private float wait5 = 8;
        private float wait6 = 8;

        void Awake()
        {
            _flareCountermeasure.transform.parent = null;
        }

        public void StartStrafe(Vector3 strafeStartPos, Vector3 strafeEndPos)
        {
            _strafeStartPos = strafeStartPos;
            _strafeEndPos = strafeEndPos;
            _strafeMiddlePos = (strafeStartPos + strafeEndPos) / 2;
            Vector3 strafeBackVector = Vector3.Normalize(strafeStartPos - strafeEndPos);
            Vector3 a10StartPos = strafeStartPos + strafeBackVector * 2000 + 320 * Vector3.up;
            Vector3 a10Heading = strafeEndPos - a10StartPos;
            float a10YAngle = Mathf.Atan2(a10Heading.x, a10Heading.z) * Mathf.Rad2Deg;
            transform.SetPositionAndRotation(a10StartPos, Quaternion.Euler(0, a10YAngle, 0));
            gameObject.SetActive(true);
            engineSource.clip = GetRandomClip(engineSounds);
            engineSource.outputAudioMixerGroup = Singleton<BetterAudio>.Instance.OutEnvironment;
            engineSource.Play();
            _strafeRequested = true;
            StartCoroutine(FlySequence());
        }

        IEnumerator FlySequence()
        {
            _flareCountermeasure.SetActive(false);
            yield return new WaitForSecondsRealtime(wait1);
            gau8Particles.SetActive(true);
            FireSupportAudio.Instance.PlayVoiceover(EVoiceoverType.JetFiring);
            yield return new WaitForSecondsRealtime(wait12);
            StartCoroutine(Gau8Sequence());
            yield return new WaitForSecondsRealtime(wait2);
            Singleton<BetterAudio>.Instance.PlayAtPoint(_strafeMiddlePos, GetRandomClip(gau8ExpSounds), Distance(_strafeMiddlePos), BetterAudio.AudioSourceGroupType.Gunshots, 1200, 1, EOcclusionTest.Regular);
            gau8Particles.SetActive(false);
            yield return new WaitForSecondsRealtime(wait3);
            _flareCountermeasure.SetActive(true);
            yield return new WaitForSecondsRealtime(wait4); 
            Singleton<BetterAudio>.Instance.PlayAtPoint(gau8Transform.position - gau8Transform.forward * 100 - gau8Transform.up * 100, GetRandomClip(gau8Sound), Distance(gau8Transform.position), BetterAudio.AudioSourceGroupType.Gunshots, 3200, 2, EOcclusionTest.Regular);
            yield return new WaitForSecondsRealtime(wait5);
            FireSupportAudio.Instance.PlayVoiceover(EVoiceoverType.JetLeaving);
            yield return new WaitForSecondsRealtime(wait6);
            _strafeRequested = false;
            gameObject.SetActive(false);
        }

        //if it works - it works (ツ)
        IEnumerator Gau8Sequence()
        {
            Vector3 gau8Pos = gau8Transform.position + gau8Transform.forward * gau8forward;
            Vector3 gau8Dir = Vector3.Normalize(_strafeMiddlePos - gau8Pos + new Vector3(0, correction, 0));
            Vector3 gau8LeftDir = Vector3.Cross(gau8Dir, Vector3.up).normalized;
            Vector3 projectileDir;
            Vector3 leftRightSpread;
            var _projectile = WeaponClass.GetAmmo("ammo_30x173_gau8_avenger");
            int counter = 50;
            while (counter > 0)
            {
                leftRightSpread = gau8LeftDir * Random.Range(rngmin, rngmax);
                gau8Dir = Vector3.Normalize(gau8Dir + new Vector3(0, constantup/100, 0));
                projectileDir = Vector3.Normalize(gau8Dir + leftRightSpread);
                WeaponClass.FireProjectile(_projectile, gau8Pos, projectileDir, 1);
                counter--;
                yield return new WaitForSecondsRealtime(delaybs);
            }
        }

        void Update()
        {
            if (_strafeRequested)
            {
                _flareCountermeasure.transform.position = transform.position - transform.forward * 2;
                _flareCountermeasure.transform.eulerAngles = new Vector3(90, 0, transform.rotation.z);
                transform.Translate(0, 0, 148 * Time.deltaTime, Space.Self);
            }
        }

        private float Distance(Vector3 position)
        {
            if (Camera.main == null)
            {
                return float.MaxValue;
            }
            return Vector3.Distance(position, Camera.main.transform.position);
        }

        private AudioClip GetRandomClip(AudioClip[] audioClips)
        {
            return audioClips[Random.Range(0, audioClips.Length)];
        }
    }
}
