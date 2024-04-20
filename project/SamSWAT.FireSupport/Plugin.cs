using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using SamSWAT.FireSupport.ArysReloaded.Patches;
using SamSWAT.FireSupport.ArysReloaded.Utils;
using System.IO;
using System.Reflection;
using UnityEngine;

namespace SamSWAT.FireSupport.ArysReloaded
{
    [BepInPlugin("com.SamSWAT.FireSupport.ArysReloaded", "SamSWAT.FireSupport.ArysReloaded", "3.0.0")]
    public class Plugin : BaseUnityPlugin
    {
        internal static string Directory;
        internal static ManualLogSource LogSource;

        // Mod Config
        internal static ConfigEntry<bool> Enabled;

        // Fire Support Config
        internal static ConfigEntry<string> SupportCostCurrency;
        internal static ConfigEntry<int> RequestCooldown;

        // Audio Config
        internal static ConfigEntry<int> VoiceoverVolume;

        // A-10 Config
        internal static ConfigEntry<int> AmountOfStrafeRequests;
        internal static ConfigEntry<int> StrafeCost;

        // Black Hawk Config
        internal static ConfigEntry<int> AmountOfExtractionRequests;
        internal static ConfigEntry<int> HelicopterCost;
        internal static ConfigEntry<int> HelicopterWaitTime;
        internal static ConfigEntry<float> HelicopterExtractTime;
        internal static ConfigEntry<float> HelicopterSpeedMultiplier;

        // Apache Config
        internal static ConfigEntry<int> AmountOfApacheRequests;
        internal static ConfigEntry<int> ApacheCost;
        internal static ConfigEntry<float> ApacheActiveTime;

        private void Awake()
        {
            LogSource = Logger;
            Directory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "/";

            BindConfigs();

            new GesturesMenuInitPatch().Enable();
            new GesturesMenuShowPatch().Enable();
            new AddItemToDatabasePatch().Enable();
            new AddLocaleToDatabasePatch().Enable();
            new BotKilledPatch().Enable();

            new GameObject("Fire Support UpdateManager").AddComponent<UpdateManager>();
        }

        private void BindConfigs()
        {
            string main = "1. Main Settings";
            string sound = "2. Sound Settings";
            string strafe = "3. A-10 Strafe Settings";
            string extract = "4. UH-60 Extraction Settings";
            string apache = "5. AH-64 Apache Settings";

            Enabled = Config.Bind("", "Plugin state", true, new ConfigDescription("Enables/disables plugin"));

            SupportCostCurrency = Config.Bind(main, "1. Currency to use for support requests", "RUB", new ConfigDescription("", new AcceptableValueList<string>("RUB", "EUR", "USD")));
            RequestCooldown = Config.Bind(main, "2. Cooldown between support requests", 300, new ConfigDescription("Seconds", new AcceptableValueRange<int>(30, 3600)));

            VoiceoverVolume = Config.Bind(sound, "1. Voiceover volume", 90, new ConfigDescription("", new AcceptableValueRange<int>(0, 100)));

            AmountOfStrafeRequests = Config.Bind(strafe, "1. Amount of autocannon strafe requests", 2, new ConfigDescription("", new AcceptableValueRange<int>(0, 10)));
            StrafeCost = Config.Bind(strafe, "2. Cost for each strafe support request", 0, new ConfigDescription("", new AcceptableValueRange<int>(0, 500000)));

            AmountOfExtractionRequests = Config.Bind(extract, "1. Amount of helicopter extraction requests", 1, new ConfigDescription("", new AcceptableValueRange<int>(0, 10)));
            HelicopterCost = Config.Bind(extract, "2. Cost for each helicopter support request", 0, new ConfigDescription("", new AcceptableValueRange<int>(0, 500000)));
            HelicopterWaitTime = Config.Bind(extract, "3. Helicopter wait time", 30, new ConfigDescription("Helicopter wait time on extraction location (seconds)", new AcceptableValueRange<int>(10, 300)));
            HelicopterExtractTime = Config.Bind(extract, "4. Extraction time", 10f, new ConfigDescription("How long you will need to stay in the exfil zone before extraction (seconds)", new AcceptableValueRange<float>(1f, 30f)));
            HelicopterSpeedMultiplier = Config.Bind(extract, "5. Helicopter speed multiplier", 1f, new ConfigDescription("How fast the helicopter arrival animation will be played", new AcceptableValueRange<float>(0.8f, 1.5f)));

            AmountOfApacheRequests = Config.Bind(apache, "1. Amount of Apache fire support requests", 1, new ConfigDescription("", new AcceptableValueRange<int>(0, 10)));
            ApacheCost = Config.Bind(apache, "2. Cost for each Apache support request", 0, new ConfigDescription("", new AcceptableValueRange<int>(0, 500000)));
            ApacheActiveTime = Config.Bind(apache, "3. Apache active field time", 20f, new ConfigDescription("How long the Apache will provide covering fire for after it arrives (seconds)", new AcceptableValueRange<float>(10f, 120f)));
        }
    }
}
