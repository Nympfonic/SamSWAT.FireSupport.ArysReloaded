using Aki.Reflection.Patching;
using Aki.Reflection.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace SamSWAT.FireSupport.Database
{
    public class AddLocaleToDatabasePatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return PatchConstants.EftTypes.First(IsTargetType).GetMethod("GetLocalization");
        }

        private bool IsTargetType(Type t)
        {
            return typeof(ISession).IsAssignableFrom(t) && t.IsNotPublic;
        }

        [PatchPostfix]
        private static async void PatchPostfix(Task<Dictionary<string, string>> __result)
        {
            var locales = await __result;
            locales.Add("ammo_30x173_gau8_avenger Name", "PGU-13/B HEI High Explosive Incendiary");
            locales.Add("ammo_30x173_gau8_avenger ShortName", "PGU-13/B HEI");
            locales.Add("ammo_30x173_gau8_avenger Description", "The PGU-13/B HEI High Explosive Incendiary round employs a standard M505 fuze and explosive mixture with a body of naturally fragmenting material that is effective against lighter vehicle and material targets.");
            
            locales.Add("weapon_ge_gau8_avenger_30x173 Name", "Fairchild Republic A-10 Thunderbolt II");
            locales.Add("weapon_ge_gau8_avenger_30x173 ShortName", "A-10 Thunderbolt II");
            locales.Add("weapon_ge_gau8_avenger_30x173 Description", "Close air support attack aircraft developed by Fairchild Republic for the USAF with mounted GAU-8/A Avenger 30mm autocannon.");
        }
    }
}
