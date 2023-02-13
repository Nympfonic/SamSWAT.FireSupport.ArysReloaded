using System.Collections;
using System.Linq;
using Comfort.Common;
using CommonAssets.Scripts.Game;
using EFT;
using EFT.UI;
using UnityEngine;

namespace SamSWAT.FireSupport.Unity
{
    public class HeliExfiltrationPoint : MonoBehaviour, IPhysicsTrigger
    {
        private float _timer;
        public string Description => "HeliExfiltrationPoint";
        private Coroutine _coroutine;

        public void OnTriggerEnter(Collider other)
        {
            var player = Singleton<GameWorld>.Instance.GetPlayerByCollider(other);
            if (player == null || !player.IsYourPlayer) return;
            
            _timer = Plugin.HelicopterExtractTime.Value;
            Singleton<GameUI>.Instance.BattleUiPanelExitTrigger.Show(_timer);
            _coroutine = StartCoroutine(Timer(player.ProfileId));
        }

        private IEnumerator Timer(string profileId)
        {
            while (_timer > 0)
            {
                _timer -= Time.deltaTime;
                yield return null;
            }

            var t = typeof(EndByExitTrigerScenario).GetNestedTypes().Single(x => x.IsInterface);
            t.GetMethod("StopSession").Invoke(Singleton<AbstractGame>.Instance, new object[]
            {
                profileId,
                ExitStatus.Survived,
                "UH-60 BlackHawk"
            });
        }

        public void OnTriggerExit(Collider other)
        {
            var player = Singleton<GameWorld>.Instance.GetPlayerByCollider(other);
            if (player == null || !player.IsYourPlayer) return;

            if (_coroutine != null)
                StopCoroutine(_coroutine);
            
            _timer = Plugin.HelicopterExtractTime.Value;
            Singleton<GameUI>.Instance.BattleUiPanelExitTrigger.Close();
        }

        private void OnDestroy()
        {
            if (Singleton<GameUI>.Instantiated)
            {
                Singleton<GameUI>.Instance.BattleUiPanelExitTrigger.Close();
            }
            if (_coroutine == null) return;
            StopCoroutine(_coroutine);
        }
    }
}