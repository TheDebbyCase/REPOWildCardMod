using HarmonyLib;
using System.Collections.Generic;
namespace REPOWildCardMod.Patches
{
    [HarmonyPatch(typeof(StatsManager))]
    public static class StatsManagerPatches
    {
        [HarmonyPatch(nameof(PlayerAvatar.Start))]
        [HarmonyPrefix]
        public static void CheckForKill(StatsManager __instance)
        {
            __instance.dictionaryOfDictionaries.Add("playerUpgradeDragonBalls", new Dictionary<string, int>());
        }
    }
}