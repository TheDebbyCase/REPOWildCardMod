using HarmonyLib;
using REPOWildCardMod.Valuables;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using UnityEngine;
namespace REPOWildCardMod.Patches
{
    [HarmonyPatch(typeof(PhysGrabber))]
    public static class PhysGrabberRayCheckPatch
    {
        static readonly BepInEx.Logging.ManualLogSource log = WildCardMod.instance.log;
        [HarmonyPatch(nameof(PhysGrabber.RayCheck))]
        [HarmonyPrefix]
        public static bool GiwiRayCheck(PhysGrabber __instance, ref bool _grab)
        {
            if (__instance.playerAvatar.isDisabled || __instance.playerAvatar.isTumbling || __instance.playerAvatar.deadSet)
            {
                return false;
            }
            float maxDistance = 10f;
            if (_grab)
            {
                __instance.grabDisableTimer = 0.1f;
            }
            Vector3 direction = __instance.playerCamera.transform.forward;
            if (__instance.overrideGrab && (bool)__instance.overrideGrabTarget)
            {
                direction = (__instance.overrideGrabTarget.transform.position - __instance.playerCamera.transform.position).normalized;
            }
            if (!_grab)
            {
                RaycastHit[] array = Physics.SphereCastAll(__instance.playerCamera.transform.position, 1f, direction, maxDistance, __instance.mask, QueryTriggerInteraction.Collide);
                for (int i = 0; i < array.Length; i++)
                {
                    RaycastHit raycastHit = array[i];
                    ValuableObject component = raycastHit.transform.GetComponent<ValuableObject>();
                    if (!component)
                    {
                        continue;
                    }
                    if (!component.discovered)
                    {
                        Vector3 direction2 = __instance.playerCamera.transform.position - raycastHit.point;
                        RaycastHit[] array2 = Physics.SphereCastAll(raycastHit.point, 0.01f, direction2, direction2.magnitude, __instance.mask, QueryTriggerInteraction.Collide);
                        bool flag = true;
                        RaycastHit[] array3 = array2;
                        for (int j = 0; j < array3.Length; j++)
                        {
                            RaycastHit raycastHit2 = array3[j];
                            if (!raycastHit2.transform.CompareTag("Player") && raycastHit2.transform != raycastHit.transform)
                            {
                                flag = false;
                                break;
                            }
                        }
                        if (flag)
                        {
                            component.Discover(ValuableDiscoverGraphic.State.Discover);
                        }
                    }
                    else
                    {
                        if (!component.discoveredReminder)
                        {
                            continue;
                        }
                        Vector3 direction3 = __instance.playerCamera.transform.position - raycastHit.point;
                        RaycastHit[] array4 = Physics.RaycastAll(raycastHit.point, direction3, direction3.magnitude, __instance.mask, QueryTriggerInteraction.Collide);
                        bool flag2 = true;
                        RaycastHit[] array3 = array4;
                        foreach (RaycastHit raycastHit3 in array3)
                        {
                            if (raycastHit3.collider.transform.CompareTag("Wall"))
                            {
                                flag2 = false;
                                break;
                            }
                        }
                        if (flag2)
                        {
                            component.discoveredReminder = false;
                            component.Discover(ValuableDiscoverGraphic.State.Reminder);
                        }
                    }
                }
            }
            if (!Physics.Raycast(__instance.playerCamera.transform.position, direction, out var hitInfo, maxDistance, __instance.mask, QueryTriggerInteraction.Ignore))
            {
                return false;
            }
            Transform hitTransform = hitInfo.transform;
            Collider hitCollider = hitInfo.collider;
            Rigidbody hitRigidbody = hitInfo.rigidbody;
            Vector3 hitPoint = hitInfo.point;
            GiwiWormValuable giwi = hitInfo.transform.GetComponentInParent<GiwiWormValuable>();
            if (giwi != null)
            {
                hitTransform = giwi.transform;
                hitCollider = giwi.transform.GetComponentInChildren<Collider>();
                hitRigidbody = giwi.physGrabObject.rb;
                hitPoint = giwi.physGrabObject.forceGrabPoint.position;
            }
            bool flag3 = false;
            flag3 = __instance.overrideGrab && !__instance.overrideGrabTarget;
            flag3 = __instance.overrideGrab && (bool)__instance.overrideGrabTarget && hitTransform.GetComponentInParent<PhysGrabObject>() == __instance.overrideGrabTarget;
            if (!__instance.overrideGrab)
            {
                flag3 = true;
            }
            if (!(hitCollider.CompareTag("Phys Grab Object") && flag3) || hitInfo.distance > __instance.grabRange)
            {
                return false;
            }
            if (_grab)
            {
                __instance.grabbedPhysGrabObject = hitTransform.GetComponentInParent<PhysGrabObject>();
                if ((bool)__instance.grabbedPhysGrabObject && __instance.grabbedPhysGrabObject.grabDisableTimer > 0f)
                {
                    return false;
                }
                if ((bool)__instance.grabbedPhysGrabObject && __instance.grabbedPhysGrabObject.rb.IsSleeping())
                {
                    __instance.grabbedPhysGrabObject.OverrideIndestructible(0.5f);
                    __instance.grabbedPhysGrabObject.OverrideBreakEffects(0.5f);
                }
                __instance.grabbedObjectTransform = hitTransform;
                if ((bool)__instance.grabbedPhysGrabObject)
                {
                    PhysGrabObjectCollider component2 = hitCollider.GetComponent<PhysGrabObjectCollider>();
                    __instance.grabbedPhysGrabObjectCollider = hitCollider;
                    __instance.grabbedPhysGrabObjectColliderID = component2.colliderID;
                    __instance.grabbedStaticGrabObject = null;
                }
                else
                {
                    __instance.grabbedPhysGrabObject = null;
                    __instance.grabbedPhysGrabObjectCollider = null;
                    __instance.grabbedPhysGrabObjectColliderID = 0;
                    __instance.grabbedStaticGrabObject = __instance.grabbedObjectTransform.GetComponentInParent<StaticGrabObject>();
                    if (!__instance.grabbedStaticGrabObject)
                    {
                        StaticGrabObject[] componentsInParent = __instance.grabbedObjectTransform.GetComponentsInParent<StaticGrabObject>();
                        foreach (StaticGrabObject staticGrabObject in componentsInParent)
                        {
                            if (staticGrabObject.colliderTransform == hitCollider.transform)
                            {
                                __instance.grabbedStaticGrabObject = staticGrabObject;
                            }
                        }
                    }
                    if (!__instance.grabbedStaticGrabObject || !__instance.grabbedStaticGrabObject.enabled)
                    {
                        return false;
                    }
                }
                __instance.PhysGrabPointActivate();
                __instance.physGrabPointPuller.gameObject.SetActive(value: true);
                __instance.grabbedObject = hitRigidbody;
                Vector3 vector = hitPoint;
                if ((bool)__instance.grabbedPhysGrabObject && __instance.grabbedPhysGrabObject.roomVolumeCheck.currentSize.magnitude < 0.5f)
                {
                    vector = hitCollider.bounds.center;
                }
                float num = Vector3.Distance(__instance.playerCamera.transform.position, vector);
                Vector3 position = __instance.playerCamera.transform.position + __instance.playerCamera.transform.forward * num;
                __instance.physGrabPointPlane.position = position;
                __instance.physGrabPointPuller.position = position;
                if (__instance.physRotatingTimer <= 0f)
                {
                    __instance.cameraRelativeGrabbedForward = Camera.main.transform.InverseTransformDirection(__instance.grabbedObjectTransform.forward);
                    __instance.cameraRelativeGrabbedUp = Camera.main.transform.InverseTransformDirection(__instance.grabbedObjectTransform.up);
                    __instance.cameraRelativeGrabbedRight = Camera.main.transform.InverseTransformDirection(__instance.grabbedObjectTransform.right);
                }
                if (GameManager.instance.gameMode == 0)
                {
                    __instance.physGrabPoint.position = vector;
                    if (!__instance.grabbedPhysGrabObject || !__instance.grabbedPhysGrabObject.forceGrabPoint)
                    {
                        __instance.localGrabPosition = __instance.grabbedObjectTransform.InverseTransformPoint(vector);
                    }
                    else
                    {
                        vector = __instance.grabbedPhysGrabObject.forceGrabPoint.position;
                        num = 1f;
                        position = __instance.playerCamera.transform.position + __instance.playerCamera.transform.forward * num - __instance.playerCamera.transform.up * 0.3f;
                        __instance.physGrabPoint.position = vector;
                        __instance.physGrabPointPlane.position = position;
                        __instance.physGrabPointPuller.position = position;
                        __instance.localGrabPosition = __instance.grabbedObjectTransform.InverseTransformPoint(vector);
                    }
                }
                else if ((bool)__instance.grabbedPhysGrabObject)
                {
                    if ((bool)__instance.grabbedPhysGrabObject.forceGrabPoint)
                    {
                        vector = __instance.grabbedPhysGrabObject.forceGrabPoint.position;
                        Quaternion quaternion = Quaternion.Euler(45f, 0f, 0f);
                        __instance.cameraRelativeGrabbedForward = quaternion * Vector3.forward;
                        __instance.cameraRelativeGrabbedUp = quaternion * Vector3.up;
                        __instance.cameraRelativeGrabbedRight = quaternion * Vector3.right;
                        num = 1f;
                        position = __instance.playerCamera.transform.position + __instance.playerCamera.transform.forward * num - __instance.playerCamera.transform.up * 0.3f;
                        if (!__instance.overrideGrabPointTransform)
                        {
                            __instance.physGrabPoint.position = vector;
                        }
                        else
                        {
                            __instance.physGrabPoint.position = __instance.overrideGrabPointTransform.position;
                        }
                        __instance.physGrabPointPlane.position = position;
                        __instance.physGrabPointPuller.position = position;
                    }
                    __instance.grabbedPhysGrabObject.GrabLink(__instance.photonView.ViewID, __instance.grabbedPhysGrabObjectColliderID, vector, __instance.cameraRelativeGrabbedForward, __instance.cameraRelativeGrabbedUp);
                }
                else if ((bool)__instance.grabbedStaticGrabObject)
                {
                    __instance.grabbedStaticGrabObject.GrabLink(__instance.photonView.ViewID, vector);
                }
                if (__instance.isLocal)
                {
                    PlayerController.instance.physGrabObject = __instance.grabbedObjectTransform.gameObject;
                    PlayerController.instance.physGrabActive = true;
                }
                __instance.initialPressTimer = 0.1f;
                __instance.prevGrabbed = __instance.grabbed;
                __instance.grabbed = true;
            }
            if (!__instance.grabbed)
            {
                bool flag4 = false;
                PhysGrabObject physGrabObject = hitTransform.GetComponent<PhysGrabObject>();
                if (!physGrabObject)
                {
                    physGrabObject = hitTransform.GetComponentInParent<PhysGrabObject>();
                }
                if ((bool)physGrabObject)
                {
                    __instance.currentlyLookingAtPhysGrabObject = physGrabObject;
                    flag4 = true;
                }
                StaticGrabObject staticGrabObject2 = hitTransform.GetComponent<StaticGrabObject>();
                if (!staticGrabObject2)
                {
                    staticGrabObject2 = hitTransform.GetComponentInParent<StaticGrabObject>();
                }
                if ((bool)staticGrabObject2 && staticGrabObject2.enabled)
                {
                    __instance.currentlyLookingAtStaticGrabObject = staticGrabObject2;
                    flag4 = true;
                }
                ItemAttributes component3 = hitTransform.GetComponentInParent<ItemAttributes>();
                if ((bool)component3)
                {
                    __instance.currentlyLookingAtItemAttributes = component3;
                    component3.ShowInfo();
                }
                if (flag4)
                {
                    Aim.instance.SetState(Aim.State.Grabbable);
                }
            }
            return false;
        }
    }
    [HarmonyPatch(typeof(PhysGrabber))]
    public static class PhysGrabberPhysGrabPointActivatePatch
    {
        static readonly BepInEx.Logging.ManualLogSource log = WildCardMod.instance.log;
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