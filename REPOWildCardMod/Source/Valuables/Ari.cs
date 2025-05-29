using Photon.Pun;
using REPOWildCardMod.Utils;
using UnityEngine;
namespace REPOWildCardMod.Valuables
{
    public class Ari : MonoBehaviour
    {
        readonly BepInEx.Logging.ManualLogSource log = WildCardMod.instance.log;
        readonly WildCardUtils utils = WildCardMod.instance.utils;
        public PhotonView photonView;
        public PhysGrabObject physGrabObject;
        public Animator animator;
        public PhysicMaterial physMat;
        public Sound ariSounds;
        public Sound flapLoop;
        public float balanceForce = 4f;
        public float floatPower = 5f;
        public bool dropped;
        public float chirpTimer;
        public void Start()
        {
            physGrabObject.OverrideMaterial(physMat, -123f);
        }
        public void FixedUpdate()
        {
            if (!LevelGenerator.Instance.Generated)
            {
                return;
            }
            if (SemiFunc.IsMasterClientOrSingleplayer())
            {
                Quaternion rotator = Quaternion.FromToRotation(transform.up, Vector3.up);
                if (physGrabObject.grabbed)
                {
                    if (dropped)
                    {
                        SetDropped(false);
                    }
                    physGrabObject.rb.AddForce((Random.insideUnitSphere / 2f) + (transform.up / 1.3f), ForceMode.Impulse);
                }
                else if (!Physics.Raycast(physGrabObject.rb.worldCenterOfMass, -transform.up, 0.5f, LayerMask.GetMask("Default", "PhysGrabObject", "PhysGrabObjectCart", "PhysGrabObjectHinge", "Enemy", "Player"), QueryTriggerInteraction.Ignore))
                {
                    if (!dropped)
                    {
                        SetDropped(true);
                    }
                    physGrabObject.rb.AddForce(transform.up * floatPower * (1.1f - (Quaternion.Angle(Quaternion.identity, rotator) / 360f)));
                    physGrabObject.rb.AddTorque(new Vector3(rotator.x, rotator.y, rotator.z) * balanceForce);
                }
                else if (dropped)
                {
                    SetDropped(false);
                }
            }
        }
        public void Update()
        {
            if (!LevelGenerator.Instance.Generated)
            {
                return;
            }
            flapLoop.PlayLoop(animator.GetBool("Grabbed"), 1f, 1f);
            if (physGrabObject.grabbed || dropped)
            {
                if (!animator.GetBool("Grabbed"))
                {
                    animator.SetBool("Grabbed", true);
                }
            }
            else
            {
                if (animator.GetBool("Grabbed"))
                {
                    animator.SetBool("Grabbed", false);
                }
            }
            if (!ariSounds.Source.isPlaying && chirpTimer <= 0f)
            {
                if (physGrabObject.grabbed)
                {
                    EnemyDirector.instance.SetInvestigate(transform.position, 15f);
                    log.LogDebug("Ari Chirp Alert!");
                }
                ariSounds.Play(physGrabObject.rb.worldCenterOfMass);
                animator.SetTrigger("Chirp");
                chirpTimer = (Random.value + 1f) * 2f;
            }
            else if (chirpTimer > 0f)
            {
                chirpTimer -= Time.deltaTime * (utils.BoolToInt(physGrabObject.grabbed) + 1);
            }
        }
        public void ImpactSquish()
        {
            if (SemiFunc.IsMasterClientOrSingleplayer())
            {
                float force = physGrabObject.impactDetector.impactForce;
                if (GameManager.Multiplayer())
                {
                    photonView.RPC("SquishRPC", RpcTarget.All, force);
                }
                else
                {
                    SquishRPC(force);
                }
            }
        }
        [PunRPC]
        public void SquishRPC(float force)
        {
            animator.SetLayerWeight(1, Mathf.Clamp01(force / 150f));
            animator.SetTrigger("Squish");
        }
        public void SetDropped(bool drop)
        {
            if (GameManager.Multiplayer())
            {
                photonView.RPC("SetDroppedRPC", RpcTarget.All, drop);
            }
            else
            {
                SetDroppedRPC(drop);
            }
        }
        [PunRPC]
        public void SetDroppedRPC(bool drop)
        {
            dropped = drop;
        }
    }
}