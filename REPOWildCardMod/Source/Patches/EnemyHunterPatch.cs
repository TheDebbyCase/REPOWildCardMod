using HarmonyLib;
using REPOWildCardMod.Extensions;
namespace REPOWildCardMod.Patches
{
    [HarmonyPatch(typeof(EnemyHunter))]
    public static class EnemyHunterPatches
    {
        [HarmonyPatch(nameof(EnemyHunter.Update))]
        [HarmonyPrefix]
        public static bool WormDisableHuntsman(EnemyHunter __instance)
        {
            if (__instance.enemy.EnemyParent.WormData().infected && __instance.currentState == EnemyHunter.State.Aim)
            {
                __instance.currentState = EnemyHunter.State.Investigate;
                __instance.stateImpulse = false;
            }
            return true;
        }
    }
}