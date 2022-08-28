using Comfort.Common;
using System.Collections;
using UnityEngine;
using Random = UnityEngine.Random;

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
        private GameObject _flareCountermeasureInstance;
        private static A10Behaviour _instance;
        private bool _strafeRequested;
        private Vector3 _strafeMiddlePos;

        public static A10Behaviour Instance
        {
            get
            {
                return _instance;
            }
        }

        public static async void Load()
        {
            _instance = Instantiate(await Utils.LoadAssetAsync<GameObject>("assets/content/vehicles/a10_warthog.bundle"), new Vector3(0, -200, 0), Quaternion.identity).GetComponent<A10Behaviour>();
            _instance.gameObject.SetActive(false);
        }

        public void StartStrafe(Vector3 strafeStartPos, Vector3 strafeEndPos)
        {
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
            _flareCountermeasureInstance = Instantiate(_flareCountermeasure, null);
            StartCoroutine(FlySequence());
        }

        //My main motto for next 2 methods is: if it works - it works (ツ)
        IEnumerator FlySequence()
        {
            _flareCountermeasureInstance.SetActive(false);
            yield return new WaitForSecondsRealtime(3);
            gau8Particles.SetActive(true);
            FireSupportAudio.Instance.PlayVoiceover(EVoiceoverType.JetFiring);
            yield return new WaitForSecondsRealtime(1);
            StartCoroutine(Gau8Sequence());
            yield return new WaitForSecondsRealtime(2);
            Singleton<BetterAudio>.Instance.PlayAtPoint(_strafeMiddlePos, GetRandomClip(gau8ExpSounds), Distance(_strafeMiddlePos), BetterAudio.AudioSourceGroupType.Gunshots, 1200, 1, EOcclusionTest.Regular);
            gau8Particles.SetActive(false);
            yield return new WaitForSecondsRealtime(5);
            _flareCountermeasureInstance.SetActive(true);
            Singleton<BetterAudio>.Instance.PlayAtPoint(gau8Transform.position - gau8Transform.forward * 100 - gau8Transform.up * 100, GetRandomClip(gau8Sound), Distance(gau8Transform.position), BetterAudio.AudioSourceGroupType.Gunshots, 3200, 2, EOcclusionTest.Regular);
            yield return new WaitForSecondsRealtime(8);
            FireSupportAudio.Instance.PlayVoiceover(EVoiceoverType.JetLeaving);
            yield return new WaitForSecondsRealtime(5);
            FireSupportAudio.Instance.PlayVoiceover(EVoiceoverType.StationStrafeEnd);
            yield return new WaitForSecondsRealtime(3);
            _strafeRequested = false;
            gameObject.SetActive(false);
        }

        IEnumerator Gau8Sequence()
        {
            Vector3 gau8Pos = gau8Transform.position + gau8Transform.forward * 515;
            Vector3 gau8Dir = Vector3.Normalize(_strafeMiddlePos - gau8Pos);
            Vector3 gau8LeftDir = Vector3.Cross(gau8Dir, Vector3.up).normalized;
            Vector3 projectileDir;
            Vector3 leftRightSpread;
            var _projectile = WeaponClass.GetAmmo("ammo_30x173_gau8_avenger");
            int counter = 50;
            while (counter > 0)
            {
                leftRightSpread = gau8LeftDir * Random.Range(-0.007f, 0.007f);
                gau8Dir = Vector3.Normalize(gau8Dir + new Vector3(0, 0.00037f, 0));
                projectileDir = Vector3.Normalize(gau8Dir + leftRightSpread);
                WeaponClass.FireProjectile(_projectile, gau8Pos, projectileDir, 1);
                counter--;
                yield return new WaitForSecondsRealtime(0.043f);
            }
        }

        void Update()
        {
            if (_strafeRequested)
            {
                _flareCountermeasureInstance.transform.position = transform.position - transform.forward * 6.5f;
                _flareCountermeasureInstance.transform.eulerAngles = new Vector3(90, 0, transform.rotation.y);
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
