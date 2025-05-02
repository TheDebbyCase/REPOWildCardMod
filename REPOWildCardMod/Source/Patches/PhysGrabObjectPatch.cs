using HarmonyLib;
using REPOWildCardMod.Valuables;
using UnityEngine;
namespace REPOWildCardMod.Patches
{
    [HarmonyPatch(typeof(PhysGrabObject))]
    public static class PhysGrabObjectPatches
    {
        static readonly BepInEx.Logging.ManualLogSource log = WildCardMod.instance.log;
        [HarmonyPatch(nameof(PhysGrabObject.Awake))]
        [HarmonyPostfix]
        public static void FixTransformFind(PhysGrabObject __instance)
        {
            Transform objectTransform = __instance.transform.Find("Object");
            Transform centerOfMass = __instance.transform.Find("Center of Mass");
            if (objectTransform != null)
            {
                if (__instance.forceGrabPoint == null)
                {
                    __instance.forceGrabPoint = objectTransform.Find("Force Grab Point");
                }
                centerOfMass = objectTransform.Find("Center of Mass");
                if (centerOfMass != null)
                {
                    __instance.rb.centerOfMass = centerOfMass.localPosition + objectTransform.localPosition;
                }
            }
            else if (centerOfMass != null)
            {
                __instance.rb.centerOfMass = centerOfMass.localPosition;
            }
        }
        [HarmonyPatch(nameof(PhysGrabObject.DestroyPhysGrabObjectRPC))]
        [HarmonyPrefix]
        public static bool ActivateValuableUpgrades(PhysGrabObject __instance)
        {
            if (RoundDirector.instance.dollarHaulList.Contains(__instance.gameObject) && __instance.transform.TryGetComponent<DragonBall>(out DragonBall dragonBall))
            {
                dragonBall.AddPlayerBall();
            }
            if (RoundDirector.instance.dollarHaulList.Contains(__instance.gameObject) && __instance.transform.TryGetComponent<ChaosEmerald>(out ChaosEmerald chaosEmerald))
            {
                chaosEmerald.AddPlayerEmerald();
            }
            return true;
        }
    }
}