using Aki.Reflection.Utils;
using EFT;
using HarmonyLib;
using System;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace SamSWAT.FireSupport.ArysReloaded.Utils
{
    internal static class ReflectionHelper
    {
        private static readonly Type _physicsManagerType = PatchConstants.EftTypes.Single(type => type.GetMethod("SupportRigidbody") != null);

        private static readonly MethodInfo _supportRigidbodyMethod = AccessTools.DeclaredMethod(_physicsManagerType, "SupportRigidbody");
        private static readonly MethodInfo _unsupportRigidbodyMethod = AccessTools.DeclaredMethod(_physicsManagerType, "UnsupportRigidbody");

        private static readonly FieldInfo _playerInventoryControllerField = AccessTools.DeclaredField(typeof(Player), "_inventoryController");

        internal static void SupportRigidbody(Rigidbody rigidbody, float quality = 1f, object visibilityChecker = null)
        {
            _supportRigidbodyMethod.Invoke(null, new object[]
            {
                rigidbody,
                quality,
                visibilityChecker
            });
        }

        internal static void UnsupportRigidbody(Rigidbody rigidbody)
        {
            _unsupportRigidbodyMethod.Invoke(null, new object[] { rigidbody });
        }

        internal static InventoryControllerClass GetInventoryController(this Player player)
        {
            return _playerInventoryControllerField.GetValue(player) as InventoryControllerClass;
        }
    }
}
