using System.Collections.Generic;
using UnityEngine;
namespace REPOWildCardMod.Extensions
{
    public static class PhysGrabObjectExtension
    {
        public static Dictionary<PhysGrabObject, Vector3> physOffsetDictionary = new Dictionary<PhysGrabObject, Vector3>();
        public static void SetUIValueOffset(this PhysGrabObject physGrabObject, Vector3 offset)
        {
            if (physOffsetDictionary.ContainsKey(physGrabObject))
            {
                physOffsetDictionary[physGrabObject] = offset;
            }
            else
            {
                physOffsetDictionary.Add(physGrabObject, offset);
            }
        }
        public static Vector3 GetUIValueOffset(this PhysGrabObject physGrabObject)
        {
            if (physOffsetDictionary.ContainsKey(physGrabObject))
            {
                return physOffsetDictionary[physGrabObject];
            }
            return Vector3.zero;
        }
    }
}