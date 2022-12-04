using Comfort.Common;
using CommonAssets.Scripts.Game;
using EFT;
using EFT.UI;
using UnityEngine;

namespace SamSWAT.FireSupport
{
    public class HeliExfiltrationPoint : MonoBehaviour, GInterface19
    {
        private float _timer;
        public string Description => "HeliExfiltrationPoint";

        public void OnTriggerEnter(Collider other)
        {
            var player = Singleton<GameWorld>.Instance.GetPlayerByCollider(other);
            if (player == null || !player.IsYourPlayer) return;
            
            _timer = Plugin.HelicopterExtractTime.Value;
            Singleton<GameUI>.Instance.BattleUiPanelExitTrigger.Show(_timer);
        }

        public void OnTriggerStay(Collider other)
        {
            var player = Singleton<GameWorld>.Instance.GetPlayerByCollider(other);
            if (player == null || !player.IsYourPlayer) return;
            
            if (_timer <= 0f)
            {
                ((EndByExitTrigerScenario.GInterface53) Singleton<AbstractGame>.Instance).StopSession(player.ProfileId,
                    ExitStatus.Survived, "UH-60 BlackHawk");
            }
            else
            {
                _timer -= Time.deltaTime;
            }
        }

        public void OnTriggerExit(Collider other)
        {
            var player = Singleton<GameWorld>.Instance.GetPlayerByCollider(other);
            if (player == null || !player.IsYourPlayer) return;
            
            _timer = Plugin.HelicopterExtractTime.Value;
            Singleton<GameUI>.Instance.BattleUiPanelExitTrigger.Close();
        }
    }
}