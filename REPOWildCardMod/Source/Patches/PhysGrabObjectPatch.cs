using HarmonyLib;
using Photon.Pun;
using REPOWildCardMod.Valuables;
using UnityEngine;
namespace REPOWildCardMod.Patches
{
    [HarmonyPatch(typeof(PhysGrabObject))]
    public static class PhysGrabObjectPatches
    {
        static readonly BepInEx.Logging.ManualLogSource log = WildCardMod.instance.log;
        [HarmonyPatch(nameof(PhysGrabObject.DestroyPhysGrabObjectRPC))]
        [HarmonyPrefix]
        public static bool ActivateValuableUpgrades(PhysGrabObject __instance)
        {
            if (RoundDirector.instance.dollarHaulList.Contains(__instance.gameObject) && __instance.transform.TryGetComponent<DragonBall>(out DragonBall dragonBall))
            {
                dragonBall.AddPlayerBall();
            }
            if (RoundDirector.instance.dollarHaulList.Contains(__instance.gameObject) && __instance.transform.TryGetComponent<ChaosEmerald>(out ChaosEmerald chaosEmerald))
            {
                chaosEmerald.AddPlayerEmerald();
            }
            return true;
        }
        [HarmonyPatch(nameof(PhysGrabObject.GrabLinkRPC))]
        [HarmonyPrefix]
        public static bool GrabLinkColliders(PhysGrabObject __instance, ref int playerPhotonID, ref int colliderID, ref Vector3 point)
        {
            if (__instance.transform.GetComponent<GiwiWormValuable>())
            {
                log.LogDebug($"Replacing {nameof(PhysGrabObject.GrabLinkRPC)}");
                PhysGrabber component = PhotonView.Find(playerPhotonID).GetComponent<PhysGrabber>();
                component.physGrabPoint.position = point;
                component.grabbedPhysGrabObjectColliderID = colliderID;
                component.grabbedObjectTransform = __instance.FindColliderFromID(component.grabbedPhysGrabObjectColliderID);
                component.localGrabPosition = component.grabbedObjectTransform.InverseTransformPoint(point);
                component.grabbedPhysGrabObjectCollider = component.grabbedObjectTransform.GetComponent<Collider>();
                component.grabbed = true;
                Transform localCameraTransform = component.playerAvatar.localCameraTransform;
                if (__instance.playerGrabbing.Count != 0)
                {
                    component.cameraRelativeGrabbedForward = localCameraTransform.InverseTransformDirection(component.grabbedObjectTransform.forward);
                    component.cameraRelativeGrabbedUp = localCameraTransform.InverseTransformDirection(component.grabbedObjectTransform.up);
                }
                else
                {
                    component.cameraRelativeGrabbedForward = localCameraTransform.InverseTransformDirection(component.grabbedObjectTransform.forward);
                    component.cameraRelativeGrabbedUp = localCameraTransform.InverseTransformDirection(component.grabbedObjectTransform.up);
                    __instance.camRelForward = component.grabbedObjectTransform.InverseTransformDirection(component.grabbedObjectTransform.forward);
                    __instance.camRelUp = component.grabbedObjectTransform.InverseTransformDirection(component.grabbedObjectTransform.up);
                }
                component.cameraRelativeGrabbedForward = component.cameraRelativeGrabbedForward.normalized;
                component.cameraRelativeGrabbedUp = component.cameraRelativeGrabbedUp.normalized;
                if (component.photonView.IsMine)
                {
                    Vector3 localGrabPosition = component.localGrabPosition;
                    __instance.photonView.RPC("GrabPointSyncRPC", RpcTarget.All, playerPhotonID, localGrabPosition);
                }
                return false;
            }
            return true;
        }
    }
}