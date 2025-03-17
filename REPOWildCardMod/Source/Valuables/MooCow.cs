using UnityEngine;
namespace REPOWildCardMod.Valuables
{
    public class MooCowValuable : MonoBehaviour
    {
        public PhysGrabObject physGrabObject;
        public Animator animator;
        public PhysicMaterial physMat;
        public Sound cowSounds;
        public float mooTimer;
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
            }
        }
        public void ImpactSquish()
        {
            animator.SetLayerWeight(1, Mathf.Clamp01(physGrabObject.impactDetector.impactForce / 150f));
            animator.SetTrigger("Squish");
        }
    }
}