using HarmonyLib;
using REPOWildCardMod.Valuables;
namespace REPOWildCardMod.Patches
{
    [HarmonyPatch(typeof(SpectateCamera))]
    public static class SpectateCameraPatches
    {
        [HarmonyPatch(nameof(SpectateCamera.StopSpectate))]
        [HarmonyPrefix]
        public static bool MoveShenronHUDFrom(SpectateCamera __instance)
        {
            ShenronHUD[] shenrons = __instance.GetComponentsInChildren<ShenronHUD>();
            if (shenrons != null && shenrons.Length > 0)
            {
                for (int i = 0; i < shenrons.Length; i++)
                {
                    shenrons[i].shenronWish.Source.transform.parent = PlayerController.instance.transform;
                    shenrons[i].transform.parent = PlayerController.instance.transform;
                }
            }
            return true;
        }
        [HarmonyPatch(nameof(SpectateCamera.SetDeath))]
        [HarmonyPrefix]
        public static bool MoveShenronHUDTo(SpectateCamera __instance)
        {
            ShenronHUD[] shenrons = PlayerController.instance.transform.GetComponentsInChildren<ShenronHUD>();
            if (shenrons != null && shenrons.Length > 0)
            {
                for (int i = 0; i < shenrons.Length; i++)
                {
                    shenrons[i].shenronWish.Source.transform.parent = __instance.transform;
                    shenrons[i].transform.parent = __instance.transform;
                }
            }
            return true;
        }
    }
}