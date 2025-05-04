using HarmonyLib;
using REPOWildCardMod.Items;
namespace REPOWildCardMod.Patches
{
    [HarmonyPatch(typeof(Enemy))]
    public static class EnemyPatches
    {
        [HarmonyPatch(nameof(Enemy.SetChaseTarget))]
        [HarmonyPrefix]
        public static bool WormDisableChase(Enemy __instance)
        {
            WormAttach worm = __instance.EnemyParent.GetComponentInChildren<WormAttach>();
            if (worm != null && worm.gameObject.activeSelf)
            {
                return false;
            }
            return true;
        }
    }
}