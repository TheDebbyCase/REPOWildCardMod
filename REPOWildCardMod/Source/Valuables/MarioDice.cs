using Photon.Pun;
using System.Collections.Generic;
using UnityEngine;
namespace REPOWildCardMod.Valuables
{
    public class MarioDice : MonoBehaviour
    {
        readonly BepInEx.Logging.ManualLogSource log = WildCardMod.instance.log;
        public PhysGrabObject physGrabObject;
        public ValuableObject valuableObject;
        public ParticleScriptExplosion explodeScript;
        public List<Transform> diceNumbers;
        public Sound diceSound;
        public int layerMask;
        public float diceTimer;
        public bool wasGrabbed;
        public GameObject[] colliders;
        public int rollGoal = 0;
        public float balanceForce = 5f;
        public float floatPower = 5f;
        public void Awake()
        {
            valuableObject.dollarValueOverride = 500;
            layerMask = LayerMask.GetMask("Default", "PhysGrabObjectCart", "PhysGrabObjectHinge", "Enemy", "Player");
        }
        public void FixedUpdate()
        {
            if (SemiFunc.IsMasterClientOrSingleplayer() && diceTimer > 0f)
            {
                if (Physics.Raycast(physGrabObject.centerPoint, Vector3.down, out RaycastHit hit, 0.125f, layerMask))
                {
                    physGrabObject.rb.AddForce(Vector3.up * (floatPower / hit.distance));
                }
                Quaternion rotator = Quaternion.FromToRotation(diceNumbers[rollGoal - 1].position - physGrabObject.centerPoint, Vector3.up);
                physGrabObject.rb.AddTorque(new Vector3(rotator.x, rotator.y, rotator.z) * balanceForce * Mathf.Min(10f, 4f / diceTimer));
                Vector3 force = Vector3.zero;
                if (Physics.Raycast(physGrabObject.centerPoint, Vector3.forward, 0.25f, layerMask))
                {
                    force = -Vector3.forward;
                }
                else if (Physics.Raycast(physGrabObject.centerPoint, -Vector3.forward, 0.25f, layerMask))
                {
                    force = Vector3.forward;
                }
                else if (Physics.Raycast(physGrabObject.centerPoint, Vector3.right, 0.25f, layerMask))
                {
                    force = -Vector3.right;
                }
                else if (Physics.Raycast(physGrabObject.centerPoint, -Vector3.right, 0.25f, layerMask))
                {
                    force = Vector3.right;
                }
                physGrabObject.rb.AddForce(force / 4f, ForceMode.Impulse);
            }
        }
        public void Update()
        {
            if (SemiFunc.IsMasterClientOrSingleplayer())
            {
                physGrabObject.OverrideIndestructible();
                if (physGrabObject.grabbed)
                {
                    if (!wasGrabbed)
                    {
                        wasGrabbed = true;
                    }
                }
                else if (wasGrabbed)
                {
                    Throw(Random.Range(1, 7));
                    wasGrabbed = false;
                }
                if (diceTimer > 0f)
                {
                    diceTimer -= Time.deltaTime;
                }
                else if (rollGoal != 0)
                {
                    ToggleCollider(true);
                    RollDice(rollGoal);
                }
            }
        }
        public void Throw(int roll)
        {
            rollGoal = roll;
            ToggleCollider(false);
            diceTimer = 4f;
            physGrabObject.rb.AddTorque(Random.onUnitSphere * 10f, ForceMode.Impulse);
            physGrabObject.rb.AddForce((Vector3.up * Random.Range(0.5f, 2f)) + new Vector3(((Random.value * 2f) - 1f) * 2f, 0f, (Random.value * 2f) - 1f) * 2f, ForceMode.Impulse);
        }
        public void RollDice(int roll)
        {
            log.LogDebug($"Mario Dice rolled: \"{roll}\"");
            rollGoal = 0;
            float multiplier = 0f;
            switch (roll)
            {
                case 1:
                    {
                        if (physGrabObject.impactDetector.valuableObject.dollarValueCurrent >= physGrabObject.impactDetector.valuableObject.dollarValueOriginal * 45f)
                        {
                            if (SemiFunc.IsMultiplayer())
                            {
                                physGrabObject.impactDetector.photonView.RPC("BreakRPC", RpcTarget.All, physGrabObject.impactDetector.valuableObject.dollarValueCurrent, physGrabObject.centerPoint, 2, true);
                            }
                            else
                            {
                                physGrabObject.impactDetector.BreakRPC(physGrabObject.impactDetector.valuableObject.dollarValueCurrent, physGrabObject.centerPoint, 2, true);
                            }
                            return;
                        }
                        multiplier = (3f / 4f);
                        break;
                    }
                case 2:
                    {
                        multiplier = (2f / 3f);
                        break;
                    }
                case 3:
                    {
                        multiplier = (1f / 2f);
                        break;
                    }
                case 4:
                    {
                        multiplier = -1f;
                        break;
                    }
                case 5:
                    {
                        multiplier = -2f;
                        break;
                    }
                case 6:
                    {
                        multiplier = -3f;
                        EnemyDirector.instance.SetInvestigate(transform.position, 30f);
                        break;
                    }
            }
            PhysGrabObjectImpactDetector impactDetector = physGrabObject.impactDetector;
            if (Mathf.Abs(multiplier) * impactDetector.valuableObject.dollarValueCurrent > 15000)
            {
                multiplier = 1f - (15000 / impactDetector.valuableObject.dollarValueCurrent);
            }
            log.LogDebug($"Dice Value Changing from: \"{impactDetector.valuableObject.dollarValueCurrent}\", to: \"{impactDetector.valuableObject.dollarValueCurrent - (multiplier * impactDetector.valuableObject.dollarValueCurrent)}\"");
            if (SemiFunc.IsMultiplayer())
            {
                impactDetector.photonView.RPC("BreakRPC", RpcTarget.All, multiplier * impactDetector.valuableObject.dollarValueCurrent, physGrabObject.centerPoint, 2, true);
            }
            else
            {
                impactDetector.BreakRPC(multiplier * impactDetector.valuableObject.dollarValueCurrent, physGrabObject.centerPoint, 2, true);
            }
            if (Mathf.Abs(multiplier) < 1f)
            {
                diceSound.Pitch = 0.75f;
            }
            else
            {
                diceSound.Pitch = 1f;
            }
            diceSound.Play(physGrabObject.centerPoint);
        }
        public void ToggleCollider(bool grabbable)
        {
            if (SemiFunc.IsMultiplayer())
            {
                physGrabObject.photonView.RPC("ToggleColliderRPC", RpcTarget.All, grabbable);
            }
            else
            {
                ToggleColliderRPC(grabbable);
            }
        }
        [PunRPC]
        public void ToggleColliderRPC(bool grabbable)
        {
            if (grabbable)
            {
                colliders[0].SetActive(true);
                colliders[1].SetActive(false);
            }
            else
            {
                colliders[0].SetActive(false);
                colliders[1].SetActive(true);
            }
        }
        public void AntiGamblingLaws()
        {
            EnemyDirector.instance.SetInvestigate(physGrabObject.centerPoint, float.MaxValue);
            explodeScript.Spawn(physGrabObject.centerPoint, 5f, 50, 50, 2.5f);
        }
    }
}