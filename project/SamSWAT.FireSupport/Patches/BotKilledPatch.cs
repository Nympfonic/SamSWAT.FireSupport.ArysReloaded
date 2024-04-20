using Aki.Reflection.Patching;
using Comfort.Common;
using EFT;
using HarmonyLib;
using SamSWAT.FireSupport.ArysReloaded.Unity;
using SamSWAT.FireSupport.ArysReloaded.Utils;
using System.Reflection;

namespace SamSWAT.FireSupport.ArysReloaded.Patches
{
    // Thanks to Amand's Hitmarker mod for this code
    internal class BotKilledPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(Player), nameof(Player.OnBeenKilledByAggressor));
        }

        [PatchPostfix]
        private static void PatchPostfix(Player __instance, Player aggressor, DamageInfo damageInfo)
        {
            var fsController = FireSupportController.Instance;
            if (fsController == null)
            {
                return;
            }

            if (aggressor == fsController.MainPlayer && __instance != fsController.MainPlayer)
            {
                var killedBy = damageInfo.Weapon;
                string killedByShortName = ModHelper.Localized(killedBy.ShortName, 0);

                if (killedByShortName == ModHelper.AH64_NAME || killedByShortName == ModHelper.A10_NAME)
                {
                    fsController.InvokeEnemyKilledByFireSupport(__instance, killedByShortName);
                }
            }
        }
    }
}
