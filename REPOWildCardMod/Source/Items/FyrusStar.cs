using Photon.Pun;
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
        public Collider trigger;
        public Transform playerContainer;
        public Collider[] containerColliders;
        public bool insideLocal;
        public float[] balancePower = new float[] {50f, 100f};
        public float[] floatHeight = new float[] {0.5f, 1f};
        public float[] floatPower = new float[] {20f, 50f};
        public float[] steerPower = new float[] {25f, 40f};
        public Vector3 steerDirection;
        public float steerTorquePower = 50f;
        public bool notGrabbed;
        public bool lastFrameOverlap;
        public void Start()
        {
            containerColliders = playerContainer.GetComponentsInChildren<Collider>();
        }
        public void Update()
        {
            if (containerColliders[0].tag != "Untagged")
            {
                for (int i = 0; i < containerColliders.Length; i++)
                {
                    containerColliders[i].tag = "Untagged";
                }
            }
            if (insideLocal && physGrabObject.grabbedLocal)
            {
                PhysGrabber.instance.OverrideGrabDistance(2.5f);
                PhysGrabber.instance.OverrideDisableRotationControls();
            }
        }
        public void FixedUpdate()
        {
            if (SemiFunc.IsMasterClientOrSingleplayer() && !itemEquippable.isEquipped && !itemEquippable.isEquipping && !itemEquippable.isUnequipping)
            {
                bool playerOverlap = false;
                Vector3 rayLocalPosition = Vector3.zero;
                PlayerAvatar[] players = SemiFunc.PlayerGetAll().ToArray();
                int insideGrabbers = 0;
                for (int i = 0; i < players.Length; i++)
                {
                    bool isInside = false;
                    if (trigger.bounds.Intersects(players[i].collider.bounds))
                    {
                        playerOverlap = true;
                        isInside = true;
                        if (players[i].isLocal && !insideLocal)
                        {
                            log.LogDebug($"Driving Fyrus Star!");
                            insideLocal = true;
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
                if (playerOverlap != lastFrameOverlap)
                {
                    EnableContainer(playerOverlap);
                }
                lastFrameOverlap = playerOverlap;
                if (playerOverlap)
                {
                    if (physGrabObject.grabbed)
                    {
                        physGrabObject.rb.constraints = RigidbodyConstraints.None;
                        rayLocalPosition /= Mathf.Max(1, insideGrabbers);
                    }
                }
                else
                {
                    physGrabObject.rb.constraints = RigidbodyConstraints.FreezeRotationY;
                    if (insideLocal)
                    {
                        log.LogDebug($"Left Fyrus Star...");
                        insideLocal = false;
                    }
                }
                rayLocalPosition += rayStart.localPosition;
                Vector3 rayPosition = transform.TransformPoint(rayLocalPosition);
                int index = utils.BoolToInt(playerOverlap);
                if (physGrabObject.grabbed)
                {
                    if (playerOverlap)
                    {
                        if (notGrabbed)
                        {
                            steerDirection = rayPosition - (transform.position + new Vector3(0f, 0.5f, 0f));
                            log.LogDebug($"Changing Steer Direction To: \"{steerDirection.normalized}\"");
                            notGrabbed = false;
                        }
                    }
                    else
                    {
                        steerDirection = Vector3.zero;
                    }
                }
                else
                {
                    if (!notGrabbed)
                    {
                        notGrabbed = true;
                    }
                    steerDirection = Vector3.zero;
                }
                Quaternion steerRotator = Quaternion.FromToRotation(Vector3.up, steerDirection.normalized * steerTorquePower);
                Quaternion rotator = Quaternion.FromToRotation(transform.up, Vector3.up);
                physGrabObject.rb.AddTorque((new Vector3(rotator.x, rotator.y, rotator.z) * balancePower[index]) - new Vector3(steerRotator.x, steerRotator.y, steerRotator.z));
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
        public void EnableContainer(bool enable)
        {
            if (SemiFunc.IsMultiplayer())
            {
                physGrabObject.photonView.RPC("EnableContainerRPC", RpcTarget.All, enable);
            }
            else
            {
                EnableContainerRPC(enable);
            }
        }
        public void EnableContainerRPC(bool enable)
        {
            playerContainer.gameObject.SetActive(enable);
        }
    }
}