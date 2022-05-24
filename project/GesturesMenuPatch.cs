using Aki.Reflection.Patching;
using Comfort.Common;
using EFT;
using EFT.UI.Gestures;
using EFT.UI;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace SamSWAT.FireSupport
{
    public class GesturesMenuPatch : ModulePatch
    {
        private static GameObject _uiGameObject;
        protected override MethodBase GetTargetMethod()
        {
            return typeof(GesturesMenu).GetMethod("Init", BindingFlags.Public | BindingFlags.Instance);
        }

        [PatchPostfix]
        public static async void PatchPostfix(GesturesMenu __instance)
        {
            if (Singleton<GameWorld>.Instantiated && Plugin.PluginEnabled.Value && _uiGameObject == null)
            {
                Player player = Singleton<GameWorld>.Instance.RegisteredPlayers[0];
                bool playerHasRangefinder = player.Profile.Inventory.AllRealPlayerItems.Any(x => x.TemplateId == "61605e13ffa6e502ac5e7eef");

                if (playerHasRangefinder && player is LocalPlayer)
                {
                    if (!ItemFactory.Instantiated)
                        ItemFactory.Init();
                    if (!WeaponClass.Instantiated)
                        WeaponClass.Init();
                    GameObject fireSupportUI = await Utils.LoadAssetAsync("assets/content/ui/FireSupport_UI.bundle", "FireSupportUI");
                    _uiGameObject = Object.Instantiate(fireSupportUI, __instance.transform);
                    //go.transform.localPosition = Vector3.zero;
                    Utils.FireSupportUI = _uiGameObject.GetComponent<FireSupportUI>();
                    Utils.FireSupportUI.Init(__instance);
                    Singleton<GUISounds>.Instance.PlaySound(Utils.GetRandomAudio(Utils.FireSupportUI.StationReminder));
                    //Utils.UnloadBundle("firesupport_ui.bundle");
                    //__instance.gameObject.AddComponent<FireSupportUI>();
                }
            }
        }
    }
}
