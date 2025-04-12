using Photon.Pun;
using UnityEngine;
namespace REPOWildCardMod.Valuables
{
    public class CloverBee : MonoBehaviour
    {
        public PhotonView photonView;
        public PhysGrabObject physGrabObject;
        public Texture2D[] eyeTextures;
        public MeshRenderer meshRenderer;
        public Sound angryBees;
        public Sound happyBees;
        public Animator animator;
        public float blinkTimer;
        public float unblinkTimer;
        public float balanceForce = 2.5f;
        public float angerTimer;
        public float playerDamageTimer;
        public void FixedUpdate()
        {
            if (SemiFunc.IsMasterClientOrSingleplayer())
            {
                if (physGrabObject.grabbed)
                {
                    physGrabObject.rb.AddForce(transform.up, ForceMode.Impulse);
                }
                else
                {
                    Quaternion rotator = Quaternion.FromToRotation(transform.up, Vector3.up);
                    physGrabObject.rb.AddTorque(new Vector3(rotator.x, rotator.y, rotator.z) * balanceForce);
                }
            }
        }
        public void Update()
        {
            angryBees.PlayLoop(angerTimer > 0f, 2f, 1f);
            happyBees.PlayLoop(angerTimer <= 0f, 2f, 1f);
            if (physGrabObject.breakHeavyTimer >= 0f)
            {
                if (animator.GetBool("Wings"))
                {
                    animator.SetBool("Wings", false);
                }
            }
            else
            {
                if (!animator.GetBool("Wings"))
                {
                    animator.SetBool("Wings", true);
                }
            }
            if (blinkTimer > 0f)
            {
                blinkTimer -= Time.deltaTime;
            }
            else
            {
                blinkTimer = Random.Range(0.15f, 1f);
                if (meshRenderer.materials[1].mainTexture == eyeTextures[0])
                {
                    meshRenderer.materials[1].mainTexture = eyeTextures[1];
                    unblinkTimer = 0.25f;
                }
            }
            if (unblinkTimer > 0f)
            {
                unblinkTimer -= Time.deltaTime;
            }
            else if (meshRenderer.materials[1].mainTexture == eyeTextures[1])
            {
                meshRenderer.materials[1].mainTexture = eyeTextures[0];
            }
            if (angerTimer > 0f)
            {
                if (physGrabObject.grabbedLocal && playerDamageTimer <= 0f)
                {
                    PlayerAvatar.instance.playerHealth.Hurt(1, true);
                    playerDamageTimer = 0.2f;
                }
                else if (playerDamageTimer > 0f)
                {
                    playerDamageTimer -= Time.deltaTime;
                }
                angerTimer -= Time.deltaTime;
            }
            else if (meshRenderer.materials[1].mainTexture == eyeTextures[2])
            {
                meshRenderer.materials[1].mainTexture = eyeTextures[0];
            }
        }
        public void OnBreak()
        {
            meshRenderer.materials[1].mainTexture = eyeTextures[2];
            angerTimer = Random.Range(0.25f, 1f);
        }
    }
}