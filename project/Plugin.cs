using BepInEx;
using BepInEx.Configuration;
using SamSWAT.FireSupport.Database;
using System.IO;

namespace SamSWAT.FireSupport
{
    [BepInPlugin("com.SamSWAT.FireSupport", "SamSWAT.FireSupport", "1.0.0")]
    public class Plugin : BaseUnityPlugin
    {
        public static string Directory;
        internal static ConfigEntry<bool> PluginEnabled;
        internal static ConfigEntry<int> AmmountOfRequets;
        internal static ConfigEntry<int> RequestCooldown;
        void Start()
        {
            Directory = Path.Combine(BepInEx.Paths.PluginPath, "SamSWAT.FireSupport/").Replace("\\", "/");
            new GesturesMenuPatch().Enable();
            new AddItem().Enable();
            new AddLocale().Enable();

            PluginEnabled = Config.Bind(
                "Main Settings",
                "Plugin state",
                true,
                new ConfigDescription("Enables/disables plugin"));
            AmmountOfRequets = Config.Bind(
                "Main Settings",
                "Amount of autocannon strafe requests",
                2,
                new ConfigDescription("",
                new AcceptableValueRange<int>(0, 10)));
            RequestCooldown = Config.Bind(
                "Main Settings",
                "Cooldown between fire support requests",
                300,
                new ConfigDescription("Seconds",
                new AcceptableValueRange<int>(100, 3600)));
        }
    }
}
