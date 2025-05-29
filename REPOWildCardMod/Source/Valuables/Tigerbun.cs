using Photon.Pun;
using UnityEngine;
using REPOWildCardMod.Utils;
namespace REPOWildCardMod.Valuables
{
    public class Tigerbun : MonoBehaviour
    {
        readonly BepInEx.Logging.ManualLogSource log = WildCardMod.instance.log;
        readonly WildCardUtils utils = WildCardMod.instance.utils;
        public PhotonView photonView;
        public PhysGrabObject physGrabObject;
        public ValuableObject valuableObject;
        public Sound ravenousLoop;
        public Texture2D[] faceTextures;
        public MeshRenderer meshRenderer;
        public Animator animator;
        public void Update()
        {
            if (!LevelGenerator.Instance.Generated)
            {
                return;
            }
            bool angry = physGrabObject.grabbed && valuableObject.dollarValueCurrent <= valuableObject.dollarValueOriginal / 2f;
            ravenousLoop.PlayLoop(angry, 2f, 1f);
            if (meshRenderer.materials[1].mainTexture != faceTextures[utils.BoolToInt(angry)])
            {
                log.LogDebug($"Tigerbun angry: {angry}");
                meshRenderer.materials[1].mainTexture = faceTextures[utils.BoolToInt(angry)];
            }
            if (animator.GetBool("Angry") != angry)
            {
                animator.SetBool("Angry", angry);
            }
        }
        public void FixedUpdate()
        {
            if (SemiFunc.IsMasterClientOrSingleplayer() && physGrabObject.grabbed && valuableObject.dollarValueCurrent <= valuableObject.dollarValueOriginal / 2f)
            {
                physGrabObject.rb.AddForce(Random.insideUnitSphere * 0.5f, ForceMode.Impulse);
                physGrabObject.rb.AddTorque(Random.insideUnitSphere * 2f, ForceMode.Impulse);
            }
        }
        public void OnBreak()
        {
            if (SemiFunc.IsMasterClientOrSingleplayer() && valuableObject.dollarValueCurrent <= valuableObject.dollarValueOriginal / 2f)
            {
                if (physGrabObject.grabbed)
                {
                    for (int i = 0; i < physGrabObject.playerGrabbing.Count; i++)
                    {
                        PlayerAvatar player = physGrabObject.playerGrabbing[i].playerAvatar;
                        player.playerHealth.HurtOther(5, player.transform.position, true);
                    }
                }
                else if (physGrabObject.lastPlayerGrabbing != null)
                {
                    PlayerAvatar player = physGrabObject.lastPlayerGrabbing;
                    player.playerHealth.HurtOther(2, player.transform.position, true);
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