using HarmonyLib;
namespace REPOWildCardMod.Patches
{
    [HarmonyPatch(typeof(Sound))]
    public static class SoundPatches
    {
        [HarmonyPatch(nameof(Sound.PlayLoop))]
        [HarmonyPrefix]
        public static bool LoopClipMultiple(Sound __instance, ref bool playing)
        {
            if (__instance.Sounds.Length <= 1)
            {
                return true;
            }
            if (playing)
            {
                if (!__instance.Source.isPlaying)
                {
                    if (__instance.AudioInfoFetched)
                    {
                        __instance.AudioInfoFetched = false;
                    }
                }
                else if (!__instance.AudioInfoFetched)
                {
                    __instance.AudioInfoFetched = true;
                }
            }
            return true;
        }
    }
}