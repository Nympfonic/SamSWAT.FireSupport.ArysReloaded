using EFT.UI.Gestures;
using EFT;
using SamSWAT.FireSupport.ArysReloaded.Unity.Interface;
using SamSWAT.FireSupport.ArysReloaded.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using Object = UnityEngine.Object;

namespace SamSWAT.FireSupport.ArysReloaded.Unity.Vehicles.A10
{
    public sealed class A10FireSupport : FireSupport<A10Behaviour>
    {
        public override async Task<List<A10Behaviour>> Load(Transform poolTransform)
        {
            var a10 = (await AssetLoader.LoadAssetAsync("assets/content/vehicles/a10_warthog.bundle")).GetComponent<A10Behaviour>();

            if (Behaviours.Count > 0) Behaviours.Clear();

            for (int i = 0; i < Plugin.AmountOfStrafeRequests.Value; i++)
            {
                Behaviours.Add(LoadVehicle(a10, poolTransform));
            }

            return Behaviours;
        }

        public override A10Behaviour LoadVehicle<A10Behaviour>(A10Behaviour resource, Transform parent)
        {
            var instance = Object.Instantiate(resource, parent);
            instance.gameObject.SetActive(false);
            return instance;
        }

        public override void MakeRequest(GesturesMenu gesturesMenu, FireSupportSpotter spotter)
        {
            gesturesMenu.Close();
            StaticManager.BeginCoroutine(SpotterSequence(spotter, (b, startPos, endPos) =>
            {
                if (!b)
                    StaticManager.BeginCoroutine(SupportRequest(startPos, endPos));
            }));
        }

        public override IEnumerator SpotterSequence(FireSupportSpotter spotter, Action<bool, Vector3, Vector3> confirmation)
        {
            yield return StaticManager.BeginCoroutine(spotter.SpotterVertical(false));
            yield return StaticManager.BeginCoroutine(spotter.SpotterHorizontal());
            yield return StaticManager.BeginCoroutine(spotter.SpotterConfirmation());
            confirmation(spotter.RequestCancelled, spotter.StrafeStartPosition, spotter.StrafeEndPosition);
        }

        public override IEnumerator SupportRequest(Vector3 v1, Vector3 v2)
        {
            var fsController = FireSupportController.Instance;
            var fsAudio = FireSupportAudio.Instance;
            var a10 = TakeFromPool();
            var pos = (v1 + v2) / 2;
            var dir = (v2 - v1).normalized;
            var cost = Plugin.StrafeCost.Value;
            fsController.AvailableStrafeRequests--;
            if (cost > 0) fsController.PaySupportCost(cost);
            fsController.StartRequestCooldown();
            fsAudio.PlayVoiceover(VoiceoverType.StationStrafeRequest);
            yield return new WaitForSecondsRealtime(8f);
            fsAudio.PlayVoiceover(VoiceoverType.JetArriving);
            a10.ProcessRequest(pos, dir, Vector3.zero);
        }
    }
}
