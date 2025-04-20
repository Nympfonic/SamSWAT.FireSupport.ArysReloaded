using HarmonyLib;
using SamSWAT.FireSupport.ArysReloaded.Unity;
using SPT.Reflection.Patching;
using System.Collections.Generic;
using System.Reflection;

namespace SamSWAT.FireSupport.ArysReloaded.Database;

public class AddLocaleToDatabasePatch : ModulePatch
{
	protected override MethodBase GetTargetMethod()
	{
		return AccessTools.Method(typeof(LocaleManagerClass), nameof(LocaleManagerClass.UpdateLocales));
	}
	
	[PatchPostfix]
	private static void PatchPostfix(Dictionary<string, string> newLocale)
	{
		newLocale.TryAdd($"{ItemConstants.GAU8_AMMO_TPL} Name", "PGU-13/B HEI High Explosive Incendiary");
		newLocale.TryAdd($"{ItemConstants.GAU8_AMMO_TPL} ShortName", "PGU-13/B HEI");
		newLocale.TryAdd($"{ItemConstants.GAU8_AMMO_TPL} Description", "The PGU-13/B HEI High Explosive Incendiary round employs a standard M505 fuze and explosive mixture with a body of naturally fragmenting material that is effective against lighter vehicle and material targets.");
		
		newLocale.TryAdd($"{ItemConstants.GAU8_WEAPON_TPL} Name", "Fairchild Republic A-10 Thunderbolt II");
		newLocale.TryAdd($"{ItemConstants.GAU8_WEAPON_TPL} ShortName", "A-10 Thunderbolt II");
		newLocale.TryAdd($"{ItemConstants.GAU8_WEAPON_TPL} Description", "Close air support attack aircraft developed by Fairchild Republic for the USAF with mounted GAU-8/A Avenger 30mm autocannon.");
	}
}