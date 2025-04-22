using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using Object = UnityEngine.Object;

namespace SamSWAT.FireSupport.ArysReloaded.Utils;

internal static class AssetLoader
{
	private static readonly Dictionary<string, AssetBundle> s_loadedBundles = new();
	
	private static async UniTask<AssetBundle> LoadBundleAsync(string bundleName)
	{
		string bundlePath = bundleName;
		bundleName = Regex.Match(bundleName, @"[^//]*$").Value;
		
		if (s_loadedBundles.TryGetValue(bundleName, out AssetBundle bundle))
		{
			return bundle;
		}
		
		AssetBundleCreateRequest bundleRequest = AssetBundle.LoadFromFileAsync(FireSupportPlugin.Directory + bundlePath);
		
		while (!bundleRequest.isDone)
		{
			await UniTask.Yield();
		}
		
		AssetBundle requestedBundle = bundleRequest.assetBundle;
		
		if (requestedBundle != null)
		{
			s_loadedBundles.Add(bundleName, requestedBundle);
			return requestedBundle;
		}
		
		FireSupportPlugin.LogSource.LogError($"Can't load bundle: {bundlePath} (does it exist?), unknown error.");
		return null;
	}
	
	public static UniTask<GameObject> LoadAssetAsync(string bundle, string assetName = null)
	{
		return LoadAssetAsync<GameObject>(bundle, assetName);
	}
	
	public static async UniTask<T> LoadAssetAsync<T>(string bundle, string assetName = null) where T : Object
	{
		AssetBundle ab = await LoadBundleAsync(bundle);
		
		AssetBundleRequest assetBundleRequest = string.IsNullOrEmpty(assetName)
			? ab.LoadAllAssetsAsync<T>()
			: ab.LoadAssetAsync<T>(assetName);
		
		while (!assetBundleRequest.isDone)
		{
			await UniTask.Yield();
		}
		
		if (assetBundleRequest.allAssets.Length == 0)
		{
			FireSupportPlugin.LogSource.LogError($"Can't load Object from bundle: {bundle}, asset list is empty.");
			return null;
		}
		
		var requestedObj = assetBundleRequest.allAssets[0] as T;
		
		return requestedObj;
	}
	
	public static void UnloadBundle(string bundleName, bool unloadAllLoadedObjects = true)
	{
		if (s_loadedBundles.TryGetValue(bundleName, out AssetBundle ab))
		{
			ab.Unload(unloadAllLoadedObjects);
			s_loadedBundles.Remove(bundleName);
		}
		else
		{
			FireSupportPlugin.LogSource.LogError($"AssetBundle '{bundleName}' already unloaded");
		}
	}
	
	public static void UnloadAllBundles(bool unloadAllLoadedObjects = true)
	{
		foreach (AssetBundle bundle in s_loadedBundles.Values)
		{
			bundle.Unload(unloadAllLoadedObjects);
		}
		
		s_loadedBundles.Clear();
	}
}