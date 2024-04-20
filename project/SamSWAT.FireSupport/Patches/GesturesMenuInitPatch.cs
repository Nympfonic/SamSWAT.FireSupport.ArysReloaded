using Aki.Reflection.Patching;
using Comfort.Common;
using EFT;
using EFT.Airdrop;
using EFT.InputSystem;
using EFT.UI.Gestures;
using HarmonyLib;
using SamSWAT.FireSupport.ArysReloaded.Unity;
using SamSWAT.FireSupport.ArysReloaded.Utils;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace SamSWAT.FireSupport.ArysReloaded.Patches
{
    internal class GesturesMenuInitPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(GesturesMenu), nameof(GesturesMenu.Init));
        }

        [PatchPostfix]
        private static async void PatchPostfix(GesturesMenu __instance)
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
            var mainPlayer = ModHelper.MainPlayer;
            if (!(mainPlayer is LocalPlayer))
            {
                return false;
            }

            var locationIsSuitable = mainPlayer.Location.ToLower() == "sandbox" // manual check since Ground Zero doesn't have AirdropPoints in 0.14.1.2
                || LocationScene.GetAll<AirdropPoint>().Any();

            if (!Plugin.Enabled.Value || FireSupportController.Instance != null || !locationIsSuitable)
            {
                return false;
            }

            var inventory = mainPlayer.Profile.Inventory;
            var hasRangefinder = inventory.AllRealPlayerItems.Any(x => x.TemplateId == ModHelper.RANGEFINDER_TPL);

            return hasRangefinder;
        }
    }
}
