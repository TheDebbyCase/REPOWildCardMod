using HarmonyLib;
using UnityEngine;
namespace REPOWildCardMod.Patches
{
    [HarmonyPatch(typeof(CameraGlitch))]
    public static class CameraGlitchPatches
    {
        [HarmonyPatch(nameof(CameraGlitch.Awake))]
        [HarmonyPrefix]
        public static bool InstanceProtection(CameraGlitch __instance)
        {
            if (CameraGlitch.Instance != null)
            {
                Object.Destroy(__instance);
                return false;
            }
            return true;
        }
    }
}