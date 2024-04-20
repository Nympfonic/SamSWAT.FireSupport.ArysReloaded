using System.Collections;
using System.Linq;
using System.Reflection;
using Comfort.Common;
using CommonAssets.Scripts.Game;
using EFT;
using EFT.UI;
using SamSWAT.FireSupport.ArysReloaded.Utils;
using UnityEngine;

namespace SamSWAT.FireSupport.ArysReloaded.Unity
{
    public class HeliExfiltrationPoint : MonoBehaviour, IPhysicsTrigger
    {
        private float _timer;
        public string Description => "HeliExfiltrationPoint";
        private Coroutine _coroutine;
        private readonly MethodInfo _stopSession;

        public HeliExfiltrationPoint()
        {
            var t = typeof(EndByExitTrigerScenario).GetNestedTypes().Single(x => x.IsInterface);
            _stopSession = t.GetMethod("StopSession");
        }

        public void OnTriggerEnter(Collider collider)
        {
            var player = Singleton<GameWorld>.Instance.GetPlayerByCollider(collider);
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

            _stopSession.Invoke(Singleton<AbstractGame>.Instance, new object[]
            {
                profileId,
                ExitStatus.Survived,
                ModHelper.UH60_NAME
            });
        }

        public void OnTriggerExit(Collider collider)
        {
            var player = ModHelper.GameWorld.GetPlayerByCollider(collider);
            if (player == null || !player.IsYourPlayer) return;
            
            _timer = Plugin.HelicopterExtractTime.Value;
            Singleton<GameUI>.Instance.BattleUiPanelExitTrigger.Close();

            if (_coroutine == null) return;
            StopCoroutine(_coroutine);
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