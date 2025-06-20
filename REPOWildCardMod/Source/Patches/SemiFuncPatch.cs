using HarmonyLib;
using REPOWildCardMod.Extensions;
namespace REPOWildCardMod.Patches
{
    [HarmonyPatch(typeof(SemiFunc))]
    public static class SemiFuncPatches
    {
        [HarmonyPatch(nameof(SemiFunc.OnSceneSwitch))]
        [HarmonyPostfix]
        public static void ClearExtensionLists()
        {
            if (EnemyParentExtension.wormDataDictionary.Count > 0)
            {
                EnemyParentExtension.wormDataDictionary.Clear();
            }
            if (PhysGrabObjectExtension.physOffsetDictionary.Count > 0)
            {
                PhysGrabObjectExtension.physOffsetDictionary.Clear();
            }
        }
    }
}