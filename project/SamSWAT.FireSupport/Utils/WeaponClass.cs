using System;
using System.Reflection;
using Comfort.Common;
using EFT;
using EFT.Ballistics;
using EFT.InventoryLogic;
using UnityEngine;

namespace SamSWAT.FireSupport.Utils
{
    internal static class WeaponClass
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

        public static void FireProjectile(object ammo, Vector3 shotPosition, Vector3 shotDirection, float speedFactor)
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
        }
        
        public static object GetAmmo(string tid)
        {
            return ItemFactory.CreateItem(Guid.NewGuid().ToString("N").Substring(0, 24), tid);
        }
    }
}