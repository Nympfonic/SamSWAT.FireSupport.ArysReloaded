using Aki.Reflection.Utils;
using Comfort.Common;
using EFT;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using Random = UnityEngine.Random;

namespace SamSWAT.FireSupport.ArysReloaded.Utils
{
    internal static class ModHelper
    {
        internal const string MOD_NAME = "SamSWAT.FireSupport.ArysReloaded";

        internal const string
            A10_NAME = "A-10 Thunderbolt II",
            UH60_NAME = "UH-60 Black Hawk",
            AH64_NAME = "AH-64 Apache";

        internal const string
            GAU8_AMMO_TPL = "660b2d05cec10101410e7d7a",
            GAU8_WEAPON_TPL = "660b2d05cec10101410e7d7b",
            M230_AMMO_TPL = "660e250682ff8cfe9109f34d",
            M230_WEAPON_TPL = "660e250682ff8cfe9109f34e",
            HYDRA70_AMMO_TPL = "660e250682ff8cfe9109f34f",
            HYDRA70_WEAPON_TPL = "660e250682ff8cfe9109f350",
            RANGEFINDER_TPL = "61605e13ffa6e502ac5e7eef";

        internal static readonly Dictionary<string, string> ItemMappings = new Dictionary<string, string>()
        {
            { GAU8_AMMO_TPL, "ammo_30x173_gau8_avenger" },
            { GAU8_WEAPON_TPL, "weapon_ge_gau8_avenger_30x173" },
            { M230_AMMO_TPL, "ammo_30x113_m230_autocannon" },
            { M230_WEAPON_TPL , "weapon_m230_autocannon_30x113" },
            { HYDRA70_AMMO_TPL , "ammo_70x1060_hydra_70" },
            { HYDRA70_WEAPON_TPL , "weapon_hydra_70_70x1060" }
        };

        internal static readonly Vector3 VECTOR3_IGNORE_Y = new Vector3(1, 0, 1);

        internal static GameWorld GameWorld =>
            Singleton<GameWorld>.Instance;

        internal static Player MainPlayer =>
            GameWorld?.MainPlayer;

        internal static BetterAudio BetterAudio =>
            Singleton<BetterAudio>.Instance;

        internal static ItemFactory ItemFactory =>
            Singleton<ItemFactory>.Instance;

        internal static BotSpawner BotSpawner =>
            Singleton<IBotGame>.Instance?.BotsController?.BotSpawner;

        internal static LayerMask LayerMaskWithBot =>
            LayerMaskClass.HighPolyWithTerrainMaskAI;

        internal static Vector3 GetBodyPosition(Player player)
        {
            return player.MainParts[BodyPartType.body].Position;
        }

        internal static bool HasRangefinderInHands()
        {
            var player = MainPlayer;
            if (player is null)
            {
                return false;
            }

            var handsController = player.HandsController;
            if (handsController is null)
            {
                return false;
            }

            return handsController.Item?.TemplateId == RANGEFINDER_TPL;
        }

        internal static string Localized(string id, EStringCase @case)
        {
            return (string)LocalizedMethod.Invoke(null, new object[] { id, @case });
        }

        internal static AudioClip GetRandomClip(AudioClip[] audioClips)
        {
            return audioClips[Random.Range(0, audioClips.Length)];
        }

        internal static string GetCurrencyByName(string name)
        {
            switch (name)
            {
                default:
                case "RUB": return "5449016a4bdc2d6f028b456f";
                case "EUR": return "569668774bdc2da2298b4568";
                case "USD": return "5696686a4bdc2da3298b456a";
            }
        }

        // Credit to Amand for this code
        private static readonly Type LocalizedType = PatchConstants.EftTypes.Single(x => x.GetMethod("ParseLocalization") != null);
        private static readonly MethodInfo LocalizedMethod = AccessTools.FirstMethod(LocalizedType, mi =>
        {
            return mi.Name == "Localized"
                && mi.GetParameters().Length == 2
                && mi.GetParameters()[0].ParameterType == typeof(string)
                && mi.GetParameters()[1].ParameterType == typeof(EStringCase);
        });
    }
}
