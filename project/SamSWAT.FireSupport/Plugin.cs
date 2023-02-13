using BepInEx;
using BepInEx.Configuration;
using SamSWAT.FireSupport.Database;
using SamSWAT.FireSupport.Patches;
using System;
using System.IO;
using System.Reflection;
using SamSWAT.FireSupport.Unity;

namespace SamSWAT.FireSupport
{
    [BepInPlugin("com.SamSWAT.FireSupport", "SamSWAT.FireSupport", "2.0.0")]
    public class Plugin : BaseUnityPlugin
    {
        public static string Directory;
        internal static ConfigEntry<bool> PluginEnabled;
        internal static ConfigEntry<int> AmountOfStrafeRequests;
        internal static ConfigEntry<int> AmountOfExtractionRequests;
        internal static ConfigEntry<int> HelicopterWaitTime;
        internal static ConfigEntry<float> HelicopterExtractTime;
        internal static ConfigEntry<float> HelicopterSpeedMultiplier;
        internal static ConfigEntry<int> RequestCooldown;
        internal static ConfigEntry<int> VoiceoverVolume;

        private void Awake()
        {
            //Directory = Path.Combine(BepInEx.Paths.PluginPath, "SamSWAT.FireSupport/").Replace("\\", "/");
            Directory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "/";
            new GesturesMenuPatch().Enable();
            new AddItemToDatabasePatch().Enable();
            new AddLocaleToDatabasePatch().Enable();

            PluginEnabled = Config.Bind(
                "",
                "Plugin state",
                true,
                new ConfigDescription("Enables/disables plugin"));
            AmountOfStrafeRequests = Config.Bind(
                "Main Settings",
                "Amount of autocannon strafe requests",
                2,
                new ConfigDescription("",
                new AcceptableValueRange<int>(0, 10)));
            AmountOfExtractionRequests = Config.Bind(
                "Main Settings",
                "Amount of helicopter extraction requests",
                1,
                new ConfigDescription("",
                    new AcceptableValueRange<int>(0, 10)));
            HelicopterWaitTime = Config.Bind(
                "Helicopter Extraction Settings",
                "Helicopter wait time",
                30,
                new ConfigDescription("Helicopter wait time on extraction location (seconds)",
                    new AcceptableValueRange<int>(10, 300)));
            HelicopterExtractTime = Config.Bind(
                "Helicopter Extraction Settings",
                "Extraction time",
                10f,
                new ConfigDescription("How long you will need to stay in the exfil zone before extraction (seconds)",
                    new AcceptableValueRange<float>(1f, 30f)));
            HelicopterSpeedMultiplier = Config.Bind(
                "Helicopter Extraction Settings",
                "Helicopter speed multiplier",
                1f,
                new ConfigDescription("How fast the helicopter arrival animation will be played",
                    new AcceptableValueRange<float>(0.8f, 1.5f)));
            RequestCooldown = Config.Bind(
                "Main Settings",
                "Cooldown between support requests",
                300,
                new ConfigDescription("Seconds",
                new AcceptableValueRange<int>(60, 3600)));
            VoiceoverVolume = Config.Bind(
                "Sound Settings",
                "Voiceover volume",
                90,
                new ConfigDescription("",
                new AcceptableValueRange<int>(0, 100)));
            VoiceoverVolume.SettingChanged += VoiceoverVolume_SettingChanged;
        }

        private static void VoiceoverVolume_SettingChanged(object sender, EventArgs e)
        {
            if (FireSupportAudio.Instance == null)
                return;
            FireSupportAudio.Instance.AudioSource.volume = (float)VoiceoverVolume.Value/100;
        }
    }
}
