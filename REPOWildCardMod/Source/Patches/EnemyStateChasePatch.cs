using HarmonyLib;
using REPOWildCardMod.Extensions;
namespace REPOWildCardMod.Patches
{
    [HarmonyPatch(typeof(EnemyStateChase))]
    public static class EnemyStateChasePatches
    {
        [HarmonyPatch(nameof(EnemyStateChase.Update))]
        [HarmonyPostfix]
        public static void WormDisableChase(EnemyStateChase __instance)
        {
            if (__instance.Enemy.EnemyParent.WormData().infected)
            {
                __instance.Enemy.Vision.VisionsTriggered[__instance.Enemy.TargetPlayerAvatar.photonView.ViewID] = 0;
                __instance.Enemy.CurrentState = EnemyState.Roaming;
            }
        }
    }
}