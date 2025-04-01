using HarmonyLib;
using System.Collections.Generic;
namespace REPOWildCardMod.Patches
{
    [HarmonyPatch(typeof(StatsManager))]
    public static class StatsManagerPatches
    {
        [HarmonyPatch(nameof(StatsManager.Start))]
        [HarmonyPrefix]
        public static void AddDragonBallUpgrade(StatsManager __instance)
        {
            __instance.dictionaryOfDictionaries.Add("playerUpgradeDragonBalls", new Dictionary<string, int>());
        }
    }
}