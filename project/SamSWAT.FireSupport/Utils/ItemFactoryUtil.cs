using Aki.Reflection.Patching;
using Aki.Reflection.Utils;
using EFT.InventoryLogic;
using System;
using System.Linq;
using System.Reflection;

namespace SamSWAT.FireSupport.ArysReloaded.Utils
{
    internal class ItemFactoryUtil : ModulePatch
    {
        internal static readonly Type Type;
        private static object _instance;
        private static CreateItemDelegate _createItemDelegate;
        private delegate Item CreateItemDelegate(object instance, string id, string tplId, object diff = null);

        static ItemFactoryUtil()
        {
            Type = PatchConstants.EftTypes.Single(x => x.GetMethod("FlatItemsToTree") != null);
        }

        protected override MethodBase GetTargetMethod()
        {
            return Type.GetConstructors()[0];
        }

        [PatchPostfix]
        private static void PatchPostfix(object __instance)
        {
            _instance = __instance;
            var method = __instance.GetType().GetMethod("CreateItem");
            _createItemDelegate = AccessToolsUtil.MethodDelegate<CreateItemDelegate>(method, false);
        }

        public static Item CreateItem(string id, string tplId)
        {
            return _createItemDelegate(_instance, id, tplId);
        }
    }
}
