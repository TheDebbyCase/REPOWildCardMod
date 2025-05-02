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
        public GameObject[] colliders;
        public bool hasSettled;
        public bool wasGrabbed;
        public bool beingThrown;
        public void Update()
        {
            if (SemiFunc.IsMasterClientOrSingleplayer())
            {
                physGrabObject.OverrideIndestructible();
                if (physGrabObject.rbAngularVelocity.magnitude > 0.01f)
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
                    wasGrabbed = false;
                    beingThrown = true;
                    physGrabObject.rb.AddTorque(Random.onUnitSphere * 5f, ForceMode.Impulse);
                    physGrabObject.rb.AddForce((Vector3.up * Random.Range(0.5f, 2f)) + new Vector3(((Random.value * 2f) - 1f) * 2f, 0f, (Random.value * 2f) - 1f) * 2f, ForceMode.Impulse);
                    hasSettled = false;
                    ToggleCollider(false);
                }
                if (hasSettled && beingThrown)
                {
                    if (diceTimer > 0f)
                    {
                        diceTimer -= Time.deltaTime;
                    }
                    else
                    {
                        RollDice();
                    }
                }
                else if (diceTimer != 1f)
                {
                    diceTimer = 1f;
                }
                if (physGrabObject.rb.freezeRotation != hasSettled)
                {
                    FreezeRotation(hasSettled);
                }
            }
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
            diceTimer = 1f;
            beingThrown = false;
            ToggleCollider(true);
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