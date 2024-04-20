using Comfort.Common;
using EFT;
using EFT.UI.Gestures;
using SamSWAT.FireSupport.ArysReloaded.Unity.Interface;
using SamSWAT.FireSupport.ArysReloaded.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using Object = UnityEngine.Object;

namespace SamSWAT.FireSupport.ArysReloaded.Unity.Vehicles.AH64
{
    public sealed class AH64FireSupport : FireSupport<AH64Behaviour>
    {
        public override async Task<List<AH64Behaviour>> Load(Transform poolTransform)
        {
            var ah64 = (await AssetLoader.LoadAssetAsync("assets/content/vehicles/ah64_apache.bundle")).GetComponent<AH64Behaviour>();

            if (Behaviours.Count > 0) Behaviours.Clear();

            for (int i = 0; i < Plugin.AmountOfApacheRequests.Value; i++)
            {
                Behaviours.Add(LoadVehicle(ah64, poolTransform));
            }

            return Behaviours;
        }

        public override AH64Behaviour LoadVehicle<AH64Behaviour>(AH64Behaviour resource, Transform parent)
        {
            var instance = Object.Instantiate(resource, parent);
            var audioController = instance.audioController;
            instance.gameObject.SetActive(false);
            var outputAudioMixerGroup = Singleton<BetterAudio>.Instance.OutEnvironment;
            audioController.engineCloseSource.outputAudioMixerGroup = outputAudioMixerGroup;
            audioController.engineDistantSource.outputAudioMixerGroup = outputAudioMixerGroup;
            audioController.rotorsCloseSource.outputAudioMixerGroup = outputAudioMixerGroup;
            audioController.rotorsDistantSource.outputAudioMixerGroup = outputAudioMixerGroup;
            audioController.turbineSource.outputAudioMixerGroup = outputAudioMixerGroup;
            return instance;
        }

        public override void MakeRequest(GesturesMenu gesturesMenu, FireSupportSpotter spotter)
        {
            gesturesMenu.Close();
            StaticManager.BeginCoroutine(SpotterSequence(spotter, (b, pos, rot) =>
            {
                if (!b)
                    StaticManager.BeginCoroutine(SupportRequest(pos, rot));
            }));
        }

        public override IEnumerator SpotterSequence(FireSupportSpotter spotter, Action<bool, Vector3, Vector3> confirmation)
        {
            yield return StaticManager.BeginCoroutine(spotter.SpotterVertical(false));
            yield return StaticManager.BeginCoroutine(spotter.SpotterConfirmation());
            confirmation(spotter.RequestCancelled, spotter.SpotterPosition, spotter.ColliderRotation);
        }

        public override IEnumerator SupportRequest(Vector3 v1, Vector3 v2)
        {
            var fsController = FireSupportController.Instance;
            var fsAudio = FireSupportAudio.Instance;
            var cost = Plugin.ApacheCost.Value;
            fsController.AvailableApacheRequests--;
            if (cost > 0) fsController.PaySupportCost(cost);
            fsController.SetRequestAvailable(false);
            fsAudio.PlayVoiceover(VoiceoverType.StationApacheRequest);
            yield return new WaitForSecondsRealtime(8f);
            var ah64 = TakeFromPool();
            ah64.ProcessRequest(v1, Vector3.zero, v2);
            fsAudio.PlayVoiceover(VoiceoverType.ApacheArriving);
            yield return new WaitUntil(ah64.IsLeavingActiveField);
            fsController.StartRequestCooldown();
        }
    }
}
