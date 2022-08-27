using Aki.Reflection.Patching;
using Comfort.Common;
using EFT;
using EFT.UI.Gestures;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace SamSWAT.FireSupport
{
    public class GesturesMenuPatch : ModulePatch
    {
        private static GameObject _fireSupportUI;

        protected override MethodBase GetTargetMethod()
        {
            return typeof(GesturesMenu).GetMethod("Init", BindingFlags.Public | BindingFlags.Instance);
        }

        [PatchPostfix]
        public static async void PatchPostfix(GesturesMenu __instance)
        {
            if (Plugin.PluginEnabled.Value && Singleton<GameWorld>.Instantiated && _fireSupportUI == null)
            {
                Player player = Singleton<GameWorld>.Instance.RegisteredPlayers[0];
                bool playerHasRangefinder = player.Profile.Inventory.AllRealPlayerItems.Any(x => x.TemplateId == "61605e13ffa6e502ac5e7eef");

                if (playerHasRangefinder && player is LocalPlayer)
                {
                    if (!ItemFactory.Instantiated)
                        ItemFactory.Init();
                    WeaponClass.Init();
                    _fireSupportUI = Object.Instantiate(await Utils.LoadAssetAsync<GameObject>("assets/content/ui/firesupport_ui.bundle", "FireSupportUI"));
                    _fireSupportUI.transform.parent = __instance.transform;
                    _fireSupportUI.transform.localPosition = new Vector3(0, -255, 0);
                    _fireSupportUI.transform.localScale = new Vector3(1.4f, 1.4f, 1);
                    _fireSupportUI.GetComponent<FireSupportUI>().Init(__instance);
                    FireSupportAudio.Instance.PlayVoiceover(EVoiceoverType.StationReminder);
                    __instance.gameObject.GetComponentInChildren<GesturesBindPanel>(true).transform.localPosition = new Vector3(0, -530, 0);
                    Utils.UnloadBundle("firesupport_ui.bundle");
                }
            }
        }
    }
}
