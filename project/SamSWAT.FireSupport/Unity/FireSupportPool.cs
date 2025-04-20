using Comfort.Common;
using SamSWAT.FireSupport.ArysReloaded.Utils;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Audio;
using Object = UnityEngine.Object;

namespace SamSWAT.FireSupport.ArysReloaded.Unity;

public static class FireSupportPool
{
	private static readonly List<A10Behaviour> s_a10Behaviours = [];
	private static readonly List<UH60Behaviour> s_uh60Behaviours = [];
	
	public static async Task LoadBundlesAndCreatePools()
	{
		var a10 = (await AssetLoader.LoadAssetAsync("assets/content/vehicles/a10_warthog.bundle")).GetComponent<A10Behaviour>();
		var uh60 = (await AssetLoader.LoadAssetAsync("assets/content/vehicles/uh60_blackhawk.bundle")).GetComponent<UH60Behaviour>();
		Transform poolTransform = new GameObject("FireSupportPool").transform;

		if (s_a10Behaviours.Count > 0) s_a10Behaviours.Clear();
		if (s_uh60Behaviours.Count > 0) s_uh60Behaviours.Clear();

		for (int i = 0; i < 10; i++)
		{
			s_a10Behaviours.Add(LoadA10(a10, poolTransform));
			s_uh60Behaviours.Add(LoadUH60(uh60, poolTransform));
		}
	}
	
	private static A10Behaviour LoadA10(A10Behaviour resource, Transform parent)
	{
		A10Behaviour instance = Object.Instantiate(resource, parent);
		instance.gameObject.SetActive(false);
		return instance;
	}
	
	private static UH60Behaviour LoadUH60(UH60Behaviour resource, Transform parent)
	{
		UH60Behaviour instance = Object.Instantiate(resource, parent);
		instance.gameObject.SetActive(false);
		AudioMixerGroup outputAudioMixerGroup = Singleton<BetterAudio>.Instance.OutEnvironment;
		instance.engineCloseSource.outputAudioMixerGroup = outputAudioMixerGroup;
		instance.engineDistantSource.outputAudioMixerGroup = outputAudioMixerGroup;
		instance.rotorsCloseSource.outputAudioMixerGroup = outputAudioMixerGroup;
		instance.rotorsDistantSource.outputAudioMixerGroup = outputAudioMixerGroup;
		return instance;
	}

	public static IFireSupportOption TakeFromPool(ESupportType supportType)
	{
		MonoBehaviour behaviour;
		
		switch (supportType)
		{
			case ESupportType.Strafe:
				behaviour = s_a10Behaviours.Find(x => x.gameObject.activeSelf == false);
				break;
			case ESupportType.Extract:
				behaviour = s_uh60Behaviours.Find(x => x.gameObject.activeSelf == false);
				break;
			default:
				throw new ArgumentOutOfRangeException(nameof(supportType), supportType, null);
		}
		
		behaviour.gameObject.SetActive(true);
		return (IFireSupportOption)behaviour;
	}
}