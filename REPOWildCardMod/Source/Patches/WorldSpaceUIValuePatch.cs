using HarmonyLib;
using REPOWildCardMod.Extensions;
using UnityEngine;
namespace REPOWildCardMod.Patches
{
    [HarmonyPatch(typeof(WorldSpaceUIValue))]
    public static class WorldSpaceUIValuePatches
    {
        [HarmonyPatch(nameof(WorldSpaceUIValue.Show))]
        [HarmonyPrefix]
        public static bool AddOffset(WorldSpaceUIValue __instance, ref PhysGrabObject _grabObject, ref Vector3 _offset)
        {
            if (_offset == Vector3.zero)
            {
                _offset = _grabObject.GetUIValueOffset();
            }
            return true;
        }
    }
}