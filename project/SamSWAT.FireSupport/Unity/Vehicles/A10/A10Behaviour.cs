using SamSWAT.FireSupport.ArysReloaded.Unity.Interface;
using SamSWAT.FireSupport.ArysReloaded.Utils;
using System.Collections;
using UnityEngine;
using Random = UnityEngine.Random;

namespace SamSWAT.FireSupport.ArysReloaded.Unity.Vehicles.A10
{
    public class A10Behaviour : VehicleBehaviour
    {
        public AudioSource engineSource;
        public AudioClip[] engineSounds;

        [SerializeField] private AudioClip[] gau8Sound;
        [SerializeField] private AudioClip[] gau8ExpSounds;
        [SerializeField] private Transform gau8Transform;
        [SerializeField] private GameObject gau8Particles;
        [SerializeField] private GameObject flareCountermeasure;

        private GameObject _flareCountermeasureInstance;
        private readonly BulletClass _gau8Ammo = WeaponClass.GetAmmo(ModHelper.GAU8_AMMO_TPL);

        public override string VehicleName => ModHelper.A10_NAME;

        public override void ProcessRequest(Vector3 position, Vector3 direction, Vector3 rotation)
        {
            Vector3 a10StartPos = position + direction * 2650 + 320 * Vector3.up;
            Vector3 a10Heading = position - a10StartPos;
            float a10YAngle = Mathf.Atan2(a10Heading.x, a10Heading.z) * Mathf.Rad2Deg;
            transform.SetPositionAndRotation(a10StartPos, Quaternion.Euler(0, a10YAngle, 0));
            gameObject.SetActive(true);
            _flareCountermeasureInstance = Instantiate(flareCountermeasure, null);
            StartCoroutine(FlySequence(position));
        }

        public override void BatchUpdate()
        {
            if (!gameObject.activeSelf || _flareCountermeasureInstance == null) 
            {
                return;
            }

            var t = transform;
            _flareCountermeasureInstance.transform.position = t.position - t.forward * 6.5f;
            _flareCountermeasureInstance.transform.eulerAngles = new Vector3(90, t.eulerAngles.y, 0);
            transform.Translate(0, 0, 148 * Time.deltaTime, Space.Self);
        }

        private void Start()
        {
            UpdateManager.Instance.RegisterSlicedUpdate(this, UpdateManager.UpdateMode.Always);
        }

        private void OnDestroy()
        {
            UpdateManager.Instance.DeregisterSlicedUpdate(this);
        }

        //My main motto for next 2 methods is: if it works - it works (ツ)
        private IEnumerator FlySequence(Vector3 strafePos)
        {
            var betterAudio = ModHelper.BetterAudio;
            var player = ModHelper.MainPlayer;

            yield return new WaitForSecondsRealtime(3f);
            engineSource.clip = engineSounds[Random.Range(0, engineSounds.Length)];
            engineSource.outputAudioMixerGroup = betterAudio.OutEnvironment;
            engineSource.Play();
            yield return new WaitForSecondsRealtime(1f);
            _flareCountermeasureInstance.SetActive(false);
            yield return new WaitForSecondsRealtime(3f);
            gau8Particles.SetActive(true);
            FireSupportAudio.Instance.PlayVoiceover(VoiceoverType.JetFiring);
            yield return new WaitForSecondsRealtime(1f);
            StartCoroutine(Gau8Sequence(strafePos));
            yield return new WaitForSecondsRealtime(2f);
            betterAudio.PlayAtPoint(
                strafePos,
                ModHelper.GetRandomClip(gau8ExpSounds),
                Vector3.Distance(player.CameraPosition.position, strafePos),
                BetterAudio.AudioSourceGroupType.Gunshots,
                1200,
                1,
                EOcclusionTest.Regular);
            gau8Particles.SetActive(false);
            yield return new WaitForSecondsRealtime(4f);
            betterAudio.PlayAtPoint(
                gau8Transform.position - gau8Transform.forward * 100 - gau8Transform.up * 100,
                ModHelper.GetRandomClip(gau8Sound),
                Vector3.Distance(player.CameraPosition.position, gau8Transform.position),
                BetterAudio.AudioSourceGroupType.Gunshots,
                3200,
                2);
            yield return new WaitForSecondsRealtime(1f);
            _flareCountermeasureInstance.SetActive(true);
            yield return new WaitForSecondsRealtime(8f);
            FireSupportAudio.Instance.PlayVoiceover(VoiceoverType.JetLeaving);
            yield return new WaitForSecondsRealtime(4f);
            FireSupportAudio.Instance.PlayVoiceover(VoiceoverType.StationStrafeEnd);
            yield return new WaitForSecondsRealtime(4f);
            ReturnToPool();
        }

        private IEnumerator Gau8Sequence(Vector3 strafePos)
        {
            Vector3 gau8Pos = gau8Transform.position + gau8Transform.forward * 515;
            Vector3 gau8Dir = Vector3.Normalize(strafePos - gau8Pos);
            Vector3 gau8LeftDir = Vector3.Cross(gau8Dir, Vector3.up).normalized;
            int counter = 50;
            while (counter > 0)
            {
                Vector3 horizontalSpread = gau8LeftDir * Random.Range(-0.012f, 0.012f);
                gau8Dir = Vector3.Normalize(gau8Dir + new Vector3(0, 0.00037f, 0));
                Vector3 projectileDir = Vector3.Normalize(gau8Dir + horizontalSpread);
                WeaponClass.FireProjectile(WeaponType.GAU8, _gau8Ammo, gau8Pos, projectileDir);
                counter--;
                yield return new WaitForSecondsRealtime(0.043f);
            }
        }
    }
}
