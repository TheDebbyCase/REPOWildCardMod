using System.Collections.Generic;
using System.Reflection;
using BepInEx.Configuration;
using HarmonyLib;
using UnityEngine;
namespace REPOWildCardMod.Config
{
    public class WildCardConfig
    {
        readonly BepInEx.Logging.ManualLogSource log = WildCardMod.log;
        internal readonly List<ConfigEntry<bool>> isValEnabled = new List<ConfigEntry<bool>>();
        internal readonly List<ConfigEntry<bool>> isItemEnabled = new List<ConfigEntry<bool>>();
        internal readonly ConfigEntry<bool> harmonyPatches;
        internal WildCardConfig(ConfigFile cfg, List<GameObject> valList, List<Item> itemList)
        {
            cfg.SaveOnConfigSet = false;
            for (int i = 0; i < valList.Count; i++)
            {
                bool defaultEnabled = true;
                isValEnabled.Add(cfg.Bind("Valuables", $"Enable {valList[i].name}?", defaultEnabled));
                log.LogDebug($"Added config for {valList[i].name}");
            }
            for (int i = 0; i < itemList.Count; i++)
            {
                bool defaultEnabled = true;
                isItemEnabled.Add(cfg.Bind("Items", $"Enable {itemList[i].name}?", defaultEnabled));
                log.LogDebug($"Added config for {itemList[i].name}");
            }
            harmonyPatches = cfg.Bind("Advanced", "Enable Harmony Patches?", true, "Only change this if you know what you're doing. Setting this to false will make Giwi Worm work unexpectedly");
            ClearOrphanedEntries(cfg);
            cfg.Save();
            cfg.SaveOnConfigSet = true;
        }
        private static void ClearOrphanedEntries(ConfigFile cfg)
        {
            PropertyInfo orphanedEntriesProp = AccessTools.Property(typeof(ConfigFile), "OrphanedEntries");
            var orphanedEntries = (Dictionary<ConfigDefinition, string>)orphanedEntriesProp.GetValue(cfg);
            orphanedEntries.Clear();
        }
    }
}