using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Comfort.Common;
using SamSWAT.FireSupport.ArysReloaded.Utils;
using UnityEngine;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

namespace SamSWAT.FireSupport.ArysReloaded.Unity
{
    public static class FireSupportPool
    {
        private static List<A10Behaviour> A10Behaviours = new List<A10Behaviour>();
        private static List<UH60Behaviour> UH60Behaviours = new List<UH60Behaviour>();
        
        public static async Task LoadBundlesAndCreatePools()
        {
            var a10 = (await AssetLoader.LoadAssetAsync("assets/content/vehicles/a10_warthog.bundle")).GetComponent<A10Behaviour>();
            var uh60 = (await AssetLoader.LoadAssetAsync("assets/content/vehicles/uh60_blackhawk.bundle")).GetComponent<UH60Behaviour>();
            var poolTransform = new GameObject("FireSupportPool").transform;

            if (A10Behaviours.Count > 0) A10Behaviours.Clear();
            if (UH60Behaviours.Count > 0) UH60Behaviours.Clear();

            for (int i = 0; i < 10; i++)
            {
                A10Behaviours.Add(LoadA10(a10, poolTransform));
                UH60Behaviours.Add(LoadUH60(uh60, poolTransform));
            }
        }

        private static A10Behaviour LoadA10(A10Behaviour resource, Transform parent)
        {
            var instance = Object.Instantiate(resource, parent);
            instance.gameObject.SetActive(false);
            return instance;
        }

        private static UH60Behaviour LoadUH60(UH60Behaviour resource, Transform parent)
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

        public static IFireSupportOption TakeFromPool(ESupportType supportType)
        {
            switch (supportType)
            {
                case ESupportType.Strafe:
                    var a10 = A10Behaviours.Find(x => x.gameObject.activeSelf == false);
                    a10.gameObject.SetActive(true);
                    return a10;
                case ESupportType.Extract:
                    var uh60 = UH60Behaviours.Find(x => x.gameObject.activeSelf == false);
                    uh60.gameObject.SetActive(true);
                    return uh60;
                default:
                    throw new ArgumentOutOfRangeException(nameof(supportType), supportType, null);
            }
        }
    }
}