using HarmonyLib;
using REPOWildCardMod.Utils;
namespace REPOWildCardMod.Patches
{
    [HarmonyPatch(typeof(PlayerVoiceChat))]
    public class PlayerVoiceChatPatch
    {
        static readonly BepInEx.Logging.ManualLogSource log = WildCardMod.log;
        static readonly WildCardUtils utils = WildCardMod.utils;
        [HarmonyPatch(nameof(PlayerVoiceChat.Update))]
        [HarmonyPrefix]
        public static bool StopVoice(PlayerVoiceChat __instance)
        {
            if (utils.pauseVoice)
            {
                log.LogDebug($"Pausing {__instance.playerAvatar.playerName}'s voice");
                __instance.clipCheckTimer = 0.001f;
                __instance.clipLoudness = 0f;
            }
            return true;
        }
    }
}