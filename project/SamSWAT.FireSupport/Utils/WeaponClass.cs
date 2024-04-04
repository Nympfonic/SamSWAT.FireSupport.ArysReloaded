using Comfort.Common;
using EFT;
using EFT.Ballistics;
using EFT.InventoryLogic;
using SamSWAT.FireSupport.ArysReloaded.Unity;
using System;
using UnityEngine;

namespace SamSWAT.FireSupport.ArysReloaded.Utils
{
    internal static class WeaponClass
    {
        private static BallisticsCalculator _calc;
        private static string _player;
        private static Weapon _gau8Weapon;
        private static bool _instantiated;

        private static readonly CreateShotDelegate CreateShot;
        private static readonly Action<BallisticsCalculator, object> Shoot;

        private delegate object CreateShotDelegate(BallisticsCalculator instance, object ammo, Vector3 origin,
            Vector3 direction, int fireIndex, string player, Item weapon, float speedFactor = 1f, int fragmentIndex = 0);

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
            var gameWorld = Singleton<GameWorld>.Instance;
            _calc = gameWorld._sharedBallisticsCalculator;
            _player = gameWorld.MainPlayer.ProfileId;

            var newId = Guid.NewGuid().ToString("N").Substring(0, 24);
            _gau8Weapon = (Weapon)ItemFactoryUtil.CreateItem(newId, ItemConstants.GAU8_WEAPON_TPL);
        }

        public static void FireProjectile(object ammo, Vector3 origin, Vector3 direction)
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