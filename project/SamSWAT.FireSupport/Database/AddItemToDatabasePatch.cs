using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Aki.Reflection.Patching;
using Aki.Reflection.Utils;
using EFT.InventoryLogic;
using Newtonsoft.Json;
using SamSWAT.FireSupport.Utils;

namespace SamSWAT.FireSupport.Database
{
	public class AddItemToDatabasePatch : ModulePatch
	{
		protected override MethodBase GetTargetMethod()
		{
			return ItemFactoryUtil.Type.GetField("ItemTemplates").FieldType.GetMethod("Init");
		}

		[PatchPostfix]
		public static void PatchPostfix(Dictionary<string, ItemTemplate> __instance)
		{
			//Utils.ItemFactory.Init();
			var t = PatchConstants.EftTypes.Single(x => x.GetField("SerializerSettings") != null);
			var converters = (JsonConverter[]) t.GetField("Converters").GetValue(null);

			var json = File.ReadAllText($"{Plugin.Directory}/database/ammo_30x173_gau8_avenger.json");
			var gau8Ammo = JsonConvert.DeserializeObject<AmmoTemplate>(json, converters);
			__instance.Add("ammo_30x173_gau8_avenger", gau8Ammo);

			json = File.ReadAllText($"{Plugin.Directory}/database/weapon_ge_gau8_avenger_30x173.json");
			var gau8Weapon = JsonConvert.DeserializeObject<WeaponTemplate>(json, converters);
			__instance.Add("weapon_ge_gau8_avenger_30x173", gau8Weapon);
		}
	}
}
