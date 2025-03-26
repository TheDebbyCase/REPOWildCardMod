using UnityEngine;
namespace REPOWildCardMod.Items
{
    public class CloverNecklace : MonoBehaviour
    {
        readonly BepInEx.Logging.ManualLogSource log = WildCardMod.log;
        public PhysGrabObject physGrabObject;
        public ItemBattery itemBattery;
        public ItemToggle itemToggle;
        public HurtCollider hurtCollider;
        public Rigidbody rigidBody;
        public ParticleSystem particleSystem;
        public PropLight light;
        public Sound beeAudio;
        public Animator animator;
        public void Update()
        {
            if (itemBattery.batteryLife <= 0f && !itemToggle.disabled)
            {
                itemToggle.ToggleDisable(true);
                itemToggle.ToggleItem(false);
            }
            beeAudio.PlayLoop(itemToggle.toggleState, 0.75f, 1f);
            if (itemToggle.toggleState != itemBattery.batteryActive)
            {
                log.LogDebug($"Clover Necklace activated? \"{itemToggle.toggleState}\"");
                itemBattery.BatteryToggle(itemToggle.toggleState);
                hurtCollider.gameObject.SetActive(itemToggle.toggleState);
                light.lightComponent.enabled = itemToggle.toggleState;
                ParticleSystem.MainModule main = particleSystem.main;
                main.loop = itemToggle.toggleState;
                if (itemToggle.toggleState)
                {
                    particleSystem.Play();
                }
            }
        }
        public void FixedUpdate()
        {
            float animNormal = Mathf.InverseLerp(2.5f, 10f, rigidBody.velocity.magnitude);
            if (animator.GetFloat("Normal") != animNormal)
            {
                animator.SetFloat("Normal", animNormal);
            }
        }
        public void EnemyHit()
        {
            itemBattery.Drain(1f);
            for (int i = 0; i < physGrabObject.playerGrabbing.Count; i++)
            {
                physGrabObject.playerGrabbing[i].playerAvatar.ForceImpulse((physGrabObject.playerGrabbing[i].playerAvatar.clientPositionCurrent - SemiFunc.EnemyGetNearest(transform.position, 3f, false).CenterTransform.position).normalized * 1.5f);
            }
        }
    }
}