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
        internal WildCardConfig(ConfigFile cfg, List<GameObject> valList)
        {
            cfg.SaveOnConfigSet = false;
            for (int i = 0; i < valList.Count; i++)
            {
                bool defaultEnabled = true;
                isValEnabled.Add(cfg.Bind("Valuables", $"Enable {valList[i].name}?", defaultEnabled));
                log.LogDebug($"Added config for {valList[i].name}");
            }
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