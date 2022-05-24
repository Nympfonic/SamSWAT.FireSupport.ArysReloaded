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
        private bool _strafeRequested;
        //private Vector3 _strafeStartPos;
        //private Vector3 _strafeEndPos;
        private Vector3 _strafeMiddlePos;
        private object _projectile;

        private float aircraftbackmultiplier = 2000f;
        private float aircraftupmultiplier = 250f;
        private float aircraftspeed = 143f;
        private float wait1 = 2.5f;
        private float wait12 = 2.5f;
        private float wait2 = 2.33f;
        private float wait3 = 1.5f;
        private float wait4 = 3f;
        private float wait5 = 5f;
        private float finalwait = 10f;
        private int Counter = 50;
        private float xrandommin = -0.005f;
        private float xrandommax = 0.005f;
        private float yrandommin = 0.018f;
        private float yrandommax = 0.02f;
        //private float randommin2z = 0f;
        //private float randommax2z = 0f;
        private float speedfactor = 9f;
        private float timebetweenshots = 0.05f;
        //private Vector3 GAU8OldPos;
        public void StartStrafe(Vector3 strafeStartPos, Vector3 strafeEndPos)
        {
            //_strafeStartPos = strafeStartPos;
            //_strafeEndPos = strafeEndPos;
            _strafeMiddlePos = (strafeStartPos + strafeEndPos) / 2;
            Vector3 strafeBackVector = Vector3.Normalize(strafeStartPos - strafeEndPos);
            Vector3 a10StartPos = strafeStartPos + strafeBackVector * aircraftbackmultiplier + Vector3.up * aircraftupmultiplier;
            Vector3 a10Heading = strafeEndPos - a10StartPos;
            float a10Yangle = Mathf.Atan2(a10Heading.x, a10Heading.z) * Mathf.Rad2Deg;
            transform.SetPositionAndRotation(a10StartPos, Quaternion.Euler(0, a10Yangle, 0));
            gameObject.SetActive(true);
            engineSource.clip = Utils.GetRandomAudio(engineSounds);
            engineSource.outputAudioMixerGroup = Singleton<BetterAudio>.Instance.OutEnvironment;
            engineSource.Play();
            _strafeRequested = true;
            StartCoroutine(GAU8Sequence());
        }

        IEnumerator GAU8Sequence()
        {
            yield return new WaitForSecondsRealtime(wait1);
            gau8Particles.SetActive(true);
            Singleton<GUISounds>.Instance.PlaySound(Utils.FireSupportUI.JetFiring[7]);
            yield return new WaitForSecondsRealtime(wait12);
            //GAU8OldPos = GAU8Location.position;
            StartCoroutine(GAU8Shot());
            yield return new WaitForSecondsRealtime(wait2);
            //Vector3 ExpSoundPos = (strafeStartPos + strafeEndPos) / 2;
            Singleton<BetterAudio>.Instance.PlayAtPoint(_strafeMiddlePos, Utils.GetRandomAudio(gau8ExpSounds), Distance(_strafeMiddlePos), BetterAudio.AudioSourceGroupType.Gunshots, 1200, 1, EOcclusionTest.Fast);
            yield return new WaitForSecondsRealtime(wait3);
            gau8Particles.SetActive(false);
            yield return new WaitForSecondsRealtime(wait4); 
            Singleton<BetterAudio>.Instance.PlayAtPoint(gau8Transform.position + (gau8Transform.up * -100), Utils.GetRandomAudio(gau8Sound), Distance(gau8Transform.position), BetterAudio.AudioSourceGroupType.Gunshots, 2400, 1, EOcclusionTest.Fast);
            yield return new WaitForSecondsRealtime(wait5);
            Singleton<GUISounds>.Instance.PlaySound(Utils.FireSupportUI.JetLeaving[2]);
            yield return new WaitForSecondsRealtime(finalwait);
            _strafeRequested = false;
            gameObject.SetActive(false);
        }

        IEnumerator GAU8Shot()
        {
            Vector3 gau8Pos = gau8Transform.position;
            Vector3 gau8Dir = Vector3.Normalize(_strafeMiddlePos - gau8Pos + new Vector3(0, 0.018f, 0));
            Vector3 projectileDir;
            Vector3 leftRightSpread;
            Vector3 upDownSpread;
            //object projectile = WeaponClass.GetBullet("5d70e500a4b9364de70d38ce");
            _projectile = WeaponClass.GetBullet("5d70e500a4b9364de70d38ce");
            int counter = Counter;
            while (counter > 0)
            {
                leftRightSpread = new Vector3(Random.Range(xrandommin, xrandommax), 0, 0);
                upDownSpread = new Vector3(0, Random.Range(yrandommin, yrandommax), 0);
                projectileDir = Vector3.Normalize(gau8Dir + leftRightSpread + upDownSpread); //Vector3.Normalize
                //ProjectileDir = Quaternion.Euler(Random.Range(randomminx, randommaxx), Random.Range(randommin2y, randommax2y), Random.Range(randommin2z, randommax2z)) * GAU8Dir;
                WeaponClass.FireProjectile(_projectile, gau8Pos, projectileDir, speedfactor);
                counter--;
                yield return new WaitForSecondsRealtime(timebetweenshots);
            }
        }

        void Update()
        {
            if (_strafeRequested)
            {
                //transform.Translate(new Vector3(0, 0, 3f), Space.Self);
                transform.Translate(0, 0, aircraftspeed * Time.deltaTime, Space.Self);
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
    }
}
