using Aki.Reflection.Patching;
using Aki.Reflection.Utils;
using EFT.InventoryLogic;
using Newtonsoft.Json;
using SamSWAT.FireSupport.ArysReloaded.Unity;
using SamSWAT.FireSupport.ArysReloaded.Utils;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace SamSWAT.FireSupport.ArysReloaded.Database
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
            var t = PatchConstants.EftTypes.Single(x => x.GetField("SerializerSettings") != null);
            var converters = (JsonConverter[])t.GetField("Converters").GetValue(null);

            var json = File.ReadAllText($"{Plugin.Directory}/database/ammo_30x173_gau8_avenger.json");
            var gau8Ammo = JsonConvert.DeserializeObject<AmmoTemplate>(json, converters);
            __instance.Add(ItemConstants.GAU8_AMMO_TPL, gau8Ammo);

            json = File.ReadAllText($"{Plugin.Directory}/database/weapon_ge_gau8_avenger_30x173.json");
            var gau8Weapon = JsonConvert.DeserializeObject<WeaponTemplate>(json, converters);
            __instance.Add(ItemConstants.GAU8_WEAPON_TPL, gau8Weapon);
        }
    }
}
