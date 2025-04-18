using REPOWildCardMod.Utils;
using UnityEngine;
namespace REPOWildCardMod.Items
{
    public class FyrusStar : MonoBehaviour
    {
        readonly BepInEx.Logging.ManualLogSource log = WildCardMod.instance.log;
        readonly WildCardUtils utils = WildCardMod.instance.utils;
        public PhysGrabObject physGrabObject;
        public ItemEquippable itemEquippable;
        public Transform rayStart;
        public Transform playerContainer;
        public bool insideLocal;
        public float[] balancePower = new float[] {50f, 100f};
        public float[] floatHeight = new float[] {0.5f, 1f};
        public float[] floatPower = new float[] {20f, 50f};
        public float[] steerPower = new float[] {25f, 400f};
        public float grabDistance = 1.5f;
        public bool grabbed = false;
        public void Update()
        {
            if (physGrabObject.grabbed)
            {
                if (!grabbed)
                {
                    log.LogDebug("Grab Started");
                    grabbed = true;
                }
            }
            else
            {
                if (grabbed)
                {
                    log.LogDebug("Grab Ended");
                    grabbed = false;
                }
            }
            if (insideLocal && physGrabObject.grabbedLocal)
            {
                PhysGrabber.instance.OverrideGrabDistance(grabDistance);
                PhysGrabber.instance.OverrideDisableRotationControls();
            }
        }
        public void FixedUpdate()
        {
            if (SemiFunc.IsMasterClientOrSingleplayer() && !itemEquippable.isEquipped && !itemEquippable.isEquipping && !itemEquippable.isUnequipping)
            {
                bool playerOverlap = false;
                Collider[] containerTriggers = playerContainer.GetComponentsInChildren<Collider>();
                Vector3 rayLocalPosition = Vector3.zero;
                PlayerAvatar[] players = SemiFunc.PlayerGetAll().ToArray();
                int insideGrabbers = 0;
                for (int i = 0; i < players.Length; i++)
                {
                    bool isInside = false;
                    for (int j = 0; j < containerTriggers.Length; j++)
                    {
                        if (containerTriggers[j].bounds.Intersects(players[i].collider.bounds))
                        {
                            if (containerTriggers[j].bounds.Intersects(players[i].collider.bounds))
                            {
                                playerOverlap = true;
                                isInside = true;
                                if (players[i].isLocal && !insideLocal)
                                {
                                    insideLocal = true;
                                }
                            }
                            else if (players[i].isLocal && insideLocal)
                            {
                                insideLocal = false;
                            }
                        }
                    }
                    if (isInside)
                    {
                        PhysGrabber playerGrabber = physGrabObject.playerGrabbing.Find((x) => x.playerAvatar == players[i]);
                        if (playerGrabber != null)
                        {
                            rayLocalPosition += new Vector3(transform.InverseTransformPoint(playerGrabber.physGrabPoint.position).x, 0f, transform.InverseTransformPoint(playerGrabber.physGrabPoint.position).z);
                            insideGrabbers++;
                        }
                    }
                }
                if (physGrabObject.grabbed && playerOverlap)
                {
                    physGrabObject.rb.constraints = RigidbodyConstraints.None;
                    rayLocalPosition /= Mathf.Max(1, insideGrabbers);
                }
                else
                {
                    physGrabObject.rb.constraints = RigidbodyConstraints.FreezeRotationY;
                }
                rayLocalPosition += rayStart.localPosition;
                if (!playerOverlap)
                {
                    physGrabObject.rb.constraints = RigidbodyConstraints.FreezeRotationY;
                }
                log.LogDebug($"Local Ray: \"{rayLocalPosition}\"");
                Vector3 rayPosition = transform.TransformPoint(rayLocalPosition);
                log.LogDebug($"World Ray: \"{rayPosition}\"");
                log.LogDebug($"World Ray Start: \"{rayStart.position}\"");
                int index = utils.BoolToInt(playerOverlap);
                Quaternion rotator = Quaternion.FromToRotation(transform.up, Vector3.up);
                physGrabObject.rb.AddTorque(new Vector3(rotator.x, rotator.y, rotator.z) * balancePower[index]);
                if (Physics.Raycast(rayPosition, Vector3.down, out RaycastHit hit, floatHeight[index], LayerMask.GetMask("Default", "PhysGrabObject", "PhysGrabObjectCart", "PhysGrabObjectHinge", "Enemy", "Player"), QueryTriggerInteraction.Ignore))
                {
                    physGrabObject.rb.AddForce(new Vector3(transform.up.x * steerPower[index], floatPower[index] / hit.distance, transform.up.z * steerPower[index]));
                }
                else if (!Physics.Raycast(rayPosition, Vector3.down, 1.25f, LayerMask.GetMask("Default", "PhysGrabObject", "PhysGrabObjectCart", "PhysGrabObjectHinge", "Enemy", "Player"), QueryTriggerInteraction.Ignore))
                {
                    physGrabObject.rb.AddForce(new Vector3(transform.up.x * steerPower[index], 1f, transform.up.z * steerPower[index]));
                }
            }
        }
    }
}