using EFT;
using EFT.InventoryLogic;
using HarmonyLib;
using Newtonsoft.Json;
using SPT.Reflection.Patching;
using SPT.Reflection.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace SamSWAT.FireSupport.ArysReloaded.Database;

public class AddItemToDatabasePatch : ModulePatch
{
	protected override MethodBase GetTargetMethod()
	{
		Type fieldType = AccessTools.Field(typeof(ItemFactoryClass), nameof(ItemFactoryClass.ItemTemplates)).FieldType;
		return AccessTools.Method(fieldType, "Init");
	}
	
	[PatchPostfix]
	public static void PatchPostfix(Dictionary<MongoID, ItemTemplate> __instance)
	{
		Type t = PatchConstants.EftTypes.Single(x => x.GetField("SerializerSettings") != null);
		var converters = (JsonConverter[])t.GetField("Converters").GetValue(null);
		string databasePath = Path.Combine(FireSupportPlugin.Directory, "database");
		
		string jsonPath = Path.Combine(databasePath, "ammo_30x173_gau8_avenger.json");
		var gau8Ammo = LoadJson<AmmoTemplate>(jsonPath, converters);
		AddItemTo(gau8Ammo, __instance);
		
		jsonPath = Path.Combine(databasePath, "weapon_ge_gau8_avenger_30x173.json");
		var gau8Weapon = LoadJson<WeaponTemplate>(jsonPath, converters);
		AddItemTo(gau8Weapon, __instance);
	}
	
	private static T LoadJson<T>(string jsonPath, JsonConverter[] converters)
	{
		string json = File.ReadAllText(jsonPath);
		return JsonConvert.DeserializeObject<T>(json, converters);
	}
	
	private static void AddItemTo(ItemTemplate itemTemplate, Dictionary<MongoID, ItemTemplate> dictionary)
	{
		dictionary.Add(itemTemplate._id, itemTemplate);
		
		MongoID? parentId = itemTemplate.ParentId;
		if (parentId.HasValue)
		{
			dictionary[parentId.Value].AddChild(itemTemplate);
		}
	}
}