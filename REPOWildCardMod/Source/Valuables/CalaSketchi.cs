using Photon.Pun;
using REPOWildCardMod.Extensions;
using TMPro;
using UnityEngine;
namespace REPOWildCardMod.Valuables
{
    public enum SketchiState
    {
        Idle,
        Happy,
        Sleepy,
        Hungry
    }
    public class CalaSketchi : MonoBehaviour
    {
        readonly BepInEx.Logging.ManualLogSource log = WildCardMod.instance.log;
        public PhotonView photonView;
        public PhysGrabObject physGrabObject;
        public ItemToggle itemToggle;
        public SketchiState state = SketchiState.Idle;
        public Vector2 direction = Vector2.zero;
        public float moveSpeed = 0.25f;
        public float hunger = 100f;
        public float energy = 100f;
        public float happy = 0f;
        public float feedTimer = 0f;
        public float directionTimer;
        public float valueTimer;
        public Animator animator;
        public Transform spriteTransform;
        public Sound walkSounds;
        public Sound happySounds;
        public Sound sleepSounds;
        public Sound hungrySounds;
        public Sound feedSounds;
        public Vector3 forceRotation = new Vector3(110f, 0f, 180f);
        public void Awake()
        {
            physGrabObject.SetUIValueOffset(Vector3.up * 0.25f);
        }
        public void FixedUpdate()
        {
            if (!SemiFunc.IsMasterClientOrSingleplayer() || !LevelGenerator.Instance.Generated)
            {
                return;
            }
            if (physGrabObject.grabbed)
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
            if (!LevelGenerator.Instance.Generated || physGrabObject.hasNeverBeenGrabbed)
            {
                return;
            }
            float delta = Time.deltaTime;
            if (physGrabObject.grabbedLocal)
            {
                SetInteractUI();
                if (itemToggle.toggleState)
                {
                    itemToggle.ToggleItem(false);
                    if (feedTimer <= 0f)
                    {
                        Feed(Random.Range(5f, 20f));
                        feedTimer = 0.5f;
                    }
                }
                if (feedTimer > 0f)
                {
                    feedTimer -= delta;
                }
            }
            if (animator.GetBool("Moving"))
            {
                Vector3 newPosition = spriteTransform.localPosition + new Vector3(direction.x, direction.y, 0f) * delta * moveSpeed;
                spriteTransform.localPosition = new Vector3(Mathf.Clamp(newPosition.x, -0.325f, 0.375f), Mathf.Clamp(newPosition.y, -0.35f, 0.35f), 0f);
            }
            if (!SemiFunc.IsMasterClientOrSingleplayer())
            {
                return;
            }
            physGrabObject.OverrideIndestructible();
            switch (state)
            {
                case SketchiState.Idle:
                    {
                        StateIdle(delta);
                        break;
                    }
                case SketchiState.Happy:
                    {
                        StateHappy(delta);
                        break;
                    }
                case SketchiState.Sleepy:
                    {
                        StateSleepy(delta);
                        break;
                    }
                case SketchiState.Hungry:
                    {
                        StateHungry(delta);
                        break;
                    }

            }
        }
        public void StateIdle(float delta)
        {
            hunger -= delta * 1.5f;
            energy -= delta * 1.5f;
            directionTimer -= delta;
            if (directionTimer <= 0f)
            {
                directionTimer = Random.Range(1f, 2.5f);
                ChangeDirection(Random.value > 0.75f, Random.insideUnitCircle.normalized, Random.Range(0.25f, 1f));
            }
            if (hunger > 75f || (hunger > 50f && energy > 50f))
            {
                happy += delta;
            }
            else if (happy > 0f)
            {
                happy -= delta;
            }
            if (energy <= 0f)
            {
                SetState(SketchiState.Sleepy);
                return;
            }
            else if (hunger <= 0f)
            {
                SetState(SketchiState.Hungry);
                return;
            }
            else if (happy >= 50f)
            {
                SetState(SketchiState.Happy);
                return;
            }
        }
        public void StateHappy(float delta)
        {
            hunger -= delta * 1.5f;
            energy -= delta * 1.5f;
            happy -= delta * 1.5f;
            directionTimer -= delta;
            if (directionTimer <= 0f)
            {
                directionTimer = Random.Range(1f, 2.5f);
                ChangeDirection(Random.value > 0.75f, Random.insideUnitCircle.normalized, Random.Range(0.25f, 1f));
            }
            if (hunger > 75f || (hunger > 50f && energy > 50f))
            {
                happy += 3f * delta;
            }
            if (happy > 75f)
            {
                energy -= delta * 1.5f;
            }
            if (energy <= 0f)
            {
                SetState(SketchiState.Sleepy);
                return;
            }
            if (hunger <= 0f)
            {
                SetState(SketchiState.Hungry);
                return;
            }
            if (happy <= 25f)
            {
                SetState(SketchiState.Idle);
                return;
            }
        }
        public void StateSleepy(float delta)
        {
            if (hunger > 0f)
            {
                hunger -= delta * 1.5f;
            }
            energy += 4.5f * delta;
            if (energy >= 100f || (energy >= 85f && Random.value < 0.01f))
            {
                if (hunger <= 0f)
                {
                    SetState(SketchiState.Hungry);
                    return;
                }
                if (happy >= 50f)
                {
                    SetState(SketchiState.Happy);
                    return;
                }
                SetState(SketchiState.Idle);
                return;
            }
        }
        public void StateHungry(float delta)
        {
            energy -= delta * 1.5f;
            if (valueTimer > 0f)
            {
                valueTimer -= delta;
            }
            else
            {
                valueTimer = 1f;
                Break(100f);
            }
            if (happy > 0f)
            {
                happy -= 4.5f * delta;
            }
            if (energy <= 0f)
            {
                SetState(SketchiState.Sleepy);
                return;
            }
            if (hunger >= 25f)
            {
                SetState(SketchiState.Idle);
                return;
            }
        }
        public void Break(float value)
        {
            if (SemiFunc.IsMultiplayer())
            {
                physGrabObject.impactDetector.photonView.RPC("BreakRPC", RpcTarget.All, value, physGrabObject.centerPoint, 2, true);
            }
            else
            {
                physGrabObject.impactDetector.BreakRPC(value, physGrabObject.centerPoint, 2, true);
            }
        }
        public void ChangeDirection(bool isStill, Vector2 vector, float speed)
        {
            if (SemiFunc.IsMultiplayer())
            {
                photonView.RPC("ChangeDirectionRPC", RpcTarget.All, isStill, vector, speed);
            }
            else
            {
                ChangeDirectionRPC(isStill, vector, speed);
            }
        }
        [PunRPC]
        public void ChangeDirectionRPC(bool isStill, Vector2 vector, float speed)
        {
            if (isStill)
            {
                vector = Vector2.zero;
            }
            direction = vector;
            animator.SetBool("Vertical", Mathf.Abs(vector.y) > Mathf.Abs(vector.x));
            animator.SetBool("Moving", !isStill);
            if (!isStill)
            {
                log.LogDebug($"CalaSketchi New Move Direction: \"{direction}\"");
            }
        }
        public void BreakEffect()
        {
            if (SemiFunc.IsMasterClientOrSingleplayer() && state != SketchiState.Hungry)
            {
                hunger -= 10f;
            }
            hungrySounds.Play(transform.position);
        }
        public void SoundEffect()
        {
            switch (state)
            {
                case SketchiState.Idle:
                    {
                        walkSounds.Play(transform.position);
                        break;
                    }
                case SketchiState.Happy:
                    {
                        happySounds.Play(transform.position);
                        break;
                    }
                case SketchiState.Sleepy:
                    {
                        sleepSounds.Play(transform.position);
                        break;
                    }
                case SketchiState.Hungry:
                    {
                        hungrySounds.Play(transform.position);
                        break;
                    }
            }
        }
        public void Feed(float amount)
        {
            if (SemiFunc.IsMasterClientOrSingleplayer())
            {
                FeedRPC(amount);
            }
            else
            {
                photonView.RPC("FeedRPC", RpcTarget.All, amount);
            }
        }
        [PunRPC]
        public void FeedRPC(float amount)
        {
            if (SemiFunc.IsMasterClientOrSingleplayer())
            {
                hunger = Mathf.Clamp(hunger + Mathf.Max(0f, amount), 0f, 100f);
                happy = Mathf.Clamp(happy + Mathf.Max(0f, amount / 5f), 0f, 100f);
                physGrabObject.rb.AddForce(Random.insideUnitSphere, ForceMode.Impulse);
                physGrabObject.rb.AddTorque(Random.insideUnitSphere * 2.5f, ForceMode.Impulse);
            }
            feedSounds.Play(transform.position);
            log.LogDebug($"Feeding CalaSketchi: \"{amount}\"");
        }
        public void SetState(SketchiState newState)
        {
            if (SemiFunc.IsMultiplayer())
            {
                photonView.RPC("SetStateRPC", RpcTarget.All, newState);
            }
            else
            {
                SetStateRPC(newState);
            }
        }
        [PunRPC]
        public void SetStateRPC(SketchiState newState)
        {
            switch (newState)
            {
                case SketchiState.Idle:
                    {
                        animator.SetBool("Moving", false);
                        animator.SetInteger("State", 0);
                        break;
                    }
                case SketchiState.Happy:
                    {
                        animator.SetBool("Moving", false);
                        animator.SetInteger("State", 1);
                        break;
                    }
                case SketchiState.Sleepy:
                    {
                        animator.SetBool("Moving", false);
                        animator.SetInteger("State", 2);
                        break;
                    }
                case SketchiState.Hungry:
                    {
                        animator.SetBool("Moving", false);
                        animator.SetInteger("State", 3);
                        break;
                    }
            }
            state = newState;
            log.LogDebug($"CalaSketchi State: \"{state}\"");
        }
        public void SetInteractUI()
        {
            string message = $"CalaSketchi <color=#FFFFFF>[{InputManager.instance.InputDisplayReplaceTags("[interact]")}]</color>";
            if (message != ItemInfoUI.instance.Text.text)
            {
                ItemInfoUI.instance.messageTimer = 0f;
                ItemInfoUI.instance.SemiUIResetAllShakeEffects();
            }
            ItemInfoUI.instance.Text.colorGradient = ItemInfoUI.instance.originalGradient;
            if (!SemiFunc.RunIsShop())
            {
                ItemInfoUI.instance.Text.fontSize = 15f;
            }
            ItemInfoUI.instance.messageTimer = 0.1f;
            if (message != ItemInfoUI.instance.messagePrev)
            {
                ItemInfoUI.instance.Text.text = message;
                ItemInfoUI.instance.SemiUISpringShakeY(5f, 5f, 0.3f);
                ItemInfoUI.instance.SemiUISpringScale(0.1f, 2.5f, 0.2f);
                ItemInfoUI.instance.messagePrev = message;
            }
        }
    }
}