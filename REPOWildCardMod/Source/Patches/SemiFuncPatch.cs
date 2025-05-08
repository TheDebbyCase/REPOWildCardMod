using HarmonyLib;
using REPOWildCardMod.Extensions;
namespace REPOWildCardMod.Patches
{
    [HarmonyPatch(typeof(SemiFunc))]
    public static class SemiFuncPatches
    {
        [HarmonyPatch(nameof(SemiFunc.OnSceneSwitch))]
        [HarmonyPostfix]
        public static void ClearInfectedList()
        {
            if (EnemyParentExtension.wormDataDictionary.Count > 0)
            {
                EnemyParentExtension.wormDataDictionary.Clear();
            }
        }
    }
}