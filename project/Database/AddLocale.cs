using Aki.Reflection.Patching;
using Aki.Reflection.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace SamSWAT.FireSupport.Database
{
    public class AddLocale : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return PatchConstants.EftTypes.Single(IsTargetType).GetConstructors()[0];
        }

        private bool IsTargetType(Type type)
        {
            return type.GetMethod("Merge") != null && type.BaseType == typeof(Dictionary<string, string>);
        }

        [PatchPostfix]
        public static void PatchPostfix(Dictionary<string, string> __instance)
        {
            __instance.Add("ammo_30x173_gau8_avenger Name", "PGU-13/B HEI High Explosive Incendiary");
            __instance.Add("ammo_30x173_gau8_avenger ShortName", "PGU-13/B HEI");
            __instance.Add("ammo_30x173_gau8_avenger Description", "The PGU-13/B HEI High Explosive Incendiary round employs a standard M505 fuze and explosive mixture with a body of naturally fragmenting material that is effective against lighter vehicle and material targets.");
            
            __instance.Add("weapon_ge_gau8_avenger_30x173 Name", "Fairchild Republic A-10 Thunderbolt II");
            __instance.Add("weapon_ge_gau8_avenger_30x173 ShortName", "A-10 Thunderbolt II");
            __instance.Add("weapon_ge_gau8_avenger_30x173 Description", "Close air support attack aircraft developed by Fairchild Republic for the USAF with mounted GAU-8/A Avenger 30mm autocannon.");
        }
    }
}
