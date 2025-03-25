using REPOWildCardMod.Items;
using HarmonyLib;
namespace REPOWildCardMod.Patches
{
    [HarmonyPatch(typeof(PlayerAvatar))]
    public static class PlayerAvatarPatch
    {
        static readonly BepInEx.Logging.ManualLogSource log = WildCardMod.log;
        [HarmonyPatch(nameof(PlayerAvatar.ChatMessageSend))]
        [HarmonyPostfix]
        public static void CheckForKill(PlayerAvatar __instance, ref string _message)
        {
            if (__instance.physGrabber != null && __instance.physGrabber.grabbedPhysGrabObject != null && __instance.physGrabber.grabbedPhysGrabObject.transform.TryGetComponent<SmithNote>(out SmithNote smithNote) && smithNote.itemToggle.toggleState && smithNote.charged && !smithNote.playersDead[__instance.playerName])
            {
                smithNote.KillPlayer(_message);
            }
        }
    }
}