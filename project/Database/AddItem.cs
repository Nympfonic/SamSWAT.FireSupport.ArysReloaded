using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Aki.Reflection.Patching;
using Aki.Reflection.Utils;
using EFT.InventoryLogic;

namespace SamSWAT.FireSupport.Database
{
	public class AddItem : ModulePatch
	{
		protected override MethodBase GetTargetMethod()
		{
			return PatchConstants.EftTypes.Single(x => x.GetMethod("GetItemTemplates") != null).GetMethod("Init");
		}

		[PatchPostfix]
		public static void PatchPostfix(Dictionary<string, ItemTemplate> __instance)
		{
			__instance.TryGetValue("5d70e500a4b9364de70d38ce", out var ags30round);
			var gau8Ammo = (AmmoTemplate)ags30round;
			gau8Ammo._id = "ammo_30x173_gau8_avenger";
			gau8Ammo.ExplosionType = "big_smoky_explosion";
			gau8Ammo.InitialSpeed = 1070f;
			gau8Ammo.MinExplosionDistance = 30f;
			gau8Ammo.MaxExplosionDistance = 50f;
			gau8Ammo.ExplosionStrength = 150f;
			gau8Ammo.Damage = 300;
			gau8Ammo.ArmorDamage = 150;
			gau8Ammo.PenetrationPower = 100;
			gau8Ammo.PenetrationPowerDiviation = 0.5f;
			gau8Ammo.PenetrationChance = 1f;
			__instance.Add("ammo_30x173_gau8_avenger", gau8Ammo);

			__instance.TryGetValue("5d52cc5ba4b9367408500062", out var ags30);
			var gau8Weapon = (WeaponTemplate)ags30;
			gau8Weapon._id = "weapon_ge_gau8_avenger_30x173";
			gau8Weapon.Name = "weapon_ge_gau8_avenger_30x173";
			gau8Weapon.Chambers[0].Filters[0].Filter[0] = "ammo_30x173_gau8_avenger";
			__instance.Add("weapon_ge_gau8_avenger_30x173", gau8Weapon);
		}
	}
}
