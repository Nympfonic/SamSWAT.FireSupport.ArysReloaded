using Aki.Reflection.Patching;
using EFT.UI;
using EFT.UI.Gestures;
using HarmonyLib;
using SamSWAT.FireSupport.ArysReloaded.Unity;
using SamSWAT.FireSupport.ArysReloaded.Utils;
using System;
using System.Reflection;

namespace SamSWAT.FireSupport.ArysReloaded.Patches
{
    public class GesturesMenuShowPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(GesturesMenu), nameof(GesturesMenu.Show));
        }

        [PatchPrefix]
        public static void PatchPrefix()
        {
            try
            {
                FireSupportController.Instance?.UpdateInventoryMoney(ModHelper.MainPlayer, Plugin.SupportCostCurrency.Value);
#if DEBUG
                ConsoleScreen.Log($"[{ModHelper.MOD_NAME}] Updated player inventory money sum");
#endif
            }
            catch (Exception ex)
            {
                ConsoleScreen.LogError($"[{ModHelper.MOD_NAME}] Error when updating player inventory money sum. Check BepInEx/LogOutput.log");
                Plugin.LogSource.LogError(ex);
            }

        }
    }
}
