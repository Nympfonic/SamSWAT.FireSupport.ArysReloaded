using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using UnityEngine;

namespace SamSWAT.FireSupport
{
    internal class Utils
    {
        public static Dictionary<string, AssetBundle> LoadedBundles = new Dictionary<string, AssetBundle>();
		public static FireSupportUI FireSupportUI;
		public static async Task<AssetBundle> LoadBundleAsync(string bundleName)
		{
			if (LoadedBundles.TryGetValue(bundleName, out var bundle))
				return bundle;

			var bundleRequest = AssetBundle.LoadFromFileAsync(Plugin.Directory + bundleName);

			while (!bundleRequest.isDone)
				await Task.Yield();

			AssetBundle requestedBundle = bundleRequest.assetBundle;

			if (requestedBundle != null)
			{
				string name = Regex.Match(bundleName, @"[^//]*$").Value;
				LoadedBundles.Add(name, requestedBundle);
				return requestedBundle;
			}
			else
			{
				Debug.LogError($"Can't load bundle: {bundleName} (does it exist?), unknown error.");
				return null;
			}
		}

		public static async Task<GameObject> LoadAssetAsync(string bundle, string assetName = null)
		{
			AssetBundle ab = await LoadBundleAsync(bundle);
			AssetBundleRequest assetBundleRequest;

			if (assetName == null)
            {
				assetBundleRequest = ab.LoadAllAssetsAsync<GameObject>();
			}
			else
            {
				assetBundleRequest = ab.LoadAssetAsync<GameObject>(assetName);
			}

			while (!assetBundleRequest.isDone)
				await Task.Yield();

			GameObject requestedGO = assetBundleRequest.allAssets[0] as GameObject;

			if (requestedGO != null)
			{
				return requestedGO;
			}
			else
			{
				Debug.LogError($"Can't load GameObject from bundle: {bundle}, unknown error.");
				return null;
			}
		}
		public static void UnloadBundle(string bundleName, bool unloadAllLoadedObjects = false)
        {
			if (LoadedBundles.TryGetValue(bundleName, out var ab))
            {
				ab.Unload(unloadAllLoadedObjects);
				LoadedBundles.Remove(bundleName);
            }
			else
			{
				Debug.LogError($"AssetBundle '{bundleName}' already unloaded");
			}
		}
		public static AudioClip GetRandomAudio(AudioClip[] audioClips)
		{
			return audioClips[UnityEngine.Random.Range(0, audioClips.Length)];
		}
	}
}
