using Aki.Reflection.Patching;
using Aki.Reflection.Utils;
using SamSWAT.FireSupport.ArysReloaded.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace SamSWAT.FireSupport.ArysReloaded.Patches
{
    internal class AddLocaleToDatabasePatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return PatchConstants.EftTypes.First(IsTargetType).GetMethod("GetLocalization");
        }

        private bool IsTargetType(Type t)
        {
            // We don't want BackendDummyClass.cs
            return typeof(IBackEndSession).IsAssignableFrom(t) && t.IsAbstract;
        }

        [PatchPostfix]
        private static async void PatchPostfix(Task<Dictionary<string, string>> __result)
        {
            var locales = await __result;
            locales.Add($"{ModHelper.GAU8_AMMO_TPL} Name", "PGU-13/B HEI High Explosive Incendiary");
            locales.Add($"{ModHelper.GAU8_AMMO_TPL} ShortName", "PGU-13/B HEI");
            locales.Add($"{ModHelper.GAU8_AMMO_TPL} Description", "The PGU-13/B HEI High Explosive Incendiary round employs a standard M505 fuze and explosive mixture with a body of naturally fragmenting material that is effective against lighter vehicle and material targets.");

            locales.Add($"{ModHelper.M230_AMMO_TPL} Name", "M789 High Explosive Dual Purpose");
            locales.Add($"{ModHelper.M230_AMMO_TPL} ShortName", "M789 HEDP");
            locales.Add($"{ModHelper.M230_AMMO_TPL} Description", "The M789 is the U.S. Apache's main tactical round, a High Explosive Dual Purpose (HEDP) ammunition cartridge. Each round contains 21.5 g (0.76 oz) of explosive charge sealed in a shaped-charge liner. The liner collapses into an armor-piercing jet of metal that can penetrate 1 in (25 mm) of rolled homogeneous armour at 500 m. The shell is also designed to fragment upon impact, killing unprotected, standing people up to about 5 ft (1.5 m) away under optimum conditions.");
            locales.Add($"{ModHelper.HYDRA70_AMMO_TPL} Name", "Hydra 70 M151 High Explosive Dual Purpose");
            locales.Add($"{ModHelper.HYDRA70_AMMO_TPL} ShortName", "Hydra 70 M151 HEDP");
            locales.Add($"{ModHelper.HYDRA70_AMMO_TPL} Description", "The Hydra 70 rocket is a 2.75-inch (70 mm) diameter fin-stabilized unguided rocket used primarily in the air-to-ground role. The most common warhead for the Hydra 70 rocket is the M151 \"10-Pounder,\" which has a blast radius of 10 meters and lethal fragmentation radius of around 50 meters.");
            
            locales.Add($"{ModHelper.GAU8_WEAPON_TPL} Name", "Fairchild Republic A-10 Thunderbolt II");
            locales.Add($"{ModHelper.GAU8_WEAPON_TPL} ShortName", ModHelper.A10_NAME);
            locales.Add($"{ModHelper.GAU8_WEAPON_TPL} Description", "Close air support attack aircraft developed by Fairchild Republic for the USAF with mounted GAU-8/A Avenger 30mm autocannon.");
            
            var apacheName = "Boeing AH-64 Apache";
            var apacheDescription = "Twin-turboshaft attack helicopter developed by Boeing for the US Army's Advanced Attack Helicopter program. Equipped with two Hydra 70 rocket pods, two sets of four AGM-114 Hellfire missiles and an M230 Bushmaster Chain Gun, capable of firing 30x113mm rounds.";
            locales.Add($"{ModHelper.M230_WEAPON_TPL} Name", apacheName);
            locales.Add($"{ModHelper.M230_WEAPON_TPL} ShortName", ModHelper.AH64_NAME);
            locales.Add($"{ModHelper.M230_WEAPON_TPL} Description", apacheDescription);
            locales.Add($"{ModHelper.HYDRA70_WEAPON_TPL} Name", apacheName);
            locales.Add($"{ModHelper.HYDRA70_WEAPON_TPL} ShortName", ModHelper.AH64_NAME);
            locales.Add($"{ModHelper.HYDRA70_WEAPON_TPL} Description", apacheDescription);
        }
    }
}
