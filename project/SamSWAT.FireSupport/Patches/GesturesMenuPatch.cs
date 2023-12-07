using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Aki.Reflection.Patching;
using Comfort.Common;
using EFT;
using EFT.Airdrop;
using EFT.InputSystem;
using EFT.UI.Gestures;
using HarmonyLib;
using SamSWAT.FireSupport.ArysReloaded.Unity;
using UnityEngine;

namespace SamSWAT.FireSupport.ArysReloaded.Patches
{
    public class GesturesMenuPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return typeof(GesturesMenu).GetMethod(nameof(GesturesMenu.Init));
        }

        [PatchPostfix]
        public static async void PatchPostfix(GesturesMenu __instance)
        {
            if (!IsFireSupportAvailable()) return;
            var owner = Singleton<GameWorld>.Instance.MainPlayer.GetComponent<GamePlayerOwner>();
            var fireSupportController = await FireSupportController.Init(__instance);
            Traverse.Create(owner).Field<List<InputNode>>("_children").Value.Add(fireSupportController);
            var gesturesBindPanel = __instance.gameObject.GetComponentInChildren<GesturesBindPanel>(true);
            gesturesBindPanel.transform.localPosition = new Vector3(0, -530, 0);
        }

        private static bool IsFireSupportAvailable()
        {
            var gameWorld = Singleton<GameWorld>.Instance;
            var locationIsSuitable = LocationScene.GetAll<AirdropPoint>().Any();

            if (!Plugin.Enabled.Value || gameWorld == null || FireSupportController.Instance != null || !locationIsSuitable)
            {
                return false;
            }
            
            var player = gameWorld.RegisteredPlayers[0];
            if (!(player is LocalPlayer)) return false;

            var inventory = player.Profile.Inventory;
            var hasRangefinder = inventory.AllRealPlayerItems.Any(x => x.TemplateId == ItemConstants.RANGEFINDER_TPL);
            
            return hasRangefinder;
        }
    }
}
