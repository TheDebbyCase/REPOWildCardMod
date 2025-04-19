using Photon.Pun;
using UnityEngine;
namespace REPOWildCardMod.Valuables
{
    public class MarioDice : MonoBehaviour
    {
        readonly BepInEx.Logging.ManualLogSource log = WildCardMod.instance.log;
        public PhysGrabObject physGrabObject;
        public Transform[] diceNumbers;
        public Sound diceSound;
        public float diceTimer;
        public bool grabReset = false;
        public bool throwToggle = true;
        public void FixedUpdate()
        {
            if (SemiFunc.IsMasterClientOrSingleplayer())
            {
                if (!physGrabObject.grabbed && throwToggle)
                {
                    physGrabObject.rb.AddTorque(Random.onUnitSphere * 2f, ForceMode.Impulse);
                    physGrabObject.rb.AddForce((Random.onUnitSphere * 0.5f) + Vector3.up, ForceMode.Impulse);
                    throwToggle = false;
                }
            }
        }
        public void Update()
        {
            if (SemiFunc.IsMasterClientOrSingleplayer())
            {
                physGrabObject.OverrideIndestructible();
                if (physGrabObject.grabbed)
                {
                    if (!throwToggle)
                    {
                        throwToggle = true;
                    }
                    if (!grabReset)
                    {
                        grabReset = true;
                    }
                }
                if (grabReset)
                {
                    if (physGrabObject.grabbed || physGrabObject.rbVelocity.magnitude > 0.1f)
                    {
                        if (diceTimer < 1.5f)
                        {
                            diceTimer = 1.5f;
                        }
                    }
                    else if (diceTimer > 0f)
                    {
                        diceTimer -= Time.deltaTime;
                    }
                    else if (diceTimer <= 0f)
                    {
                        RollDice();
                        diceTimer = 1.5f;
                        grabReset = false;
                        throwToggle = true;
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
            log.LogDebug($"Dice Value Changing from: \"{impactDetector.valuableObject.dollarValueCurrent}\", to: \"{impactDetector.valuableObject.dollarValueCurrent - (multiplier * impactDetector.valuableObject.dollarValueCurrent)}\"");
            if (SemiFunc.IsMultiplayer())
            {
                impactDetector.photonView.RPC("BreakRPC", RpcTarget.All, multiplier * impactDetector.valuableObject.dollarValueCurrent, physGrabObject.centerPoint, 2, true);
            }
            else
            {
                impactDetector.BreakRPC(multiplier * impactDetector.valuableObject.dollarValueCurrent, physGrabObject.centerPoint, 2, true);
            }
            if (multiplier < 1f)
            {
                diceSound.Pitch = 0.75f; 
            }
            else
            {
                diceSound.Pitch = 1f;
            }
            diceSound.Play(transform.position);
        }
    }
}