using Aki.Reflection.Patching;
using Aki.Reflection.Utils;
using SamSWAT.FireSupport.ArysReloaded.Unity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace SamSWAT.FireSupport.ArysReloaded.Database
{
    public class AddLocaleToDatabasePatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return PatchConstants.EftTypes.First(IsTargetType).GetMethod("GetLocalization");
        }

        private bool IsTargetType(Type t)
        {
            return typeof(ISession).IsAssignableFrom(t);
        }

        [PatchPostfix]
        private static async void PatchPostfix(Task<Dictionary<string, string>> __result)
        {
            var locales = await __result;
            locales.Add($"{ItemConstants.GAU8_AMMO_NAME} Name", "PGU-13/B HEI High Explosive Incendiary");
            locales.Add($"{ItemConstants.GAU8_AMMO_NAME} ShortName", "PGU-13/B HEI");
            locales.Add($"{ItemConstants.GAU8_AMMO_NAME} Description", "The PGU-13/B HEI High Explosive Incendiary round employs a standard M505 fuze and explosive mixture with a body of naturally fragmenting material that is effective against lighter vehicle and material targets.");

            locales.Add($"{ItemConstants.GAU8_WEAPON_NAME} Name", "Fairchild Republic A-10 Thunderbolt II");
            locales.Add($"{ItemConstants.GAU8_WEAPON_NAME} ShortName", "A-10 Thunderbolt II");
            locales.Add($"{ItemConstants.GAU8_WEAPON_NAME} Description", "Close air support attack aircraft developed by Fairchild Republic for the USAF with mounted GAU-8/A Avenger 30mm autocannon.");
        }
    }
}
