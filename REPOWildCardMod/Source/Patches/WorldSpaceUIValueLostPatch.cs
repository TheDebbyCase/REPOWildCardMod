using HarmonyLib;
using UnityEngine;
namespace REPOWildCardMod.Patches
{
    [HarmonyPatch(typeof(WorldSpaceUIValueLost))]
    public static class WorldSpaceUIValueLostPatches
    {
        [HarmonyPatch(nameof(WorldSpaceUIValueLost.Start))]
        [HarmonyPostfix]
        public static void PositiveValue(WorldSpaceUIValueLost __instance)
        {
            if (__instance.value < 0)
            {
                __instance.textColor = Color.green;
                __instance.text.text = __instance.text.text.Replace("-", "");
                if (__instance.value < 1000)
                {
                    __instance.scale /= 0.75f;
                    __instance.transform.localScale = __instance.scale;
                }
            }
        }
    }
}