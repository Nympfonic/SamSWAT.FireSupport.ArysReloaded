using Comfort.Common;
using EFT;
using EFT.Airdrop;
using EFT.InputSystem;
using EFT.InventoryLogic;
using EFT.UI.Gestures;
using HarmonyLib;
using SamSWAT.FireSupport.ArysReloaded.Unity;
using SPT.Reflection.Patching;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace SamSWAT.FireSupport.ArysReloaded.Patches;

public class GesturesMenuPatch : ModulePatch
{
	protected override MethodBase GetTargetMethod()
	{
		return typeof(GesturesMenu).GetMethod(nameof(GesturesMenu.Init));
	}
	
	[PatchPostfix]
	public static async void PatchPostfix(GesturesMenu __instance)
	{
		if (!IsFireSupportAvailable())
		{
			return;
		}
		
		try
		{
			var owner = Singleton<GameWorld>.Instance.MainPlayer.GetComponent<GamePlayerOwner>();
			var fireSupportController = await FireSupportController.Create(__instance);
			Traverse.Create(owner)
				.Field<List<InputNode>>("_children")
				.Value
				.Add(fireSupportController);
			
			var gesturesBindPanel = __instance.gameObject.GetComponentInChildren<GesturesBindPanel>(includeInactive: true);
			gesturesBindPanel.transform.localPosition = new Vector3(0, -530, 0);
		}
		catch (Exception ex)
		{
			FireSupportPlugin.LogSource.LogError(ex);
		}
	}
	
	private static bool IsFireSupportAvailable()
	{
		GameWorld gameWorld = Singleton<GameWorld>.Instance;
		if (gameWorld == null)
		{
			return false;
		}
		
		bool locationIsSuitable = gameWorld.MainPlayer.Location.ToLower() == "sandbox"
			|| LocationScene.GetAll<AirdropPoint>().Any();
		
		if (!FireSupportPlugin.Enabled.Value || FireSupportController.Instance != null || !locationIsSuitable)
		{
			return false;
		}
		
		Player player = gameWorld.MainPlayer;
		if (player == null)
		{
			return false;
		}
		
		Inventory inventory = player.Profile.Inventory;
		bool hasRangefinder = inventory.AllRealPlayerItems.Any(x => x.TemplateId == ItemConstants.RANGEFINDER_TPL);
		
		return hasRangefinder;
	}
}