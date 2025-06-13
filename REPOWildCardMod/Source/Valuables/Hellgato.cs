using Photon.Pun;
using UnityEngine;
namespace REPOWildCardMod.Valuables
{
    public class Hellgato : MonoBehaviour
    {
        readonly BepInEx.Logging.ManualLogSource log = WildCardMod.instance.log;
        public PhotonView photonView;
        public PhysGrabObject physGrabObject;
        public ValuableObject valuableObject;
        public PhysGrabObjectImpactDetector impactDetector;
        public ParticleScriptExplosion explodeScript;
        public ParticleSystem particleSystem;
        public PropLight propLight;
        public Sound crackleLoop;
        public float loopTimer;
        public bool playLoop;
        public Transform floatPoint;
        public Animator animator;
        public float blinkTimer;
        public float earTimer;
        public float pawTimer;
        public bool lightTrigger;
        public AnimationCurve lightPulseCurve;
        public float lightPulseTimer;
        public bool wasGrabbed;
        public float alterTimer;
        public float balanceForce = 4f;
        public float floatHeight = 0.75f;
        public float floatPower = 5f;
        public float glidePower = 0.5f;
        public void Awake()
        {
            valuableObject.dollarValueOverride = Mathf.Max(1000, Mathf.RoundToInt(Random.Range(valuableObject.valuePreset.valueMin, valuableObject.valuePreset.valueMax) / 1000f) * 1000);
        }
        public void FixedUpdate()
        {
            if (!LevelGenerator.Instance.Generated)
            {
                return;
            }
            if (SemiFunc.IsMasterClientOrSingleplayer())
            {
                Quaternion rotator = Quaternion.FromToRotation(transform.up, Vector3.up);
                physGrabObject.rb.AddTorque(new Vector3(rotator.x, rotator.y, rotator.z) * balanceForce);
                if (Physics.Raycast(floatPoint.position, -Vector3.up, out RaycastHit hit, floatHeight, LayerMask.GetMask("Default", "PhysGrabObject", "PhysGrabObjectCart", "PhysGrabObjectHinge", "Enemy", "Player"), QueryTriggerInteraction.Ignore) && !physGrabObject.colliders.Contains(hit.collider.transform))
                {
                    physGrabObject.rb.AddForce(transform.up * (floatPower / hit.distance) * (1.1f - (Quaternion.Angle(Quaternion.identity, rotator) / 360f)));
                }
                else
                {
                    physGrabObject.rb.AddForce(transform.up * floatPower * glidePower * (1.1f - (Quaternion.Angle(Quaternion.identity, rotator) / 360f)));
                }
            }
        }
        public void Update()
        {
            if (!LevelGenerator.Instance.Generated)
            {
                return;
            }
            if (SemiFunc.IsMasterClientOrSingleplayer())
            {
                physGrabObject.OverrideIndestructible();
                if (blinkTimer > 0f)
                {
                    blinkTimer -= Time.deltaTime;
                }
                else
                {
                    blinkTimer = Random.Range(1f, 3f);
                    AnimTrigger("Blink");
                }
                if (earTimer > 0f)
                {
                    earTimer -= Time.deltaTime;
                }
                else
                {
                    earTimer = Random.Range(4f, 9f);
                    AnimTrigger("Ear Flick", "Ear Side", Random.Range(0, 2));
                }
                if (pawTimer > 0f)
                {
                    pawTimer -= Time.deltaTime;
                }
                else
                {
                    pawTimer = Random.Range(3f, 7f);
                    AnimTrigger("Paw Twitch", "Paw Side", Random.Range(0, 4));
                }
            }
            if (loopTimer > 0f)
            {
                loopTimer -= Time.deltaTime;
            }
            else
            {
                loopTimer = Random.Range(1f, 5f);
                playLoop = !playLoop;
            }
            crackleLoop.PlayLoop(playLoop, 1f, 1f, Mathf.Clamp(physGrabObject.rbVelocity.magnitude / 1.5f, 1f, 1.5f));
            animator.SetLayerWeight(3, Mathf.Clamp(Mathf.Abs(physGrabObject.rbVelocity.y / 2f), 0.1f, 1f));
            if (physGrabObject.grabbedLocal)
            {
                if (!wasGrabbed)
                {
                    wasGrabbed = true;
                    alterTimer = 1f;
                }
                if (alterTimer <= 0f)
                {
                    alterTimer = 1f;
                    ValueChange(!PhysGrabber.instance.isRotating);
                }
                alterTimer -= Time.deltaTime;
            }
            else if (wasGrabbed)
            {
                wasGrabbed = false;
            }
            if (lightTrigger)
            {
                particleSystem.Play();
                lightTrigger = false;
                lightPulseTimer = 1f;
            }
            if (lightPulseTimer > 0f)
            {
                propLight.lightComponent.intensity = lightPulseCurve.Evaluate(1f - lightPulseTimer);
                lightPulseTimer -= Time.deltaTime;
            }
        }
        public void ValueChange(bool healPlayer)
        {
            if (healPlayer)
            {
                if (PlayerAvatar.instance.playerHealth.health <= PlayerAvatar.instance.playerHealth.maxHealth - 10)
                {
                    Break(1000f);
                    PlayerAvatar.instance.playerHealth.HealOther(10, true);
                }
            }
            else if (PlayerAvatar.instance.playerHealth.health >= 11)
            {
                Break(-1000f);
                PlayerAvatar.instance.playerHealth.Hurt(10, true);
            }
        }
        public void Break(float value)
        {
            if (SemiFunc.IsMultiplayer())
            {
                impactDetector.photonView.RPC("BreakRPC", RpcTarget.All, value, physGrabObject.centerPoint, 2, true);
            }
            else
            {
                impactDetector.BreakRPC(value, physGrabObject.centerPoint, 2, true);
            }
        }
        public void BreakEffect()
        {
            lightTrigger = true;
            int randomImpact = Random.Range(0, 3);
            Sound chosenImpact = null;
            switch (randomImpact)
            {
                case 0:
                    {
                        chosenImpact = valuableObject.audioPreset.impactLight;
                        break;
                    }
                case 1:
                    {
                        chosenImpact = valuableObject.audioPreset.impactMedium;
                        break;
                    }
                case 2:
                    {
                        chosenImpact = valuableObject.audioPreset.impactHeavy;
                        break;
                    }
            }
            chosenImpact.Play(transform.position);
            if (SemiFunc.IsMasterClientOrSingleplayer())
            {
                physGrabObject.rb.AddForce(Random.insideUnitSphere * 2f, ForceMode.Impulse);
                physGrabObject.rb.AddTorque(Random.insideUnitSphere * 3.5f, ForceMode.Impulse);
            }
        }
        public void AnimTrigger(string trigger, string randomiser = "", int value = -1)
        {
            if (SemiFunc.IsMultiplayer())
            {
                photonView.RPC("AnimTriggerRPC", RpcTarget.All, trigger, randomiser, value);
            }
            else
            {
                AnimTriggerRPC(trigger, randomiser, value);
            }
        }
        [PunRPC]
        public void AnimTriggerRPC(string trigger, string randomiser, int value)
        {
            if (randomiser != "" && value > -1)
            {
                animator.SetInteger(randomiser, value);
            }
            animator.SetTrigger(trigger);
        }
        public void ImpactSquish()
        {
            if (SemiFunc.IsMasterClientOrSingleplayer())
            {
                float force = impactDetector.impactForce;
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
        public void DestroyEffect()
        {
            EnemyDirector.instance.SetInvestigate(physGrabObject.centerPoint, float.MaxValue);
            explodeScript.Spawn(physGrabObject.centerPoint, 1f, 5, 10, 0.75f);
        }
    }
}