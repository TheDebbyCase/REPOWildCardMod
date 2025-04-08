using HarmonyLib;
using REPOWildCardMod.Valuables;
using System;
namespace REPOWildCardMod.Patches
{
    [HarmonyPatch(typeof(PhysGrabObjectImpactDetector))]
    public static class PhysGrabObjectImpactDetectorPatches
    {
        static readonly BepInEx.Logging.ManualLogSource log = WildCardMod.instance.log;
        [HarmonyPatch(nameof(PhysGrabObjectImpactDetector.FixedUpdate))]
        [HarmonyPrefix]
        public static bool SwitchRigidbodies(PhysGrabObjectImpactDetector __instance)
        {
            if (__instance.physGrabObject.transform.TryGetComponent<GiwiWormValuable>(out GiwiWormValuable giwi))
            {
                int newIndex = Array.FindIndex(giwi.rigidBodies, (x) => x == __instance.rb) + 1;
                if (newIndex == giwi.rigidBodies.Length)
                {
                    newIndex = 0;
                }
                __instance.rb = giwi.rigidBodies[newIndex];
            }
            return true;
        }
    }
}
