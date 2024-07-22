using Comfort.Common;
using CommonAssets.Scripts.Game;
using EFT;
using EFT.UI;
using System.Collections;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace SamSWAT.FireSupport.ArysReloaded.Unity
{
    public class HeliExfiltrationPoint : MonoBehaviour, IPhysicsTrigger
    {
        private float _timer;
        private Coroutine _coroutine;
        private MethodInfo _stopSession;
        private BattleUIPanelExitTrigger _battleUIPanelExitTrigger;

        public string Description => "HeliExfiltrationPoint";

        private HeliExfiltrationPoint()
        {
            var t = typeof(EndByExitTrigerScenario).GetNestedTypes().Single(x => x.IsInterface);
            _stopSession = t.GetMethod("StopSession");
        }

        private void Start()
        {
            _battleUIPanelExitTrigger = Singleton<GameUI>.Instance.BattleUiPanelExitTrigger;
        }

        public void OnTriggerEnter(Collider collider)
        {
            var player = Singleton<GameWorld>.Instance.GetPlayerByCollider(collider);
            if (player == null || !player.IsYourPlayer) return;

            _timer = FireSupportPlugin.HelicopterExtractTime.Value;
            _battleUIPanelExitTrigger.Show(_timer);
            _coroutine = StartCoroutine(Timer(player.ProfileId));
        }

        public void OnTriggerExit(Collider collider)
        {
            var player = Singleton<GameWorld>.Instance.GetPlayerByCollider(collider);
            if (player == null || !player.IsYourPlayer) return;

            _timer = FireSupportPlugin.HelicopterExtractTime.Value;
            _battleUIPanelExitTrigger.Close();

            if (_coroutine == null) return;
            StopCoroutine(_coroutine);
        }

        private void OnDestroy()
        {
            if (Singleton<GameUI>.Instantiated)
            {
                _battleUIPanelExitTrigger.Close();
            }

            if (_coroutine == null) return;
            StopCoroutine(_coroutine);
        }

        private IEnumerator Timer(string profileId)
        {
            while (_timer > 0)
            {
                _timer -= Time.deltaTime;
                yield return null;
            }

            _stopSession.Invoke(Singleton<AbstractGame>.Instance, new object[]
            {
                profileId,
                ExitStatus.Survived,
                "UH-60 BlackHawk"
            });
        }
    }
}