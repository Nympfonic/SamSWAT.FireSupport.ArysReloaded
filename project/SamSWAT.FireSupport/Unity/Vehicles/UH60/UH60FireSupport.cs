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

namespace SamSWAT.FireSupport.ArysReloaded.Unity.Vehicles.UH60
{
    public sealed class UH60FireSupport : FireSupport<UH60Behaviour>
    {
        public override async Task<List<UH60Behaviour>> Load(Transform poolTransform)
        {
            var uh60 = (await AssetLoader.LoadAssetAsync("assets/content/vehicles/uh60_blackhawk.bundle")).GetComponent<UH60Behaviour>();

            if (Behaviours.Count > 0) Behaviours.Clear();

            for (int i = 0; i < Plugin.AmountOfExtractionRequests.Value; i++)
            {
                Behaviours.Add(LoadVehicle(uh60, poolTransform));
            }

            return Behaviours;
        }

        public override UH60Behaviour LoadVehicle<UH60Behaviour>(UH60Behaviour resource, Transform parent)
        {
            var instance = Object.Instantiate(resource, parent);
            instance.gameObject.SetActive(false);
            var outputAudioMixerGroup = Singleton<BetterAudio>.Instance.OutEnvironment;
            instance.engineCloseSource.outputAudioMixerGroup = outputAudioMixerGroup;
            instance.engineDistantSource.outputAudioMixerGroup = outputAudioMixerGroup;
            instance.rotorsCloseSource.outputAudioMixerGroup = outputAudioMixerGroup;
            instance.rotorsDistantSource.outputAudioMixerGroup = outputAudioMixerGroup;
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
            yield return StaticManager.BeginCoroutine(spotter.SpotterVertical(true));
            yield return StaticManager.BeginCoroutine(spotter.SpotterConfirmation());
            confirmation(spotter.RequestCancelled, spotter.SpotterPosition, spotter.ColliderRotation);
        }

        public override IEnumerator SupportRequest(Vector3 v1, Vector3 v2)
        {
            var fsController = FireSupportController.Instance;
            var fsAudio = FireSupportAudio.Instance;
            var cost = Plugin.HelicopterCost.Value;
            fsController.AvailableExtractRequests--;
            if (cost > 0) fsController.PaySupportCost(cost);
            fsController.SetRequestAvailable(false);
            fsAudio.PlayVoiceover(VoiceoverType.StationExtractionRequest);
            yield return new WaitForSecondsRealtime(8f);
            var uh60 = TakeFromPool();
            uh60.ProcessRequest(v1, Vector3.zero, v2);
            fsAudio.PlayVoiceover(VoiceoverType.SupportHeliArrivingToPickup);
            //yield return new WaitForSecondsRealtime(35f + Plugin.HelicopterWaitTime.Value);
            yield return new WaitUntil(uh60.IsLeaving);
            fsController.StartRequestCooldown();
        }
    }
}
