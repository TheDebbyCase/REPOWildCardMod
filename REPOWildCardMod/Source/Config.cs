using System.Collections.Generic;
using BepInEx.Configuration;
using HarmonyLib;
using REPOWildCardMod.Utils;
using REPOWildCardMod.Valuables;
using UnityEngine;
namespace REPOWildCardMod.Config
{
    public class WildCardConfig
    {
        readonly BepInEx.Logging.ManualLogSource log = WildCardMod.instance.log;
        internal readonly List<ConfigEntry<bool>> isValEnabled = new List<ConfigEntry<bool>>();
        internal readonly List<ConfigEntry<bool>> isItemEnabled = new List<ConfigEntry<bool>>();
        internal readonly List<ConfigEntry<bool>> isReskinEnabled = new List<ConfigEntry<bool>>();
        internal readonly List<ConfigEntry<float>> reskinChance = new List<ConfigEntry<float>>();
        internal readonly List<List<ConfigEntry<float>>> reskinVariantChance = new List<List<ConfigEntry<float>>>();
        internal readonly List<ConfigEntry<bool>> isAudioReplacerEnabled = new List<ConfigEntry<bool>>();
        internal readonly List<ConfigEntry<float>> audioReplaceChance = new List<ConfigEntry<float>>();
        internal readonly List<List<ConfigEntry<float>>> audioReplacerVariantChance = new List<List<ConfigEntry<float>>>();
        internal readonly ConfigEntry<bool> noteDestroy;
        internal readonly ConfigEntry<string> wishBlacklist;
        internal readonly ConfigEntry<bool> vanillaOnlyUpgrades;
        internal readonly Dictionary<string, ConfigEntry<string>> wishMaxPerDict = new Dictionary<string, ConfigEntry<string>>();
        internal readonly ConfigEntry<int> wishCurrencyAmount;
        internal readonly ConfigEntry<int>[] wishValuableAmounts = new ConfigEntry<int>[8];
        internal readonly ConfigEntry<bool> harmonyPatches;
        internal WildCardConfig(List<GameObject> valList, List<GameObject> itemList, List<Reskin> reskinList, List<AudioReplacer> audioReplacerList)
        {
            WildCardMod.instance.Config.SaveOnConfigSet = false;
            for (int i = 0; i < valList.Count; i++)
            {
                isValEnabled.Add(WildCardMod.instance.Config.Bind("Valuables", $"Enable {valList[i].name}?", true));
                log.LogDebug($"Added config for {valList[i].name}");
            }
            for (int i = 0; i < itemList.Count; i++)
            {
                isItemEnabled.Add(WildCardMod.instance.Config.Bind("Items", $"Enable {itemList[i].name}?", true));
                log.LogDebug($"Added config for {itemList[i].name}");
            }
            noteDestroy = WildCardMod.instance.Config.Bind("Items", "Destroy Smith Note on Enemy Kill?", false, "By default, the Smith note loses half of its charge and is rechargeable");
            for (int i = 0; i < reskinList.Count; i++)
            {
                if (reskinList[i].identifier != "")
                {
                    isReskinEnabled.Add(WildCardMod.instance.Config.Bind("Reskins", $"Enable {reskinList[i].identifier} reskin?", true));
                    reskinChance.Add(WildCardMod.instance.Config.Bind("Reskins", $"Chance for {reskinList[i].identifier} reskin", reskinList[i].replaceChance.value, "Decimal between 0 and 1"));
                    reskinVariantChance.Add(new List<ConfigEntry<float>>());
                    if (reskinList[i].variantChances.Length > 1)
                    {
                        for (int j = 0; j < reskinList[i].variantChances.Length; j++)
                        {
                            reskinVariantChance[i].Add(WildCardMod.instance.Config.Bind("Reskins", $"Chance for variant {j + 1} of {reskinList[i].identifier} reskin", reskinList[i].variantChances[j].value, "Decimal between 0 and 1"));
                        }
                    }
                    log.LogDebug($"Added configs for {reskinList[i].identifier} reskin, had {reskinVariantChance[i].Count} variants");
                }
                else
                {
                    log.LogError($"Reskin at index {i} was not valid!");
                }
            }
            for (int i = 0; i < audioReplacerList.Count; i++)
            {
                if (audioReplacerList[i].identifier != "")
                {
                    isAudioReplacerEnabled.Add(WildCardMod.instance.Config.Bind("Audio Replacers", $"Enable {audioReplacerList[i].identifier} audio replacer?", true));
                    audioReplaceChance.Add(WildCardMod.instance.Config.Bind("Audio Replacers", $"Chance for {audioReplacerList[i].identifier} audio replacer", audioReplacerList[i].replaceChance.value, "Decimal between 0 and 1"));
                    audioReplacerVariantChance.Add(new List<ConfigEntry<float>>());
                    if (audioReplacerList[i].variantChances.Length > 1)
                    {
                        for (int j = 0; j < audioReplacerList[i].variantChances.Length; j++)
                        {
                            audioReplacerVariantChance[i].Add(WildCardMod.instance.Config.Bind("Audio Replacers", $"Chance for variant {j + 1} of {audioReplacerList[i].identifier} audio replacer", audioReplacerList[i].variantChances[j].value, "Decimal between 0 and 1"));
                        }
                    }
                    log.LogDebug($"Added configs for {audioReplacerList[i].identifier} audio replacer, had {audioReplacerVariantChance[i].Count} variants");
                }
                else
                {
                    log.LogError($"Audio Replacer at index {i} was not valid!");
                }
            }
            wishBlacklist = WildCardMod.instance.Config.Bind("Dragon Balls", "Wish Upgrades Blacklist", "Crouch Rest", "Which upgrades should be blacklisted from being given by Shenron as a part of the wish? Example: \"Health, Stamina, Launch\"");
            log.LogDebug("Added config for Dragon Ball wish upgrades blacklist");
            vanillaOnlyUpgrades = WildCardMod.instance.Config.Bind("Dragon Balls", "Vanilla Only Wish Upgrades", true, "Whether to only use vanilla upgrades for the Shenron's Wish reward");
            log.LogDebug("Added config for Dragon Ball wish upgrades vanilla only");
            wishCurrencyAmount = WildCardMod.instance.Config.Bind("Dragon Balls", "Wish Currency Amount", 20, "Amount of money given by Shenron's wish if all 7 dragon balls are collected in the last extraction point of a level (multiplied by 1000)");
            string[] valuableSizes = new string[] { "Tiny", "Small", "Medium", "Big" };
            int[] valuableAmountDefaults = new int[] { 15, 20, 9, 12, 6, 9, 3, 6 };
            int stringIndex = 0;
            for (int i = 0; i < wishValuableAmounts.Length; i++)
            {
                string minOrMax;
                int remainder = i % 2;
                if (remainder == 0)
                {
                    minOrMax = "Minimum";
                }
                else
                {
                    minOrMax = "Maximum";
                }
                wishValuableAmounts[i] = WildCardMod.instance.Config.Bind("Dragon Balls", $"Wish {valuableSizes[stringIndex]} Valuable {minOrMax} Amount (inclusive)", valuableAmountDefaults[i]);
                stringIndex += remainder;
            }
            log.LogDebug("Added configs for Dragon Ball wish valuable amounts");
            harmonyPatches = WildCardMod.instance.Config.Bind("Advanced", "Enable Giwi Harmony Patches?", true, "Only change this if you know what you're doing. Setting this to false will make Giwi Worm work unexpectedly");
            log.LogDebug("Added config for turning off Giwi harmony patches");
            WildCardMod.instance.Config.Save();
            WildCardMod.instance.Config.SaveOnConfigSet = true;
        }
        public void WishUpgradesConfig(List<string> upgradeNames)
        {
            WildCardMod.instance.Config.SaveOnConfigSet = false;
            upgradeNames.RemoveAll(ShenronHUD.wishBlacklist.Contains);
            for (int i = 0; i < upgradeNames.Count; i++)
            {
                wishMaxPerDict.Add(upgradeNames[i], WildCardMod.instance.Config.Bind("Dragon Balls", $"{upgradeNames[i]} Maximum Upgrades / Maximum Upgrades Per Wish", "0, 5", "Set the maximum upgrades possible to be given by Shenron's Wish and the maximum upgrades Shenron's Wish can add per wish, separated by \", \""));
            }
            log.LogDebug("Added config for Dragon Ball wish upgrades maximum and per wish amounts");
            (AccessTools.Property(typeof(ConfigFile), "OrphanedEntries").GetValue(WildCardMod.instance.Config) as Dictionary<ConfigDefinition, string>).Clear();
            WildCardMod.instance.Config.Save();
            WildCardMod.instance.Config.SaveOnConfigSet = true;
        }
    }
}