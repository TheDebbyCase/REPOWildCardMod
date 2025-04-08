using UnityEngine;
namespace REPOWildCardMod.Items
{
    public class CloverNecklace : MonoBehaviour
    {
        readonly BepInEx.Logging.ManualLogSource log = WildCardMod.instance.log;
        public PhysGrabObject physGrabObject;
        public ItemBattery itemBattery;
        public ItemToggle itemToggle;
        public HurtCollider hurtCollider;
        public Rigidbody rigidBody;
        public ParticleSystem particleSystem;
        public PropLight light;
        public Sound beeAudio;
        public Animator animator;
        public Vector3 forceRotation = new Vector3(110f, 0f, 180f);
        public bool holding;
        public PlayerAvatar lastHolder;
        public void FixedUpdate()
        {
            float animNormal = Mathf.InverseLerp(2.5f, 10f, rigidBody.velocity.magnitude);
            if (animator.GetFloat("Normal") != animNormal)
            {
                animator.SetFloat("Normal", animNormal);
            }
            if (physGrabObject.grabbed && SemiFunc.IsMasterClientOrSingleplayer())
            {
                int nonRotatingGrabbers = physGrabObject.playerGrabbing.Count;
                for (int i = 0; i < physGrabObject.playerGrabbing.Count; i++)
                {
                    if (physGrabObject.playerGrabbing[i].isRotating)
                    {
                        nonRotatingGrabbers--;
                    }
                }
                if (nonRotatingGrabbers == physGrabObject.playerGrabbing.Count)
                {
                    physGrabObject.TurnXYZ(Quaternion.Euler(forceRotation.x, 0f, 0f), Quaternion.Euler(0f, forceRotation.y, 0f), Quaternion.Euler(0f, 0f, forceRotation.z));
                }
            }
        }
        public void Update()
        {
            if (physGrabObject.playerGrabbing.Count == 1 && lastHolder != physGrabObject.playerGrabbing[0].playerAvatar)
            {
                lastHolder = physGrabObject.playerGrabbing[0].playerAvatar;
            }
            if (physGrabObject.grabbedLocal && !holding)
            {
                PhysGrabber.instance.OverrideGrabDistance(0.5f);
                holding = true;
            }
            else if (!physGrabObject.grabbedLocal)
            {
                holding = false;
            }
            if (itemBattery.batteryLife <= 0f && !itemToggle.disabled)
            {
                itemToggle.ToggleDisable(true);
                itemToggle.ToggleItem(false);
            }
            beeAudio.PlayLoop(itemToggle.toggleState, 1f, 0.25f);
            if (itemToggle.toggleState != itemBattery.batteryActive)
            {
                log.LogDebug($"Clover Necklace activated? \"{itemToggle.toggleState}\"");
                itemBattery.BatteryToggle(itemToggle.toggleState);
                hurtCollider.gameObject.SetActive(itemToggle.toggleState);
                light.lightComponent.enabled = itemToggle.toggleState;
                light.halo.enabled = itemToggle.toggleState;
                ParticleSystem.MainModule main = particleSystem.main;
                main.loop = itemToggle.toggleState;
                if (itemToggle.toggleState)
                {
                    particleSystem.Play();
                }
            }
        }
        public void EnemyHit()
        {
            itemBattery.batteryLife -= 5f;
            for (int i = 0; i < physGrabObject.playerGrabbing.Count; i++)
            {
                Enemy enemy = SemiFunc.EnemyGetNearest(transform.position, 5f, false);
                if (enemy != null)
                {
                    physGrabObject.playerGrabbing[i].playerAvatar.ForceImpulse((physGrabObject.playerGrabbing[i].playerAvatar.clientPositionCurrent - enemy.CenterTransform.position).normalized * 10f);
                }
            }
        }
    }
}