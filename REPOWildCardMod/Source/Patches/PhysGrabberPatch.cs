using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using UnityEngine;
namespace REPOWildCardMod.Patches
{
    [HarmonyPatch(typeof(PhysGrabber))]
    public class PhysGrabberRayCheckPatch
    {
        static readonly BepInEx.Logging.ManualLogSource log = WildCardMod.log;
        [HarmonyPatch(nameof(PhysGrabber.RayCheck))]
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            List<CodeInstruction> codes = new List<CodeInstruction>(instructions);
            log.LogDebug($"Harmony Patching {nameof(PhysGrabber.RayCheck)}");
            for (int i = 0; i < codes.Count; i++)
            {
                if (codes[i].ToString() == "callvirt PhysGrabObject UnityEngine.Component::GetComponent()" && codes[i + 1].ToString() == "stfld PhysGrabObject PhysGrabber::grabbedPhysGrabObject")
                {
                    CodeInstruction instruction = new CodeInstruction(OpCodes.Callvirt, AccessTools.Method(typeof(Component), nameof(Component.GetComponentInParent), new Type[] { }, new Type[] { typeof(PhysGrabObject) }));
                    log.LogDebug($"Patching IL line {i}: \"{codes[i]}\" in \"{nameof(PhysGrabber)}::{nameof(PhysGrabber.RayCheck)}\", replacing with \"{instruction}\"");
                    codes[i] = instruction;
                    break;
                }
            }
            return codes.AsEnumerable();
        }
    }
    [HarmonyPatch(typeof(PhysGrabber))]
    public class PhysGrabberPatchPhysGrabPointActivate
    {
        static readonly BepInEx.Logging.ManualLogSource log = WildCardMod.log;
        [HarmonyPatch(nameof(PhysGrabber.PhysGrabPointActivate))]
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            List<CodeInstruction> codes = new List<CodeInstruction>(instructions);
            log.LogDebug($"Harmony Patching {nameof(PhysGrabber.PhysGrabPointActivate)}");
            for (int i = 0; i < codes.Count; i++)
            {
                if (codes[i].ToString() == "callvirt PhysGrabObject UnityEngine.Component::GetComponent()" && codes[i + 1].ToString() == "stfld PhysGrabObject PhysGrabber::grabbedPhysGrabObject")
                {
                    CodeInstruction instruction = new CodeInstruction(OpCodes.Callvirt, AccessTools.Method(typeof(Component), nameof(Component.GetComponentInParent), new Type[] { }, new Type[] { typeof(PhysGrabObject) }));
                    log.LogDebug($"Patching IL line {i}: \"{codes[i]}\" in \"{nameof(PhysGrabber)}::{nameof(PhysGrabber.PhysGrabPointActivate)}\", replacing with \"{instruction}\"");
                    codes[i] = instruction;
                    break;
                }
            }
            return codes.AsEnumerable();
        }
    }
}