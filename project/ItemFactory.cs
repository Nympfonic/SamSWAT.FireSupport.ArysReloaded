using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Aki.Reflection.Utils;
using Comfort.Common;
using EFT;
using EFT.Ballistics;
using EFT.InventoryLogic;
using UnityEngine;

namespace SamSWAT.FireSupport
{
    static class ItemFactory
    {
        private static object _instance;
        private static MethodBase _methodCreateItem;
        private static Dictionary<string, ItemTemplate> _items = new Dictionary<string, ItemTemplate>();
        public static bool Instantiated;
        public static void Init()
        {
            var itemFactoryType = PatchConstants.EftTypes.Single(x => x.GetMethod("LogErrors") != null);
            var singletonType = typeof(Singleton<>).MakeGenericType(itemFactoryType);
            _instance = singletonType.GetProperty("Instance").GetValue(singletonType);
            _methodCreateItem = itemFactoryType.GetMethod("CreateItem");
            _items = (Dictionary<string, ItemTemplate>)itemFactoryType.GetField("ItemTemplates").GetValue(_instance);
            Instantiated = true;
        }
		public static Item CreateItem(string id, string tpid)
		{
			return (Item)_methodCreateItem.Invoke(_instance, new object[] { id, tpid, null });
		}
		public static ItemTemplate GetItemTemplateById(string id)
		{
			return _items[id];
		}
	}
    static class WeaponClass
    {
        private static BallisticsCalculator _ballisticsCalc;
        private static Player _player;
        private static Weapon _weapon;
        private static MethodInfo _methodShoot;
        private static MethodBase _methodCreateShot;
        public static bool Instantiated;
        public static void Init()
        {
            _ballisticsCalc = Singleton<GameWorld>.Instance._sharedBallisticsCalculator;
            Type type = _ballisticsCalc.GetType();
            _methodShoot = type.GetMethod("Shoot");
            _methodCreateShot = type.GetMethod("CreateShot");
            _player = Singleton<GameWorld>.Instance.RegisteredPlayers[0];
            _weapon = (Weapon)ItemFactory.CreateItem(Guid.NewGuid().ToString("N").Substring(0, 24), "5d52cc5ba4b9367408500062");
            Instantiated = true;
        }
        public static object FireProjectile(object ammo, Vector3 shotPosition, Vector3 shotDirection, float speedFactor)
        {
            if (ammo == null)
                ammo = GetBullet("5d70e500a4b9364de70d38ce");
            object obj = _methodCreateShot.Invoke(_ballisticsCalc, new object[] { ammo, shotPosition, shotDirection, 0, _player, _weapon, speedFactor, 0 });
            _methodShoot.Invoke(_ballisticsCalc, new object[] { obj });
            return obj;
        }
        public static object GetBullet(string tid)
        {
            return ItemFactory.CreateItem(Guid.NewGuid().ToString("N").Substring(0, 24), tid);
        }
    }
}
