using REPOWildCardMod.Utils;
using UnityEngine;
namespace REPOWildCardMod.Valuables
{
    public class MooCowValuable : MonoBehaviour
    {
        public WildCardUtils utils = WildCardMod.utils;
        public PhysGrabObject physGrabObject;
        public Animator animator;
        public PhysicMaterial physMat;
        public Sound cowSounds;
        public float mooTimer;
        public float playerMooTimer;
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
                if (!animator.GetBool("Grabbed"))
                {
                    animator.SetBool("Grabbed", true);
                }
                if (!cowSounds.Source.isPlaying && mooTimer <= 0f)
                {
                    EnemyDirector.instance.SetInvestigate(this.transform.position, 15f);
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
                if (utils.pauseVoice)
                {
                    utils.pauseVoice = false;
                }
            }
        }
        public void Update()
        {
            if (physGrabObject.grabbedLocal && SemiFunc.IsMultiplayer())
            {
                if (!utils.pauseVoice)
                {
                    utils.pauseVoice = true;
                }
                if (PhysGrabber.instance.playerAvatar.voiceChat.isTalking && playerMooTimer <= 0 && !ChatManager.instance.chatActive)
                {
                    int mooNum = new System.Random().Next(-3, 5);
                    if (mooNum <= 0)
                    {
                        mooNum = 1;
                    }
                    string mooString = "moooo ";
                    for (int i = 1; i < mooNum; i++)
                    {
                        mooString += mooString;
                    }
                    ChatManager.instance.PossessChatScheduleStart(9);
                    ChatManager.instance.PossessChat(ChatManager.PossessChatID.LovePotion, mooString, 2f, Color.blue);
                    ChatManager.instance.PossessChatScheduleEnd();
                    playerMooTimer = (Random.value + 0.5f) * (Random.value + 1f);
                }
                if (playerMooTimer >= 0f && !ChatManager.instance.chatActive)
                {
                    playerMooTimer -= Time.deltaTime;
                }
            }
        }
        public void ImpactSquish()
        {
            animator.SetLayerWeight(1, Mathf.Clamp01(physGrabObject.impactDetector.impactForce / 150f));
            animator.SetTrigger("Squish");
        }
    }
}