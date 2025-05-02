using Photon.Pun;
using UnityEngine;
namespace REPOWildCardMod.Valuables
{
    public class MooCowValuable : MonoBehaviour
    {
        readonly BepInEx.Logging.ManualLogSource log = WildCardMod.instance.log;
        public PhotonView photonView;
        public PhysGrabObject physGrabObject;
        public Animator animator;
        public PhysicMaterial physMat;
        public Sound cowSounds;
        public float balanceForce = 0.1f;
        public float mooTimer;
        public bool mooTrigger;
        public void Start()
        {
            physGrabObject.OverrideMaterial(physMat, -123f);
        }
        public void FixedUpdate()
        {
            if (SemiFunc.IsMasterClientOrSingleplayer() && physGrabObject.grabbed)
            {
                bool rotating = false;
                for (int i = 0; i < physGrabObject.playerGrabbing.Count; i++)
                {
                    if (physGrabObject.playerGrabbing[i].isRotating)
                    {
                        rotating = true;
                        break;
                    }
                }
                if (!rotating)
                {
                    physGrabObject.OverrideTorqueStrengthX(0f);
                    physGrabObject.OverrideTorqueStrengthZ(0f);
                    Quaternion rotator = Quaternion.FromToRotation(transform.up, Vector3.up);
                    physGrabObject.rb.AddTorque(new Vector3(rotator.x, rotator.y, rotator.z) * balanceForce);
                }
                physGrabObject.rb.AddForce((Random.insideUnitSphere / 2f) + (transform.up / 1.3f), ForceMode.Impulse);
            }
        }
        public void Update()
        {
            if (SemiFunc.IsMultiplayer() && physGrabObject.grabbedLocal && PlayerVoiceChat.instance.isTalking && mooTrigger)
            {
                int mooNum = new System.Random().Next(-3, 4);
                if (mooNum <= 0)
                {
                    mooNum = 1;
                }
                string finalMessage = "";
                for (int i = 1; i < mooNum; i++)
                {
                    int oNum = new System.Random().Next(2, 6);
                    string mooString = "m";
                    for (int j = 0; j < oNum; j++)
                    {
                        mooString += "o";
                    }
                    mooString += " ";
                    finalMessage += mooString;
                }
                finalMessage = finalMessage.Trim();
                log.LogDebug($"Moo Cow making player chat: \"{finalMessage}\"");
                ChatManager.instance.PossessChatScheduleStart(9);
                ChatManager.instance.PossessChat(ChatManager.PossessChatID.LovePotion, finalMessage, 2f, Color.blue);
                ChatManager.instance.PossessChatScheduleEnd();
                mooTrigger = false;
            }
            if (!ChatManager.instance.chatActive && ChatManager.instance.spamTimer <= 0 && !mooTrigger)
            {
                mooTrigger = true;
            }
            if (physGrabObject.grabbed)
            {
                if (!animator.GetBool("Grabbed"))
                {
                    animator.SetBool("Grabbed", true);
                }
                if (!cowSounds.Source.isPlaying && mooTimer <= 0f)
                {
                    EnemyDirector.instance.SetInvestigate(transform.position, 15f);
                    cowSounds.Play(physGrabObject.rb.worldCenterOfMass);
                    mooTimer = (Random.value + 0.25f) * 2f;
                }
                else if (mooTimer > 0f)
                {
                    mooTimer -= Time.deltaTime;
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