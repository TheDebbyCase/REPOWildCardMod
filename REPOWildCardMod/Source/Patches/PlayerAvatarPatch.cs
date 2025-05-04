using REPOWildCardMod.Items;
using HarmonyLib;
namespace REPOWildCardMod.Patches
{
    [HarmonyPatch(typeof(PlayerAvatar))]
    public static class PlayerAvatarPatches
    {
        [HarmonyPatch(nameof(PlayerAvatar.ChatMessageSend))]
        [HarmonyPostfix]
        public static void CheckForKill(PlayerAvatar __instance, ref string _message)
        {
            if (__instance.physGrabber != null && __instance.physGrabber.grabbedPhysGrabObject != null && __instance.physGrabber.grabbedPhysGrabObject.transform.TryGetComponent<SmithNote>(out SmithNote smithNote) && smithNote.itemToggle.toggleState && SemiFunc.RunIsLevel())
            {
                smithNote.KillMessage(_message);
            }
        }
    }
}