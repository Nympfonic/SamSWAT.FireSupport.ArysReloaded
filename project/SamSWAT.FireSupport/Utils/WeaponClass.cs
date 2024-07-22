using Comfort.Common;
using EFT;
using EFT.Ballistics;
using EFT.InventoryLogic;
using HarmonyLib;
using SamSWAT.FireSupport.ArysReloaded.Unity;
using System;
using System.Reflection;
using UnityEngine;

namespace SamSWAT.FireSupport.ArysReloaded.Utils
{
    internal static class WeaponClass
    {
        private static readonly Type _type = typeof(BallisticsCalculator);
        private static readonly MethodInfo _createShotMethod = AccessTools.DeclaredMethod(_type, nameof(BallisticsCalculator.CreateShot));
        private static readonly MethodInfo _shootMethod = AccessTools.DeclaredMethod(_type, nameof(BallisticsCalculator.Shoot));
        private static CreateShotDelegate _createShotDelegate;
        private static Action<EftBulletClass> _shootDelegate;

        private delegate EftBulletClass CreateShotDelegate(BulletClass ammo, Vector3 origin,
            Vector3 direction, int fireIndex, string player, Item weapon, float speedFactor = 1f, int fragmentIndex = 0);

        private static string _player;
        private static Weapon _gau8Weapon;

        public static void Init()
        {
            var gameWorld = Singleton<GameWorld>.Instance;
            var calc = gameWorld._sharedBallisticsCalculator;
            _player = gameWorld.MainPlayer.ProfileId;

            _shootDelegate = AccessTools.MethodDelegate<Action<EftBulletClass>>(_shootMethod, calc, false);
            _createShotDelegate = AccessTools.MethodDelegate<CreateShotDelegate>(_createShotMethod, calc);

            var newId = Guid.NewGuid().ToString("N").Substring(0, 24);
            _gau8Weapon = (Weapon)ItemFactoryUtil.CreateItem(newId, ItemConstants.GAU8_WEAPON_TPL);
        }

        public static void FireProjectile(BulletClass ammo, Vector3 origin, Vector3 direction)
        {
            var projectile = _createShotDelegate(ammo, origin, direction, 0, _player, _gau8Weapon);
            _shootDelegate(projectile);
        }

        public static BulletClass GetAmmo(string tid)
        {
            var id = Guid.NewGuid().ToString("N").Substring(0, 24);
            return (BulletClass)ItemFactoryUtil.CreateItem(id, tid);
        }
    }
}