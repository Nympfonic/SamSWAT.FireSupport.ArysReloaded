using EFT.InventoryLogic;
using HarmonyLib;
using SPT.Reflection.Patching;
using System;
using System.Reflection;

namespace SamSWAT.FireSupport.ArysReloaded.Utils
{
	internal class ItemFactoryUtil : ModulePatch
	{
		internal static readonly Type Type = typeof(ItemFactory);
		private static readonly MethodInfo _createItemMethod = AccessTools.DeclaredMethod(Type, nameof(ItemFactory.CreateItem));

		private static CreateItemDelegate _createItemDelegate;
		private delegate Item CreateItemDelegate(string id, string tplId, object diff = null);

		protected override MethodBase GetTargetMethod()
		{
			return Type.GetConstructors()[0];
		}

		[PatchPostfix]
		private static void PatchPostfix(ItemFactory __instance)
		{
			_createItemDelegate = AccessTools.MethodDelegate<CreateItemDelegate>(_createItemMethod, __instance, false);
		}

		public static Item CreateItem(string id, string tplId)
		{
			return _createItemDelegate(id, tplId);
		}
	}
}
