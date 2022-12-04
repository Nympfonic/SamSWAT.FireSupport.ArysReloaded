using Aki.Reflection.Patching;
using Comfort.Common;
using EFT;
using EFT.Airdrop;
using EFT.UI.Gestures;
using System.Linq;
using System.Reflection;
using EFT.InventoryLogic;
using UnityEngine;

namespace SamSWAT.FireSupport
{
    public class GesturesMenuPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return typeof(GesturesMenu).GetMethod("Init", BindingFlags.Public | BindingFlags.Instance);
        }

        [PatchPostfix]
        public static async void PatchPostfix(GesturesMenu __instance)
        {
            if (!Plugin.PluginEnabled.Value ||
                !Singleton<GameWorld>.Instantiated ||
                FireSupportUI.Instance != null ||
                !LocationScene.GetAll<AirdropPoint>().Any())
                return;
            
            Player player = Singleton<GameWorld>.Instance.RegisteredPlayers[0];
            bool playerHasRangefinder = player.Profile.Inventory.AllRealPlayerItems.Any(x =>
                x.TemplateId == "61605e13ffa6e502ac5e7eef");

            if (!playerHasRangefinder || !(player is LocalPlayer)) return;
            
            if (!ItemFactory.Instantiated)
                ItemFactory.Init();
            WeaponClass.Init();
            await FireSupportAudio.Load();
            FireSupportSpotter.Load();
            FireSupportUI.Load(__instance);
            A10Behaviour.Load();
            UH60Behaviour.Load();
            __instance.gameObject.GetComponentInChildren<GesturesBindPanel>(true).transform.localPosition = new Vector3(0, -530, 0);
            FireSupportAudio.Instance.PlayVoiceover(EVoiceoverType.StationReminder);
        }
    }
}
