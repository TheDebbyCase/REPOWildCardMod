using Photon.Pun;
using UnityEngine;
namespace REPOWildCardMod.Valuables
{
    public class MarioDice : MonoBehaviour
    {
        readonly BepInEx.Logging.ManualLogSource log = WildCardMod.instance.log;
        public PhysGrabObject physGrabObject;
        public ParticleScriptExplosion explodeScript;
        public Transform[] diceNumbers;
        public Sound diceSound;
        public float diceTimer;
        public bool grabReset = false;
        public bool throwToggle = false;
        public GameObject[] colliders;
        public bool hasSettled;
        public bool wasGrabbed;
        public bool beingThrown;
        public void FixedUpdate()
        {
            if (SemiFunc.IsMasterClientOrSingleplayer())
            {
                if (throwToggle)
                {
                    physGrabObject.rb.AddTorque(Random.onUnitSphere * 5f, ForceMode.Impulse);
                    physGrabObject.rb.AddForce((Random.onUnitSphere * 1.5f) + Vector3.up, ForceMode.Impulse);
                    throwToggle = false;
                    hasSettled = false;
                    ToggleCollider(false);
                }
                if (hasSettled)
                {
                    if (!physGrabObject.rb.freezeRotation)
                    {
                        physGrabObject.rb.freezeRotation = true;
                    }
                }
                else
                {
                    if (physGrabObject.rb.freezeRotation)
                    {
                        physGrabObject.rb.freezeRotation = false;
                    }
                }
            }
        }
        public void Update()
        {
            if (SemiFunc.IsMasterClientOrSingleplayer())
            {
                physGrabObject.OverrideIndestructible();
                if (physGrabObject.grabbed && !wasGrabbed)
                {
                    wasGrabbed = true;
                }
                if (physGrabObject.grabbed || !hasSettled)
                {
                    if (diceTimer != 1f)
                    {
                        diceTimer = 1f;
                    }
                    if (hasSettled)
                    {
                        hasSettled = false;
                    }
                }
                else if (diceTimer > 0f)
                {
                    diceTimer -= Time.deltaTime;
                }
                else if (beingThrown)
                {
                    RollDice();
                    diceTimer = 1f;
                    beingThrown = false;
                    ToggleCollider(true);
                }
                if (!physGrabObject.grabbed && wasGrabbed)
                {
                    wasGrabbed = false;
                    throwToggle = true;
                    beingThrown = true;
                }
                if (physGrabObject.rbAngularVelocity.magnitude > 0.01f)
                {
                    if (hasSettled)
                    {
                        hasSettled = false;
                    }
                }
                else
                {
                    if (!hasSettled && beingThrown)
                    {
                        hasSettled = true;
                    }
                }
            }
        }
        public void RollDice()
        {
            float maxHeight = 0f;
            int diceIndex = 0;
            for (int i = 0; i < diceNumbers.Length; i++)
            {
                float newHeight = diceNumbers[i].position.y;
                if (newHeight > maxHeight)
                {
                    maxHeight = newHeight;
                    diceIndex = i;
                }
            }
            diceIndex++;
            float multiplier = 0;
            switch (diceIndex)
            {
                case 1:
                    {
                        if (physGrabObject.impactDetector.valuableObject.dollarValueCurrent >= physGrabObject.impactDetector.valuableObject.dollarValueOriginal * 50f)
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
            PhysGrabObjectImpactDetector impactDetector = physGrabObject.impactDetector;
            if (Mathf.Abs(multiplier) * impactDetector.valuableObject.dollarValueCurrent > impactDetector.valuableObject.dollarValueOriginal * 50f)
            {
                multiplier = 1f - ((impactDetector.valuableObject.dollarValueOriginal * 50f) / impactDetector.valuableObject.dollarValueCurrent);
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
            explodeScript.Spawn(transform.position, 2f, 20, 20, 5f);
        }
    }
}