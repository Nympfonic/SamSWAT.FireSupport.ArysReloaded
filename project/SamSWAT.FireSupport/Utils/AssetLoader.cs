using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using UnityEngine;
using Object = UnityEngine.Object;

namespace SamSWAT.FireSupport.Utils
{
	internal static class AssetLoader
	{
	    private static readonly Dictionary<string, AssetBundle> LoadedBundles = new Dictionary<string, AssetBundle>();

	    private static async Task<AssetBundle> LoadBundleAsync(string bundleName)
		{
			string bundlePath = bundleName;
			bundleName = Regex.Match(bundleName, @"[^//]*$").Value;

			if (LoadedBundles.TryGetValue(bundleName, out var bundle))
				return bundle;

			var bundleRequest = AssetBundle.LoadFromFileAsync(Plugin.Directory + bundlePath);

			while (!bundleRequest.isDone)
				await Task.Yield();

			var requestedBundle = bundleRequest.assetBundle;

			if (requestedBundle != null)
			{
				LoadedBundles.Add(bundleName, requestedBundle);
				return requestedBundle;
			}

			Plugin.Logger.LogError($"Can't load bundle: {bundlePath} (does it exist?), unknown error.");
			return null;
		}

	    public static Task<GameObject> LoadAssetAsync(string bundle, string assetName = null)
	    {
		    return LoadAssetAsync<GameObject>(bundle, assetName);
	    }
	    
		public static async Task<T> LoadAssetAsync<T>(string bundle, string assetName = null) where T : Object
		{
			var ab = await LoadBundleAsync(bundle);

			var assetBundleRequest = string.IsNullOrEmpty(assetName)
				? ab.LoadAllAssetsAsync<T>()
				: ab.LoadAssetAsync<T>(assetName);

			while (!assetBundleRequest.isDone)
				await Task.Yield();

			if (assetBundleRequest.allAssets.Length == 0)
			{
				Plugin.Logger.LogError($"Can't load Object from bundle: {bundle}, asset list is empty.");
				return null;
			}
			
			var requestedObj = assetBundleRequest.allAssets[0] as T;

			return requestedObj;
		}

		public static void UnloadBundle(string bundleName, bool unloadAllLoadedObjects = true)
        {
			if (LoadedBundles.TryGetValue(bundleName, out var ab))
            {
				ab.Unload(unloadAllLoadedObjects);
				LoadedBundles.Remove(bundleName);
            }
			else
			{
				Plugin.Logger.LogError($"AssetBundle '{bundleName}' already unloaded");
			}
		}

		public static void UnloadAllBundles()
		{
			foreach (var bundle in LoadedBundles.Values)
			{
				bundle.Unload(true);
			}
		}
	}
}
