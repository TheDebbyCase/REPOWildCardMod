using Photon.Pun;
using REPOWildCardMod.Utils;
using UnityEngine;
namespace REPOWildCardMod.Valuables
{
    public class AlolanVulpixie : MonoBehaviour
    {
        readonly BepInEx.Logging.ManualLogSource log = WildCardMod.instance.log;
        readonly WildCardUtils utils = WildCardMod.instance.utils;
        public PhotonView photonView;
        public PhysGrabObject physGrabObject;
        public GameObject[] pixieMeshes;
        public Sound[] pixieSounds;
        public PhysicMaterial physMat;
        public Animator animator;
        public bool scrungle = false;
        public float voiceTimer;
        public void Start()
        {
            physGrabObject.OverrideMaterial(physMat, -123f);
        }
        public void Update()
        {
            if (physGrabObject.grabbed)
            {
                if (voiceTimer > 0f)
                {
                    voiceTimer -= Time.deltaTime;
                }
                else
                {
                    voiceTimer = Random.Range(0.5f, 2.5f);
                    int index = utils.BoolToInt(scrungle);
                    pixieSounds[index].Play(pixieSounds[index].Source.transform.position);
                }
            }
        }
        public void PixieImpact(bool sad)
        {
            int index = utils.BoolToInt(sad);
            scrungle = !scrungle;
            pixieMeshes[1].SetActive(scrungle);
            pixieMeshes[0].SetActive(!scrungle);
            pixieSounds[index].Play(pixieSounds[index].Source.transform.position);
            log.LogDebug($"Vulpixie Scrungle: {scrungle}, Vulpixie Sadge: {sad}");
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