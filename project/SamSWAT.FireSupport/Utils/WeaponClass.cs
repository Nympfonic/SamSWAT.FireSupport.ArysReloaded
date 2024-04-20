using Comfort.Common;
using EFT;
using EFT.Ballistics;
using EFT.InventoryLogic;
using SamSWAT.FireSupport.ArysReloaded.Unity;
using UnityEngine;

namespace SamSWAT.FireSupport.ArysReloaded.Utils
{
    internal static class WeaponClass
    {
        private static BallisticsCalculator _calc;
        private static string _playerProfileId;
        private static Weapon _gau8Weapon;
        private static Weapon _m230Weapon;
        private static Weapon _hydra70Weapon;

        public static void Init()
        {
            var itemFactory = ModHelper.ItemFactory;
            _calc = ModHelper.GameWorld?._sharedBallisticsCalculator;
            _playerProfileId = ModHelper.MainPlayer?.ProfileId;

            _gau8Weapon = (Weapon)itemFactory.CreateItem(MongoID.Generate(), ModHelper.GAU8_WEAPON_TPL, null);
            _m230Weapon = (Weapon)itemFactory.CreateItem(MongoID.Generate(), ModHelper.M230_WEAPON_TPL, null);
            _hydra70Weapon = (Weapon)itemFactory.CreateItem(MongoID.Generate(), ModHelper.HYDRA70_WEAPON_TPL, null);
        }

        public static void FireProjectile(WeaponType weaponType, BulletClass ammo, Vector3 origin, Vector3 direction)
        {
            EftBulletClass projectile;
            switch (weaponType)
            {
                case WeaponType.GAU8:
                    projectile = _calc.CreateShot(ammo, origin, direction, 0, _playerProfileId, _gau8Weapon);
                    break;
                case WeaponType.M230:
                    projectile = _calc.CreateShot(ammo, origin, direction, 0, _playerProfileId, _m230Weapon);
                    break;
                case WeaponType.Hydra70:
                    projectile = _calc.CreateShot(ammo, origin, direction, 0, _playerProfileId, _hydra70Weapon);
                    break;
                default:
                    return;
            }

            _calc.Shoot(projectile);
        }

        public static BulletClass GetAmmo(string tpl)
        {
            return ModHelper.ItemFactory.CreateItem(MongoID.Generate(), tpl, null) as BulletClass;
        }
    }
}