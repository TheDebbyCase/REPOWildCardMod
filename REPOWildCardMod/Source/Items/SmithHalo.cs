using UnityEngine;
namespace REPOWildCardMod.Items
{
    public class SmithHalo : MonoBehaviour
    {
        public PhysGrabObject physGrabObject;
        public ItemMelee itemMelee;
        public Rigidbody rigidBody;
        public Animator animator;
        public ParticleSystem particleSystem;
        public void Update()
        {
            if (physGrabObject.grabbedLocal)
            {
                if (itemMelee.isSwinging)
                {
                    PhysGrabber.instance.OverrideGrabDistance(2.5f);
                }
                else
                {
                    PhysGrabber.instance.OverrideGrabDistance(1f);
                }
            }
            if (physGrabObject.grabbed && !itemMelee.isBroken)
            {
                animator.SetFloat("Speed", Mathf.Clamp(Mathf.InverseLerp(0f, 5f, rigidBody.velocity.magnitude), 0.25f, 1f));
            }
            else if (animator.GetFloat("Speed") != 0.125f)
            {
                animator.SetFloat("Speed", 0.125f);
            }
        }
        public void Hit()
        {
            particleSystem.Play();
        }
    }
}