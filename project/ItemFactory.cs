using Aki.Reflection.Utils;
using Comfort.Common;
using EFT;
using EFT.Ballistics;
using EFT.InventoryLogic;
using System;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace SamSWAT.FireSupport
{
    static class ItemFactory
    {
        private static object _instance;
        private static MethodBase _methodCreateItem;
        public static bool Instantiated;

        public static void Init()
        {
            var itemFactoryType = PatchConstants.EftTypes.Single(x => x.GetMethod("LogErrors") != null);
            var singletonType = typeof(Singleton<>).MakeGenericType(itemFactoryType);
            _instance = singletonType.GetProperty("Instance")?.GetValue(singletonType);
            _methodCreateItem = itemFactoryType.GetMethod("CreateItem");
            Instantiated = true;
        }

		public static Item CreateItem(string id, string tpid)
		{
			return (Item)_methodCreateItem.Invoke(_instance, new object[] { id, tpid, null });
		}
	}

    static class WeaponClass
    {
        private static BallisticsCalculator _ballisticsCalc;
        private static Player _player;
        private static Weapon _weapon;
        private static MethodInfo _methodShoot;
        private static MethodBase _methodCreateShot;
        private static bool _instantiated;

        public static void Init()
        {
            if (_instantiated)
            {
                _ballisticsCalc = Singleton<GameWorld>.Instance._sharedBallisticsCalculator;
                _player = Singleton<GameWorld>.Instance.RegisteredPlayers[0];
                _weapon = (Weapon)ItemFactory.CreateItem(
                    Guid.NewGuid().ToString("N").Substring(0, 24), 
                    "weapon_ge_gau8_avenger_30x173");
            }
            else
            {
                _ballisticsCalc = Singleton<GameWorld>.Instance._sharedBallisticsCalculator;
                Type type = _ballisticsCalc.GetType();
                _methodShoot = type.GetMethod("Shoot");
                _methodCreateShot = type.GetMethod("CreateShot");
                _player = Singleton<GameWorld>.Instance.RegisteredPlayers[0];
                _weapon = (Weapon)ItemFactory.CreateItem(
                    Guid.NewGuid().ToString("N").Substring(0, 24), 
                    "weapon_ge_gau8_avenger_30x173");
                _instantiated = true;
            }
        }

        public static object FireProjectile(object ammo, Vector3 shotPosition, Vector3 shotDirection, float speedFactor)
        {
            if (ammo == null)
                ammo = GetAmmo("ammo_30x173_gau8_avenger");
            object obj = _methodCreateShot.Invoke(_ballisticsCalc, new[]
            {
                ammo, 
                shotPosition, 
                shotDirection, 
                0, 
                _player, 
                _weapon, 
                speedFactor, 
                0
            });
            _methodShoot.Invoke(_ballisticsCalc, new[] { obj });
            return obj;
        }
        public static object GetAmmo(string tid)
        {
            return ItemFactory.CreateItem(Guid.NewGuid().ToString("N").Substring(0, 24), tid);
        }
    }
}
