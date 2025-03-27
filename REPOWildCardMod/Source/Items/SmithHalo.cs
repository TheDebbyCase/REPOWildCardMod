using UnityEngine;
namespace REPOWildCardMod.Items
{
    public class SmithHalo : MonoBehaviour
    {
        readonly BepInEx.Logging.ManualLogSource log = WildCardMod.log;
        public PhysGrabObject physGrabObject;
        public ItemMelee itemMelee;
        public Rigidbody rigidBody;
        public Animator animator;
        public ParticleSystem hitParticles;
        public ParticleSystem dripParticles;
        public float balanceForce = 2.5f;
        public bool debugBool;
        public void FixedUpdate()
        {
            if (!physGrabObject.grabbed && SemiFunc.IsMasterClientOrSingleplayer())
            {
                Quaternion rotator = Quaternion.FromToRotation(transform.up, Vector3.up);
                physGrabObject.rb.AddTorque(new Vector3(rotator.x, rotator.y, rotator.z) * balanceForce);
            }
        }
        public void Update()
        {
            if (physGrabObject.grabbedLocal)
            {
                if (itemMelee.isSwinging || SemiFunc.InputHold(InputKey.Interact))
                {
                    PhysGrabber.instance.OverrideGrabDistance(2.5f);
                    if (!debugBool)
                    {
                        log.LogDebug($"{gameObject.name} setting grab distance to {PhysGrabber.instance.pullerDistance}!");
                        debugBool = true;
                    }
                }
                else
                {
                    PhysGrabber.instance.OverrideGrabDistance(1.5f);
                    if (debugBool)
                    {
                        log.LogDebug($"{gameObject.name} setting grab distance to {PhysGrabber.instance.pullerDistance}!");
                        debugBool = false;
                    }
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
            if (itemMelee.isBroken && dripParticles.isPlaying)
            {
                log.LogDebug($"{gameObject.name} is broken!");
                dripParticles.Stop();
            }
            else if (!itemMelee.isBroken && !dripParticles.isPlaying)
            {
                log.LogDebug($"{gameObject.name} is fixed!");
                dripParticles.Play();
            }
        }
        public void Hit()
        {
            hitParticles.Play();
        }
    }
}