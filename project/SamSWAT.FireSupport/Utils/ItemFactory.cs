using System;
using System.Linq;
using System.Reflection;
using Aki.Reflection.Utils;
using Comfort.Common;
using EFT.InventoryLogic;

namespace SamSWAT.FireSupport.Utils
{
    internal static class ItemFactory
    {
	    public static Type Type;
        private static object _instance;
        private static MethodBase _methodCreateItem;
        public static bool Instantiated;

        static ItemFactory()
        {
	        Type = PatchConstants.EftTypes.Single(x => x.GetMethod("LogErrors") != null);
        }

        public static void Init()
        {
            var singletonType = typeof(Singleton<>).MakeGenericType(Type);
            _instance = singletonType.GetProperty("Instance")?.GetValue(singletonType);
            _methodCreateItem = Type.GetMethod("CreateItem");
            Instantiated = true;
        }

		public static Item CreateItem(string id, string tpid)
		{
			return (Item)_methodCreateItem.Invoke(_instance, new object[] { id, tpid, null });
		}
	}
}
