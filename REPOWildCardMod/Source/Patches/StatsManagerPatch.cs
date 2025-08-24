using HarmonyLib;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
namespace REPOWildCardMod.Patches
{
    [HarmonyPatch(typeof(StatsManager))]
    public static class StatsManagerPatches
    {
        public static bool doneConfig = false;
        [HarmonyPatch(nameof(StatsManager.Start))]
        [HarmonyPrefix]
        public static bool AddChaosDragonUpgrade(StatsManager __instance)
        {
            __instance.dictionaryOfDictionaries.Add("playerUpgradeDragonBalls", new Dictionary<string, int>());
            __instance.dictionaryOfDictionaries.Add("dragonBallsUnique", new Dictionary<string, int>());
            __instance.dictionaryOfDictionaries.Add("playerUpgradeChaosEmeralds", new Dictionary<string, int>());
            __instance.dictionaryOfDictionaries.Add("chaosEmeraldsUnique", new Dictionary<string, int>());
            return true;
        }
        [HarmonyPatch(nameof(StatsManager.RunStartStats))]
        [HarmonyPostfix]
        public static void ChaosDragonStart(StatsManager __instance)
        {
            string[] emeraldColours = new string[] { "Yellow", "Red", "Cyan", "Purple", "Green", "White", "Blue" };
            Dictionary<string, int> dragonBallsDict = new Dictionary<string, int>();
            Dictionary<string, int> chaosEmeraldsDict = new Dictionary<string, int>();
            for (int i = 0; i < 7; i++)
            {
                dragonBallsDict.Add((i + 1).ToString(), 0);
                chaosEmeraldsDict.Add(emeraldColours[i], 0);
            }
            __instance.dictionaryOfDictionaries["dragonBallsUnique"] = dragonBallsDict;
            __instance.dictionaryOfDictionaries["chaosEmeraldsUnique"] = chaosEmeraldsDict;
        }
        [HarmonyPatch(nameof(StatsManager.RunStartStats))]
        [HarmonyPostfix]
        public static void DragonBallWishUpgrades(StatsManager __instance)
        {
            if (doneConfig)
            {
                return;
            }
            List<KeyValuePair<string, Dictionary<string, int>>> dictionaryPairs = __instance.dictionaryOfDictionaries.ToList();
            List<string> upgradeStrings = new List<string>();
            Regex regex = new Regex("(?<!^)(?=[A-Z])");
            for (int i = 0; i < dictionaryPairs.Count; i++)
            {
                string upgradeString = dictionaryPairs[i].Key;
                if (!upgradeString.StartsWith("playerUpgrade"))
                {
                    continue;
                }
                string[] splitString = regex.Split(upgradeString);
                upgradeString = "";
                bool stringEnd = false;
                for (int j = 0; j < splitString.Length; j++)
                {
                    if (stringEnd)
                    {
                        upgradeString += $"{splitString[j]} ";
                    }
                    stringEnd = splitString[j] == "Upgrade" || stringEnd;
                }
                upgradeString = upgradeString.Trim();
                upgradeStrings.Add(upgradeString);
            }
            WildCardMod.instance.ModConfig.WishUpgradesConfig(upgradeStrings);
            doneConfig = true;
        }
    }
}