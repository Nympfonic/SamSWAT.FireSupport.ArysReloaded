using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Aki.Reflection.Utils;
using UnityEngine;
using Object = UnityEngine.Object;

namespace SamSWAT.FireSupport.Utils
{
	internal static class UtilsClass
	{
		internal static Type RangefinderControllerType;
	    private static Dictionary<string, AssetBundle> LoadedBundles = new Dictionary<string, AssetBundle>();
	    private static AssetBundleRequest _assetBundleRequest;

	    static UtilsClass()
	    {
		    RangefinderControllerType = PatchConstants.EftTypes.Single(x => x.Name == "PortableRangeFinderController");
	    }
	    
        private static async Task<AssetBundle> LoadBundleAsync(string bundleName)
		{
			string bundlePath = bundleName;
			bundleName = Regex.Match(bundleName, @"[^//]*$").Value;

			if (LoadedBundles.TryGetValue(bundleName, out var bundle))
				return bundle;

			var bundleRequest = AssetBundle.LoadFromFileAsync(Plugin.Directory + bundlePath);

			while (!bundleRequest.isDone)
				await Task.Yield();

			AssetBundle requestedBundle = bundleRequest.assetBundle;

			if (requestedBundle != null)
			{
				LoadedBundles.Add(bundleName, requestedBundle);
				return requestedBundle;
			}

			Debug.LogError($"Can't load bundle: {bundleName} (does it exist?), unknown error.");
			return null;
		}

		public static async Task<T> LoadAssetAsync<T>(string bundle, string assetName = null) where T : Object
		{
			AssetBundle ab = await LoadBundleAsync(bundle);

			_assetBundleRequest = string.IsNullOrEmpty(assetName)
				? ab.LoadAllAssetsAsync<T>()
				: ab.LoadAssetAsync<T>(assetName);

			while (!_assetBundleRequest.isDone)
				await Task.Yield();

			if (_assetBundleRequest.allAssets.Length == 0)
			{
				Debug.LogError($"Can't load Object from bundle: {bundle}, asset list is empty.");
				return null;
			}
			
			var requestedObj = _assetBundleRequest.allAssets[0] as T;

			return requestedObj;
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
	}
}
