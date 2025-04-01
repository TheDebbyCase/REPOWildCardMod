using HarmonyLib;
using System.Collections.Generic;
namespace REPOWildCardMod.Patches
{
    [HarmonyPatch(typeof(StatsManager))]
    public static class StatsManagerPatches
    {
        [HarmonyPatch(nameof(StatsManager.Start))]
        [HarmonyPrefix]
        public static bool AddDragonBallUpgrade(StatsManager __instance)
        {
            __instance.dictionaryOfDictionaries.Add("playerUpgradeDragonBalls", new Dictionary<string, int>());
            return true;
        }
    }
}