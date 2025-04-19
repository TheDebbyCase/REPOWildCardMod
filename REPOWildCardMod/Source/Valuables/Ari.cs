using Photon.Pun;
using UnityEngine;
namespace REPOWildCardMod.Valuables
{
    public class Ari : MonoBehaviour
    {
        readonly BepInEx.Logging.ManualLogSource log = WildCardMod.instance.log;
        public PhotonView photonView;
        public PhysGrabObject physGrabObject;
        public Animator animator;
        public PhysicMaterial physMat;
        public Sound ariSounds;
        public Sound flapLoop;
        public float chirpTimer;
        public void Start()
        {
            physGrabObject.OverrideMaterial(physMat, -123f);
        }
        public void FixedUpdate()
        {
            if (physGrabObject.grabbed)
            {
                if (SemiFunc.IsMasterClientOrSingleplayer())
                {
                    physGrabObject.rb.AddForce((Random.insideUnitSphere / 2f) + (transform.up / 1.3f), ForceMode.Impulse);
                }

            }
        }
        public void Update()
        {
            flapLoop.PlayLoop(animator.GetBool("Grabbed"), 1f, 1f);
            if (physGrabObject.grabbed)
            {
                if (!animator.GetBool("Grabbed"))
                {
                    animator.SetBool("Grabbed", true);
                }
                if (!ariSounds.Source.isPlaying && chirpTimer <= 0f)
                {
                    EnemyDirector.instance.SetInvestigate(transform.position, 15f);
                    ariSounds.Play(physGrabObject.rb.worldCenterOfMass);
                    animator.SetTrigger("Chirp");
                    log.LogDebug("Ari Chirp!");
                    chirpTimer = (Random.value + 0.25f) * 2f;
                }
                else if (chirpTimer > 0f)
                {
                    chirpTimer -= Time.deltaTime;
                }
            }
            else
            {
                if (animator.GetBool("Grabbed"))
                {
                    animator.SetBool("Grabbed", false);
                }
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
    }
}