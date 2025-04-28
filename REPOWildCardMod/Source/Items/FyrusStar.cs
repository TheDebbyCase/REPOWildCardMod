using Photon.Pun;
using UnityEngine;
namespace REPOWildCardMod.Items
{
    public class FyrusStar : MonoBehaviour
    {
        readonly BepInEx.Logging.ManualLogSource log = WildCardMod.instance.log;
        public PhysGrabObject physGrabObject;
        public ItemEquippable itemEquippable;
        public Transform rayStart;
        public Collider trigger;
        public Transform playerContainer;
        public Collider[] containerColliders;
        public bool insideLocal;
        public float balancePower = 75f;
        public Vector3 lookDirection = Vector3.zero;
        public PlayerAvatar ridingPlayer = null;
        public float steerPower = 20f;
        public float steerRamp = 1f;
        public float rampMax = 10f;
        public float currentSteerPower;
        public float floatPower = 40f;
        public float floatHeight = 0.6f;
        public float lookBalancePower = 0.2f;
        public float steerSlowTimer;
        public Vector3 lastLookDirection;
        public float lookDifference = 1.5f;
        public void FixedUpdate()
        {
            if (SemiFunc.IsMasterClientOrSingleplayer() && !itemEquippable.isEquipped && !itemEquippable.isEquipping && !itemEquippable.isUnequipping)
            {
                Vector3 balanceDirection = Vector3.up;
                Vector3 currentDirection = Vector3.zero;
                if (ridingPlayer != null)
                {
                    currentDirection = lookDirection;
                    balanceDirection += new Vector3(currentDirection.x, 0f, currentDirection.z) * lookBalancePower;
                    if (Vector3.Distance(currentDirection, lastLookDirection) <= lookDifference)
                    {
                        steerRamp += Time.fixedDeltaTime * 2f;
                        if (steerRamp > rampMax)
                        {
                            steerRamp = rampMax;
                        }
                        currentSteerPower = steerPower * steerRamp;
                    }
                    else
                    {
                        currentSteerPower = steerPower;
                        steerRamp = 1f;
                    }
                }
                Quaternion rotator = Quaternion.FromToRotation(transform.up, balanceDirection);
                physGrabObject.rb.AddTorque(new Vector3(rotator.x, rotator.y, rotator.z) * balancePower);
                if (Physics.Raycast(rayStart.position, Vector3.down, out RaycastHit hit, floatHeight, LayerMask.GetMask("Default", "PhysGrabObject", "PhysGrabObjectCart", "PhysGrabObjectHinge", "Enemy", "Player"), QueryTriggerInteraction.Ignore))
                {
                    physGrabObject.rb.AddForce(new Vector3(currentDirection.x * currentSteerPower, floatPower / hit.distance, currentDirection.z * currentSteerPower));
                }
                if (steerSlowTimer <= 0f)
                {
                    steerSlowTimer = 0.5f;
                    lastLookDirection = lookDirection;
                }
                steerSlowTimer -= Time.fixedDeltaTime;
            }
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
            if (trigger.bounds.Intersects(SemiFunc.PlayerAvatarLocal().collider.bounds))
            {
                if (!insideLocal)
                {
                    log.LogDebug($"Driving Fyrus Star!");
                    insideLocal = true;
                    if (SemiFunc.IsMultiplayer())
                    {
                        physGrabObject.photonView.RPC("SetOverlapRPC", RpcTarget.MasterClient, SemiFunc.PhotonViewIDPlayerAvatarLocal());
                    }
                    else
                    {
                        SetOverlapRPC(SemiFunc.PhotonViewIDPlayerAvatarLocal());
                    }
                }
            }
            else if (insideLocal)
            {
                log.LogDebug($"Left Fyrus Star...");
                insideLocal = false;
                if (SemiFunc.IsMultiplayer())
                {
                    physGrabObject.photonView.RPC("SetOverlapRPC", RpcTarget.MasterClient, -1);
                }
                else
                {
                    SetOverlapRPC(-1);
                }
            }
            if (insideLocal)
            {
                if (physGrabObject.grabbedLocal)
                {
                    PhysGrabber.instance.OverrideDisableRotationControls();
                }
                Vector3 cameraDirection = (PlayerController.instance.transform.rotation * Vector3.forward).normalized;
                if (SemiFunc.IsMultiplayer())
                {
                    physGrabObject.photonView.RPC("UpdateLookDirectionRPC", RpcTarget.MasterClient, cameraDirection);
                }
                else
                {
                    UpdateLookDirectionRPC(cameraDirection);
                }
            }
        }
        [PunRPC]
        public void SetOverlapRPC(int photonID)
        {
            if (photonID == -1)
            {
                ridingPlayer = null;
                log.LogDebug($"Fyrus Star has no pilot!");
            }
            else if (ridingPlayer == null)
            {
                ridingPlayer = SemiFunc.PlayerAvatarGetFromPhotonID(photonID);
                log.LogDebug($"Fyrus Star Pilot: \"{ridingPlayer.playerName}\"");
            }
            if (playerContainer.gameObject.activeSelf != (photonID != -1))
            {
                EnableContainer(photonID != -1);
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
        [PunRPC]
        public void EnableContainerRPC(bool enable)
        {
            playerContainer.gameObject.SetActive(enable);
        }
        [PunRPC]
        public void UpdateLookDirectionRPC(Vector3 vector)
        {
            lookDirection = vector;
        }
    }
}