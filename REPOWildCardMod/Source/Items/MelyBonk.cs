using System.Collections.Generic;
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
        public ExplosionPreset explosionPreset;
        public bool itemBroken;
        public Collider headCollider;
        public void Awake()
        {
            mask = SemiFunc.LayerMaskGetPhysGrabObject() + LayerMask.GetMask("Player") + LayerMask.GetMask("Default") + LayerMask.GetMask("Enemy");
        }
        public void Update()
        {
            if (physGrabObject.grabbedLocal)
            {
                PhysGrabber.instance.OverrideGrabDistance(2f);
            }
            if (itemMelee.isBroken)
            {
                if (!itemBroken)
                {
                    itemBroken = true;
                    sparkleParticles.Stop();
                    headCollider.gameObject.SetActive(false);
                }
            }
            else if (itemBroken)
            {
                itemBroken = false;
                sparkleParticles.Play();
                headCollider.gameObject.SetActive(true);
            }
        }
        public void OnHit()
        {
            explosionPreset.explosionSoundMedium.Play(transform.position, 0.5f);
            explosionPreset.explosionSoundMediumGlobal.Play(transform.position, 0.5f);
            sparkleParticles.Emit(10);
            if (SemiFunc.IsMasterClientOrSingleplayer())
            {
                Collider[] hits = Physics.OverlapSphere(physGrabObject.impactDetector.contactPoint, 5f, mask, QueryTriggerInteraction.Collide);
                List<PhysGrabObject> validPhysHits = new List<PhysGrabObject>();
                List<EnemyParent> validEnemyHits = new List<EnemyParent>();
                bool durabilityLoss = false;
                for (int i = 0; i < hits.Length; i++)
                {
                    if (hits[i].gameObject.CompareTag("Player"))
                    {
                        PlayerAvatar player = hits[i].gameObject.GetComponentInParent<PlayerAvatar>();
                        if (player == null)
                        {
                            player = hits[i].gameObject.GetComponentInParent<PlayerController>().playerAvatarScript;
                        }
                        if (player != null && player.isGrounded && !player.physGrabber.grabbed || (player.physGrabber.grabbed && player.physGrabber.grabbedPhysGrabObject != physGrabObject))
                        {
                            log.LogDebug("Mely Bonk Launching Player");
                            player.tumble.TumbleRequest(true, false);
                            player.tumble.TumbleOverrideTime(0.5f);
                            player.tumble.TumbleForce(Vector3.up * 15f);
                            continue;
                        }
                    }
                    EnemyParent enemy = hits[i].gameObject.GetComponentInParent<EnemyParent>();
                    if (!validEnemyHits.Contains(enemy) && enemy != null)
                    {
                        log.LogDebug($"Mely Bonk hitting {enemy.enemyName}");
                        if (enemy.Enemy.HasStateStunned)
                        {
                            enemy.Enemy.StateStunned.Set(1f);
                        }
                        enemy.Enemy.Freeze(0.2f);
                        if (enemy.Enemy.HasRigidbody)
                        {
                            enemy.Enemy.Rigidbody.FreezeForces(Vector3.up * 4f, Random.onUnitSphere);
                        }
                        if (enemy.Enemy.HasHealth)
                        {
                            enemy.Enemy.Health.Hurt(35, Vector3.up * 2.5f);
                        }
                        validEnemyHits.Add(enemy);
                        durabilityLoss = true;
                        continue;
                    }
                    if (hits[i].gameObject.CompareTag("Phys Grab Object"))
                    {
                        PhysGrabObject physGrabObject = hits[i].gameObject.GetComponentInParent<PhysGrabObject>();
                        if (!validPhysHits.Contains(physGrabObject) && physGrabObject != null && physGrabObject != this.physGrabObject)
                        {
                            physGrabObject.rb.AddForce(Vector3.up * 2.5f * Mathf.Max(1f, 1f / physGrabObject.rb.mass), ForceMode.Impulse);
                            validPhysHits.Add(physGrabObject);
                        }
                    }
                }
                if (durabilityLoss)
                {
                    itemMelee.EnemySwingHit();
                }
            }
        }
    }
}