using UnityEngine;
namespace REPOWildCardMod.Valuables
{
    public class MooCowValuable : MonoBehaviour
    {
        readonly BepInEx.Logging.ManualLogSource log = WildCardMod.log;
        public PhysGrabObject physGrabObject;
        public Animator animator;
        public PhysicMaterial physMat;
        public Sound cowSounds;
        public float mooTimer;
        public float playerMooTimer = 0f;
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
        public void Update()
        {
            if (!SemiFunc.IsMultiplayer() || PhysGrabber.instance == null || !PhysGrabber.instance.grabbed || PhysGrabber.instance.grabbedPhysGrabObject == null || PhysGrabber.instance.grabbedPhysGrabObject != physGrabObject)
            {
                return;
            }
            if (PlayerVoiceChat.instance.isTalking && !ChatManager.instance.chatActive && playerMooTimer == 0f)
            {
                int mooNum = new System.Random().Next(-3, 5);
                if (mooNum <= 0)
                {
                    mooNum = 1;
                }
                string mooString = "moo ";
                for (int i = 1; i < mooNum; i++)
                {
                    mooString += mooString;
                }
                log.LogDebug($"{gameObject.name} making player chat: \"{mooString}\"");
                ChatManager.instance.PossessChatScheduleStart(9);
                ChatManager.instance.PossessChat(ChatManager.PossessChatID.LovePotion, mooString, 2f, Color.blue);
                ChatManager.instance.PossessChatScheduleEnd();
                playerMooTimer = 0.25f;
            }
            else if (playerMooTimer < 0f)
            {
                playerMooTimer = 0f;
            }
            else
            {
                playerMooTimer -= Time.deltaTime;
            }
        }
        public void ImpactSquish()
        {
            animator.SetLayerWeight(1, Mathf.Clamp01(physGrabObject.impactDetector.impactForce / 150f));
            animator.SetTrigger("Squish");
        }
    }
}