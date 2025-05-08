using HarmonyLib;
using REPOWildCardMod.Extensions;
using REPOWildCardMod.Items;
namespace REPOWildCardMod.Patches
{
    [HarmonyPatch(typeof(EnemyOnScreen))]
    public static class EnemyOnScreenPatches
    {
        [HarmonyPatch(nameof(EnemyOnScreen.GetOnScreen))]
        [HarmonyPrefix]
        public static bool InfectedBlindness(EnemyOnScreen __instance, ref bool __result)
        {
            if (__instance.Enemy.EnemyParent.WormData().infected)
            {
                __result = false;
                return false;
            }
            return true;
        }
    }
}