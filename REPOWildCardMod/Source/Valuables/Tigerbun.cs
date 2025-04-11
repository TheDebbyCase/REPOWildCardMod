using Photon.Pun;
using UnityEngine;
namespace REPOWildCardMod.Valuables
{
    public class Tigerbun : MonoBehaviour
    {
        public PhotonView photonView;
        public PhysGrabObject physGrabObject;
        public ValuableObject valuableObject;
        public Sound tigerSounds;
        public Sound ravenousLoop;
        public Animator animator;
        public void FixedUpdate()
        {
            if (SemiFunc.IsMasterClientOrSingleplayer() && physGrabObject.grabbed && valuableObject.dollarValueCurrent < valuableObject.dollarValueOriginal)
            {
                physGrabObject.rb.AddForce(Random.insideUnitSphere * 3f, ForceMode.Impulse);
                physGrabObject.rb.AddTorque(Random.insideUnitSphere * 7f, ForceMode.Impulse);
            }
        }
        public void OnBreak()
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