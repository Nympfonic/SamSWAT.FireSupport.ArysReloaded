using Aki.Reflection.Patching;
using Comfort.Common;
using EFT;
using EFT.Airdrop;
using EFT.UI.Gestures;
using System.Linq;
using System.Reflection;
using BepInEx.Logging;
using EFT.InventoryLogic;
using SamSWAT.FireSupport.Unity;
using SamSWAT.FireSupport.Utils;
using UnityEngine;

namespace SamSWAT.FireSupport.Patches
{
    public class GesturesMenuPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return typeof(GesturesMenu).GetMethod(nameof(GesturesMenu.Init));
        }

        [PatchPostfix]
        public static async void PatchPostfix(GesturesMenu __instance)
        {
            if (!IsFireSupportAvailable()) return;

            if (!Utils.ItemFactory.Instantiated)
            {
                Utils.ItemFactory.Init();
            }

            WeaponClass.Init();
            await FireSupportAudio.Load();
            await FireSupportSpotter.Load();
            await FireSupportUI.Load(__instance);
            await A10Behaviour.Load();
            await UH60Behaviour.Load();
            __instance.gameObject.GetComponentInChildren<GesturesBindPanel>(true).transform.localPosition = new Vector3(0, -530, 0);
            FireSupportAudio.Instance.PlayVoiceover(EVoiceoverType.StationReminder);
        }

        private static bool IsFireSupportAvailable()
        {
            var gameWorld = Singleton<GameWorld>.Instance;

            if (!Plugin.PluginEnabled.Value || gameWorld == null || FireSupportUI.Instance != null ||
                !LocationScene.GetAll<AirdropPoint>().Any())
            {
                return false;
            }
            
            var player = gameWorld.RegisteredPlayers[0];
            if (!(player is LocalPlayer)) return false;

            const string rangefinderTpl = "61605e13ffa6e502ac5e7eef";
            var inventory = player.Profile.Inventory;
            var hasRangefinder = inventory.AllRealPlayerItems.Any(x => x.TemplateId == rangefinderTpl);
            
            return hasRangefinder;
        }
    }
}
