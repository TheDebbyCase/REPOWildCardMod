using HarmonyLib;
using System.Collections.Generic;
namespace REPOWildCardMod.Patches
{
    [HarmonyPatch(typeof(StatsManager))]
    public static class StatsManagerPatches
    {
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
    }
}