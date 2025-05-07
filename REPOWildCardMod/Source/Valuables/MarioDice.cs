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
        public bool grabReset = false;
        public GameObject[] colliders;
        public bool hasSettled;
        public bool wasGrabbed;
        public bool beingThrown;
        public Transform lowestTransform;
        public float lastRotMag;
        public float cumRotMag;
        public float rotThreshhold = 85f;
        public void Awake()
        {
            valuableObject.dollarValueOverride = 500;
            layerMask = LayerMask.GetMask("Default", "PhysGrabObject", "PhysGrabObjectCart", "PhysGrabObjectHinge", "Enemy", "Player");
        }
        public void Update()
        {
            if (SemiFunc.IsMasterClientOrSingleplayer())
            {
                physGrabObject.OverrideIndestructible();
                if (physGrabObject.rbAngularVelocity.magnitude > 0.01f || cumRotMag < rotThreshhold)
                {
                    if (hasSettled)
                    {
                        hasSettled = false;
                    }
                }
                else if (!hasSettled)
                {
                    hasSettled = true;
                }
                if (physGrabObject.grabbed)
                {
                    if (!wasGrabbed)
                    {
                        wasGrabbed = true;
                    }
                    if (hasSettled)
                    {
                        hasSettled = false;
                    }
                }
                else if (wasGrabbed)
                {
                    Throw();
                }
                if (beingThrown)
                {
                    float maxLowHeight = diceNumbers[0].position.y;
                    int diceLowIndex = 0;
                    for (int i = 1; i < diceNumbers.Count; i++)
                    {
                        float newLowHeight = diceNumbers[i].position.y;
                        if (newLowHeight < maxLowHeight)
                        {
                            maxLowHeight = newLowHeight;
                            diceLowIndex = i;
                        }
                    }
                    lowestTransform = diceNumbers[diceLowIndex];
                    if (diceTimer > 0f)
                    {
                        lastRotMag = physGrabObject.rbAngularVelocity.magnitude;
                        cumRotMag += lastRotMag;
                        diceTimer -= Time.deltaTime;
                    }
                    else if (hasSettled)
                    {
                        RollDice();
                    }
                    else
                    {
                        Throw();
                    }
                }
                else if (diceTimer != 2f)
                {
                    diceTimer = 2f;
                    cumRotMag = 0f;
                }
                if (physGrabObject.rb.freezeRotation != hasSettled)
                {
                    FreezeRotation(hasSettled);
                }
            }
        }
        public void Throw(Vector3 overrideForce = default)
        {
            diceTimer = 2f;
            cumRotMag = 0f;
            wasGrabbed = false;
            beingThrown = true;
            physGrabObject.rb.AddTorque(Random.onUnitSphere * 10f, ForceMode.Impulse);
            Vector3 force;
            if (overrideForce == default)
            {
                force = (Vector3.up * Random.Range(0.5f, 2f)) + new Vector3(((Random.value * 2f) - 1f) * 2f, 0f, (Random.value * 2f) - 1f) * 2f;
            }
            else
            {
                force = overrideForce;
            }
            physGrabObject.rb.AddForce(force, ForceMode.Impulse);
            hasSettled = false;
            ToggleCollider(false);
        }
        public void FreezeRotation(bool freeze)
        {
            if (SemiFunc.IsMultiplayer())
            {
                physGrabObject.photonView.RPC("FreezeRotationRPC", RpcTarget.All, freeze);
            }
            else
            {
                FreezeRotationRPC(freeze);
            }
        }
        [PunRPC]
        public void FreezeRotationRPC(bool freeze)
        {
            physGrabObject.rb.freezeRotation = freeze;
        }
        public void RollDice()
        {
            for (int i = 0; i < diceNumbers.Count; i++)
            {
                if (diceNumbers[i] == lowestTransform)
                {
                    continue;
                }
                Vector3 direction = (diceNumbers[i].position - transform.position).normalized;
                if (Physics.Raycast(transform.position, direction, out RaycastHit hit, 0.25f, layerMask))
                {
                    Throw(direction * hit.distance * -8f);
                    return;
                }
            }
            beingThrown = false;
            ToggleCollider(true);
            float maxHeight = diceNumbers[0].position.y;
            int diceIndex = 0;
            for (int i = 1; i < diceNumbers.Count; i++)
            {
                float newHeight = diceNumbers[i].position.y;
                if (newHeight > maxHeight)
                {
                    maxHeight = newHeight;
                    diceIndex = i;
                }
            }
            diceIndex++;
            log.LogDebug($"Mario Dice rolled: \"{diceIndex}\"");
            float multiplier = 0f;
            switch (diceIndex)
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
                        break;
                    }
            }
            if (multiplier < 0f)
            {
                EnemyDirector.instance.SetInvestigate(transform.position, float.MaxValue);
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
            diceSound.Play(transform.position);
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
            EnemyDirector.instance.SetInvestigate(transform.position, float.MaxValue);
            explodeScript.Spawn(transform.position, 5f, 100, 100, 10f);
        }
    }
}