using UnityEngine;
namespace REPOWildCardMod.Items
{
    public class MelyBonk : MonoBehaviour
    {
        readonly BepInEx.Logging.ManualLogSource log = WildCardMod.instance.log;
        public PhysGrabObject physGrabObject;
        public ItemMelee itemMelee;
        public ParticleSystem sparkleParticles;
        public LayerMask mask;
        public ParticleSystem.EmissionModule emissionModule;
        public ExplosionPreset explosionPreset;
        public void Awake()
        {
            mask = SemiFunc.LayerMaskGetPhysGrabObject() + LayerMask.GetMask("Player") + LayerMask.GetMask("Default") + LayerMask.GetMask("Enemy");
            emissionModule = sparkleParticles.emission;
        }
        public void Update()
        {
            if (physGrabObject.grabbedLocal)
            {
                PhysGrabber.instance.OverrideGrabDistance(2f);
            }
            if (emissionModule.rateOverTimeMultiplier > 1f)
            {
                emissionModule.rateOverTimeMultiplier -= Time.deltaTime * 2f;
            }
            else if (emissionModule.rateOverTimeMultiplier < 1f)
            {
                emissionModule.rateOverTimeMultiplier = 1f;
            }
        }
        public void OnHit()
        {
            explosionPreset.explosionSoundMedium.Play(transform.position, 0.5f);
            explosionPreset.explosionSoundMediumGlobal.Play(transform.position, 0.5f);
            emissionModule.rateOverTimeMultiplier = 6f;
            sparkleParticles.Emit(10);
            Collider[] hits = Physics.OverlapSphere(physGrabObject.impactDetector.contactPoint, 5f, mask, QueryTriggerInteraction.Collide);
            for (int i = 0; i < hits.Length; i++)
            {
                if (hits[i].gameObject.CompareTag("Player"))
                {
                    PlayerAvatar player = hits[i].gameObject.GetComponentInParent<PlayerAvatar>();
                    if (player == null)
                    {
                        player = hits[i].gameObject.GetComponentInParent<PlayerController>().playerAvatarScript;
                    }
                    if (player != null && player.isLocal && player.isGrounded && !player.physGrabber.grabbed || (player.physGrabber.grabbed && player.physGrabber.grabbedPhysGrabObject != physGrabObject))
                    {
                        log.LogDebug("Mely Bonk Launching Player");
                        player.tumble.TumbleRequest(true, false);
                        player.tumble.TumbleOverrideTime(0.5f);
                        player.tumble.TumbleForce(Vector3.up * 15f);
                    }
                }
                if (SemiFunc.IsMasterClientOrSingleplayer())
                {
                    if (hits[i].gameObject.CompareTag("Phys Grab Object"))
                    {
                        PhysGrabObject physGrabObject = hits[i].gameObject.GetComponentInParent<PhysGrabObject>();
                        if (physGrabObject != null && physGrabObject != this.physGrabObject)
                        {
                            physGrabObject.rb.AddForce(Vector3.up * 0.5f, ForceMode.Impulse);
                        }
                    }
                    else
                    {
                        EnemyParent enemy = hits[i].gameObject.GetComponentInParent<EnemyParent>();
                        if (enemy != null)
                        {
                            if (enemy.Enemy.HasRigidbody)
                            {
                                enemy.Enemy.Rigidbody.FreezeForces(Vector3.up, Random.onUnitSphere);
                            }
                            if (enemy.Enemy.HasHealth)
                            {
                                enemy.Enemy.Health.Hurt(20, Vector3.up);
                            }
                        }
                    }
                }
            }
        }
    }
}