using BepInEx;
using BepInEx.Configuration;
using SamSWAT.FireSupport.Database;
using System;
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
        internal static ConfigEntry<int> VoiceoverVolume;
        internal static ConfigEntry<bool> VoiceoverNotice;

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
                new AcceptableValueRange<int>(60, 3600)));
            VoiceoverVolume = Config.Bind(
                "Sound Settings",
                "Voiceover volume",
                90,
                new ConfigDescription("",
                new AcceptableValueRange<int>(0, 100)));
            VoiceoverNotice = Config.Bind(
                "Sound Settings",
                "Voiceover when support becomes avaialble again",
                true,
                new ConfigDescription(""));
            VoiceoverVolume.SettingChanged += VoiceoverVolume_SettingChanged;
        }

        void VoiceoverVolume_SettingChanged(object sender, EventArgs e)
        {
            if (FireSupportAudio.Instance == null)
                return;
            FireSupportAudio.Instance.AudioSource.volume = (float)VoiceoverVolume.Value/100;
        }
    }
}
