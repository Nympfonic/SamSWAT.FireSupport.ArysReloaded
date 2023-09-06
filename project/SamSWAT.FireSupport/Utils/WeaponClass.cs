using System;
using Comfort.Common;
using EFT;
using EFT.Ballistics;
using EFT.InventoryLogic;
using UnityEngine;

namespace SamSWAT.FireSupport.Utils
{
    internal static class WeaponClass
    {
        private static BallisticsCalculator _calc;
        private static Player _player;
        private static Weapon _gau8Weapon;
        private static bool _instantiated;

        private static readonly CreateShotDelegate CreateShot;
        private static readonly Action<BallisticsCalculator, object> Shoot;

        private delegate object CreateShotDelegate(BallisticsCalculator instance, object ammo, Vector3 origin,
            Vector3 direction, int fireIndex, Player player, Item weapon, float speedFactor = 1f, int fragmentIndex = 0);

        static WeaponClass()
        {
            var type = typeof(BallisticsCalculator);
            var shootMethod = type.GetMethod(nameof(BallisticsCalculator.Shoot));
            var createShotMethod = type.GetMethod(nameof(BallisticsCalculator.CreateShot));
            Shoot = AccessToolsUtil.MethodDelegate<Action<BallisticsCalculator, object>>(shootMethod, false);
            CreateShot = AccessToolsUtil.MethodDelegate<CreateShotDelegate>(createShotMethod);
        }
        
        public static void Init()
        {
            _calc = Singleton<GameWorld>.Instance._sharedBallisticsCalculator;
            _player = Singleton<GameWorld>.Instance.RegisteredPlayers[0];
            
            var newId = Guid.NewGuid().ToString("N").Substring(0, 24);
            _gau8Weapon = (Weapon)ItemFactoryUtil.CreateItem(newId, "weapon_ge_gau8_avenger_30x173");
        }

        public static void FireProjectile(object ammo, Vector3 origin, Vector3 direction, float speedFactor = 1)
        {
            var projectile = CreateShot(_calc, ammo, origin, direction, 0, _player, _gau8Weapon);
            Shoot(_calc, projectile);
        }
        
        public static object GetAmmo(string tid)
        {
            var id = Guid.NewGuid().ToString("N").Substring(0, 24);
            return ItemFactoryUtil.CreateItem(id, tid);
        }
    }
}