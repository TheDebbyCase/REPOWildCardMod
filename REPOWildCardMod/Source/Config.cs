using System.Collections.Generic;
using System.Reflection;
using BepInEx.Configuration;
using HarmonyLib;
using REPOWildCardMod.Utils;
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
        internal readonly ConfigEntry<bool> harmonyPatches;
        internal readonly ConfigEntry<bool> noteDestroy;
        internal WildCardConfig(ConfigFile cfg, List<GameObject> valList, List<Item> itemList, List<Reskin> reskinList)
        {
            cfg.SaveOnConfigSet = false;
            for (int i = 0; i < valList.Count; i++)
            {
                isValEnabled.Add(cfg.Bind("Valuables", $"Enable {valList[i].name}?", true));
                log.LogDebug($"Added config for {valList[i].name}");
            }
            for (int i = 0; i < itemList.Count; i++)
            {
                isItemEnabled.Add(cfg.Bind("Items", $"Enable {itemList[i].name}?", true));
                log.LogDebug($"Added config for {itemList[i].name}");
            }
            noteDestroy = cfg.Bind("Items", "Destroy Smith Note on Enemy Kill?", false, "By default, the Smith note loses half of its charge and is rechargeable");
            for (int i = 0; i < reskinList.Count; i++)
            {
                if (reskinList[i].identifier != "")
                {
                    isReskinEnabled.Add(cfg.Bind("Reskins", $"Enable {reskinList[i].identifier} reskin?", true));
                    reskinChance.Add(cfg.Bind("Reskins", $"Chance for {reskinList[i].identifier} reskin.", reskinList[i].replaceChance.value, "Decimal between 0 and 1"));
                    reskinVariantChance.Add(new List<ConfigEntry<float>>());
                    if (reskinList[i].variantChances.Length > 1)
                    {
                        for (int j = 0; j < reskinList[i].variantChances.Length; j++)
                        {
                            reskinVariantChance[i].Add(cfg.Bind("Reskins", $"Chance for variant {j + 1} of {reskinList[i].identifier} reskin", reskinList[i].variantChances[j].value, "Decimal between 0 and 1"));
                        }
                    }
                    log.LogDebug($"Added configs for {reskinList[i].identifier} reskin, had {reskinVariantChance[i].Count} variants");
                }
                else
                {
                    log.LogError($"Reskin at index {i} was not valid!");
                }
            }
            harmonyPatches = cfg.Bind("Advanced", "Enable Giwi Harmony Patches?", true, "Only change this if you know what you're doing. Setting this to false will make Giwi Worm work unexpectedly");
            ClearOrphanedEntries(cfg);
            cfg.Save();
            cfg.SaveOnConfigSet = true;
        }
        private static void ClearOrphanedEntries(ConfigFile cfg)
        {
            PropertyInfo orphanedEntriesProp = AccessTools.Property(typeof(ConfigFile), "OrphanedEntries");
            Dictionary<ConfigDefinition, string> orphanedEntries = (Dictionary<ConfigDefinition, string>)orphanedEntriesProp.GetValue(cfg);
            orphanedEntries.Clear();
        }
    }
}