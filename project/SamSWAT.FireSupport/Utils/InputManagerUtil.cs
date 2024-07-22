using EFT.InputSystem;
using HarmonyLib;
using SPT.Reflection.Patching;
using System.Reflection;

namespace SamSWAT.FireSupport.ArysReloaded.Utils
{
	internal class InputManagerUtil : ModulePatch
	{
		private static InputManager _inputManager;

		protected override MethodBase GetTargetMethod()
		{
			return AccessTools.DeclaredMethod(typeof(InputManager), nameof(InputManager.Create));
		}

		[PatchPostfix]
		private static void PatchPostfix(InputManager __result)
		{
			_inputManager = __result;
		}

		internal static InputManager GetInputManager()
		{
			return _inputManager;
		}
	}
}
