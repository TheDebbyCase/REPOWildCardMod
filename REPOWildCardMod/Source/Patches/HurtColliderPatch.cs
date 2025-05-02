using HarmonyLib;
using REPOWildCardMod.Items;
using UnityEngine;
namespace REPOWildCardMod.Patches
{
    [HarmonyPatch(typeof(HurtCollider))]
    public static class HurtColliderPatch
    {
        static readonly BepInEx.Logging.ManualLogSource log = WildCardMod.instance.log;
        [HarmonyPatch(nameof(HurtCollider.CanHit))]
        [HarmonyPrefix]
        public static bool CheckForNecklaceHolding(HurtCollider __instance, ref GameObject hitObject, ref bool __result)
        {
            if (hitObject.TryGetComponent<PlayerAvatar>(out PlayerAvatar player) && __instance.ignoreObjects.Count > 0)
            {
                if ((__instance.ignoreObjects[0].TryGetComponent<CloverNecklace>(out CloverNecklace necklace) && (player.physGrabber.grabbedPhysGrabObject == necklace || player == necklace.lastHolder)) || __instance.ignoreObjects[0].TryGetComponent<FyrusStar>(out FyrusStar fyrusStar) && fyrusStar.lastRider == player)
                {
                    __result = false;
                    return false;
                }
            }
            return true;
        }
    }
}