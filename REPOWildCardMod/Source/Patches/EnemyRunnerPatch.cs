using HarmonyLib;
using REPOWildCardMod.Extensions;
namespace REPOWildCardMod.Patches
{
    [HarmonyPatch(typeof(EnemyRunner))]
    public static class EnemyRunnerPatches
    {
        [HarmonyPatch(nameof(EnemyRunner.StateAttackPlayer))]
        [HarmonyPostfix]
        public static void WormStopRunner(EnemyRunner __instance)
        {
            if (__instance.enemy.EnemyParent.WormData().infected)
            {
                __instance.UpdateState(EnemyRunner.State.Idle);
            }
        }
        public static bool WormStopLookUnder(EnemyRunner __instance)
        {
            if (__instance.enemy.EnemyParent.WormData().infected)
            {
                __instance.stateImpulse = false;
                __instance.stateTimer = 0f;
                __instance.UpdateState(EnemyRunner.State.LookUnderStop);
                return false;
            }
            return true;
        }
    }
}