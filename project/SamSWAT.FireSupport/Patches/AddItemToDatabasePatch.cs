using Aki.Reflection.Patching;
using Aki.Reflection.Utils;
using EFT.InventoryLogic;
using HarmonyLib;
using Newtonsoft.Json;
using SamSWAT.FireSupport.ArysReloaded.Utils;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace SamSWAT.FireSupport.ArysReloaded.Patches
{
    internal class AddItemToDatabasePatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.DeclaredField(typeof(ItemFactory), nameof(ItemFactory.ItemTemplates)).FieldType.GetMethod("Init");
        }

        [PatchPostfix]
        private static void PatchPostfix(Dictionary<string, ItemTemplate> __instance)
        {
            var t = PatchConstants.EftTypes.Single(x => x.GetField("SerializerSettings") != null);
            var converters = (JsonConverter[])t.GetField("Converters").GetValue(null);

            AddItem<AmmoTemplate>(__instance, converters, ModHelper.GAU8_AMMO_TPL);
            AddItem<WeaponTemplate>(__instance, converters, ModHelper.GAU8_WEAPON_TPL);

            AddItem<AmmoTemplate>(__instance, converters, ModHelper.M230_AMMO_TPL);
            AddItem<WeaponTemplate>(__instance, converters, ModHelper.M230_WEAPON_TPL);

            AddItem<AmmoTemplate>(__instance, converters, ModHelper.HYDRA70_AMMO_TPL);
            AddItem<WeaponTemplate>(__instance, converters, ModHelper.HYDRA70_WEAPON_TPL);
        }

        private static void AddItem<T>(IDictionary<string, ItemTemplate> itemTemplates, JsonConverter[] converters, string tpl)
            where T : ItemTemplate
        {
            var json = File.ReadAllText($"{Plugin.Directory}/database/{ModHelper.ItemMappings[tpl]}.json");
            ItemTemplate item = JsonConvert.DeserializeObject<T>(json, converters);
            itemTemplates.Add(tpl, item);
        }
    }
}
