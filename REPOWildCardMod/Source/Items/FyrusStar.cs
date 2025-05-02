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
        public HurtCollider hurtCollider;
        public int floatMask;
        public bool insideLocal;
        public float balancePower = 75f;
        public Vector3 lookDirection = Vector3.zero;
        public PlayerAvatar ridingPlayer = null;
        public PlayerAvatar lastRider = null;
        public float steerPower = 20f;
        public float steerRamp = 1f;
        public float rampMax = 10f;
        public float currentSteerPower = 20f;
        public float floatPower = 40f;
        public float floatHeight = 0.6f;
        public float lookBalancePower = 0.2f;
        public float physCheckTimer;
        public Vector3 lastLookDirection;
        public bool lastGrounded;
        public float hoverPower = 40f;
        public float hoverPowerCurrent;
        public float hoverTimer;
        public float hoverTargetY;
        public float lookDifference = 1.5f;
        public bool rammingSpeed;
        public float originalPlayerForce;
        public float originalPhysForce;
        public float originalEnemyForce;
        public float lastVelocityMag;
        public void Awake()
        {
            originalPlayerForce = hurtCollider.playerHitForce;
            originalPhysForce = hurtCollider.physHitForce;
            originalEnemyForce = hurtCollider.enemyHitForce;
            floatMask = LayerMask.GetMask("Default", "PhysGrabObject", "PhysGrabObjectCart", "PhysGrabObjectHinge", "Enemy", "Player");
        }
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
                    if (Vector3.Distance(currentDirection, lastLookDirection) <= lookDifference && lastVelocityMag - physGrabObject.rb.velocity.magnitude <= 3f)
                    {
                        steerRamp += Time.fixedDeltaTime * 1.25f;
                        if (steerRamp > rampMax)
                        {
                            steerRamp = rampMax;
                        }
                    }
                    else
                    {
                        if (lastVelocityMag - physGrabObject.rb.velocity.magnitude > 3f)
                        {
                            steerRamp /= 2f;
                            if (steerRamp < 1f)
                            {
                                steerRamp = 1f;
                            }
                        }
                        else
                        {
                            steerRamp = 1f;
                            hurtCollider.playerHitForce = originalPlayerForce;
                            hurtCollider.physHitForce = originalPhysForce;
                            hurtCollider.enemyHitForce = originalEnemyForce;
                        }
                    }
                    Quaternion facingRotator = Quaternion.FromToRotation(transform.forward, currentDirection);
                    physGrabObject.rb.AddTorque(new Vector3(facingRotator.x, facingRotator.y, facingRotator.z) * balancePower);
                }
                else
                {
                    steerRamp = 1f;
                    hurtCollider.playerHitForce = originalPlayerForce;
                    hurtCollider.physHitForce = originalPhysForce;
                    hurtCollider.enemyHitForce = originalEnemyForce;
                }
                float hurtMultiplier = Mathf.InverseLerp(8f, rampMax, steerRamp) + 1f;
                hurtCollider.playerHitForce = originalPlayerForce * hurtMultiplier;
                hurtCollider.physHitForce = originalPhysForce * hurtMultiplier;
                hurtCollider.enemyHitForce = originalEnemyForce * hurtMultiplier;
                currentSteerPower = steerPower * steerRamp;
                Quaternion rotator = Quaternion.FromToRotation(transform.up, balanceDirection);
                physGrabObject.rb.AddTorque(new Vector3(rotator.x, rotator.y, rotator.z) * balancePower);
                bool grounded = false;
                if (Physics.Raycast(rayStart.position, Vector3.down, out RaycastHit hit, floatHeight, floatMask, QueryTriggerInteraction.Ignore))
                {
                    physGrabObject.rb.AddForce(new Vector3(currentDirection.x * currentSteerPower, floatPower / hit.distance, currentDirection.z * currentSteerPower));
                    grounded = true;
                    hoverTimer = 0f;
                }
                else if (hoverTimer > 0f)
                {
                    if (hoverTimer <= 2f)
                    {
                        hoverPowerCurrent = Mathf.Max(0f, hoverPowerCurrent - (Time.fixedDeltaTime * (hoverPower / 4f)));
                    }
                    physGrabObject.rb.AddForce(new Vector3(currentDirection.x * currentSteerPower, hoverPowerCurrent / (rayStart.position.y - hoverTargetY), currentDirection.z * currentSteerPower));
                    hoverTimer -= Time.fixedDeltaTime;
                }
                else if (lastGrounded && !Physics.Raycast(rayStart.position, Vector3.down, floatHeight * 2f, floatMask, QueryTriggerInteraction.Ignore))
                {
                    hoverTimer = 6f;
                    hoverTargetY = rayStart.position.y;
                }
                if (physCheckTimer <= 0f)
                {
                    physCheckTimer = 0.5f;
                    lastLookDirection = lookDirection;
                    lastGrounded = grounded;
                    lastVelocityMag = physGrabObject.rb.velocity.magnitude;
                }
                else
                {
                    physCheckTimer -= Time.fixedDeltaTime;
                }
            }
        }
        public void Update()
        {
            if (SemiFunc.IsMasterClientOrSingleplayer())
            {
                if (rammingSpeed != (steerRamp >= 8f))
                {
                    RammingSpeed(steerRamp >= 8f);
                }
            }
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
                        physGrabObject.photonView.RPC("SetOverlapRPC", RpcTarget.All, SemiFunc.PhotonViewIDPlayerAvatarLocal());
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
                    physGrabObject.photonView.RPC("SetOverlapRPC", RpcTarget.All, -1);
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
            if (rammingSpeed)
            {
                if (!hurtCollider.gameObject.activeSelf)
                {
                    hurtCollider.gameObject.SetActive(true);
                }
            }
            else if (hurtCollider.gameObject.activeSelf)
            {
                hurtCollider.gameObject.SetActive(false);
            }
        }
        public void RammingSpeed(bool ram)
        {
            if (SemiFunc.IsMultiplayer())
            {
                physGrabObject.photonView.RPC("RammingSpeedRPC", RpcTarget.All, ram);
            }
            else
            {
                RammingSpeedRPC(ram);
            }
        }
        [PunRPC]
        public void RammingSpeedRPC(bool ram)
        {
            rammingSpeed = ram;
        }
        [PunRPC]
        public void SetOverlapRPC(int photonID)
        {
            if (photonID == -1)
            {
                ridingPlayer = null;
                trigger.transform.localScale = new Vector3(0.3f, 0.35f, 0.3f);
                log.LogDebug($"Fyrus Star has no pilot!");
            }
            else if (ridingPlayer == null)
            {
                ridingPlayer = SemiFunc.PlayerAvatarGetFromPhotonID(photonID);
                lastRider = ridingPlayer;
                trigger.transform.localScale = new Vector3(0.4f, 0.35f, 0.4f);
                log.LogDebug($"Fyrus Star Pilot: \"{ridingPlayer.playerName}\"");
            }
            if ((playerContainer.gameObject.activeSelf != (photonID != -1)) && SemiFunc.IsMasterClientOrSingleplayer())
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