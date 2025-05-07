using HarmonyLib;
using REPOWildCardMod.Items;
namespace REPOWildCardMod.Patches
{
    [HarmonyPatch(typeof(EnemyHunter))]
    public static class EnemyHunterPatches
    {
        [HarmonyPatch(nameof(EnemyHunter.Update))]
        [HarmonyPrefix]
        public static bool WormDisableChase(EnemyHunter __instance)
        {
            WormAttach worm = __instance.enemy.EnemyParent.GetComponentInChildren<WormAttach>();
            if (worm != null && worm.gameObject.activeSelf && __instance.currentState == EnemyHunter.State.Aim)
            {
                __instance.currentState = EnemyHunter.State.Investigate;
                __instance.stateImpulse = false;
            }
            return true;
        }
    }
}