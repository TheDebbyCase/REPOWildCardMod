using Photon.Pun;
using UnityEngine;
namespace REPOWildCardMod.Valuables
{
    public enum JarFloater
    {
        Other,
        V0,
        V1,
        V2
    }
    public class PixelJar : MonoBehaviour
    {
        readonly BepInEx.Logging.ManualLogSource log = WildCardMod.log;
        public ParticleSystem particle;
        public ParticleSystemRenderer particleRenderer;
        public Animator animator;
        public bool settled = true;
        public PhysGrabObject physGrabObject;
        public Texture2D[] floaterVariants;
        public JarFloater floater;
        public float bobLerp;
        public float verticalSpeed;
        public float horizontalSpeed;
        public bool pickingUp = true;
        public PhotonView photonView;
        private readonly System.Random random = new System.Random();
        public void Start()
        {
            if (SemiFunc.IsMasterClientOrSingleplayer())
            {
                int index = random.Next(0, floaterVariants.Length);
                if (SemiFunc.IsMultiplayer())
                {
                    photonView.RPC("FloaterTextureRPC", RpcTarget.All, index);
                }
                else
                {
                    SetTexture(index);
                }
            }
        }
        public void FixedUpdate()
        {
            if (bobLerp > 0.25f)
            {
                if (settled)
                {
                    settled = false;
                }
                animator.SetFloat("Vertical Speed", Mathf.Clamp(bobLerp * verticalSpeed, 1f, 2f));
                animator.SetFloat("Horizontal Speed", Mathf.Clamp(bobLerp * horizontalSpeed, 1f, 2f));
                animator.SetLayerWeight(1, bobLerp);
                animator.SetLayerWeight(2, bobLerp);
                bobLerp -= Time.deltaTime / 2f;
            }
            else if (!settled)
            {
                settled = true;
                animator.SetFloat("Vertical Speed", 1f);
                animator.SetFloat("Horizontal Speed", 1f);
                animator.SetLayerWeight(1, 0.25f);
                animator.SetLayerWeight(2, 0.25f);
                bobLerp = 0.25f;
            }
            if (physGrabObject.grabbedLocal && pickingUp && SemiFunc.IsMultiplayer())
            {
                pickingUp = false;
                string message = " ON TOP";
                Color colour = new Color();
                switch (floater)
                {
                    case JarFloater.V0:
                        {
                            message = "V0" + message;
                            colour = Color.green;
                            break;
                        }
                    case JarFloater.V1:
                        {
                            message = "V1" + message;
                            colour = Color.cyan;
                            break;
                        }
                    case JarFloater.V2:
                        {
                            message = "V2" + message;
                            colour = new Color(0.5f, 0f, 1f);
                            break;
                        }
                    case JarFloater.Other:
                        {
                            return;
                        }
                }
                log.LogDebug($"{gameObject.name} making player chat: \"{message}\"");
                ChatManager.instance.PossessChatScheduleStart(9);
                ChatManager.instance.PossessChat(ChatManager.PossessChatID.LovePotion, message, 2f, colour);
                ChatManager.instance.PossessChatScheduleEnd();
            }
            else if (!physGrabObject.grabbedLocal && !pickingUp)
            {
                pickingUp = true;
            }
        }
        public void ImpactBob()
        {
            bobLerp = Mathf.Clamp(physGrabObject.impactDetector.impactForce / 75f, 0.5f, 2f);
            RandomSpeed();
        }
        public void DestroyParticle()
        {
            particle.Stop();
            particle.Clear();
        }
        public void RandomSpeed()
        {
            if (Random.value > 0.5f)
            {
                verticalSpeed = (Random.value + 1f) * -1f;
                horizontalSpeed = Random.value + 1f;
            }
            else
            {
                verticalSpeed = Random.value + 1f;
                horizontalSpeed = (Random.value + 1f) * -1f;
            }
            log.LogDebug($"{gameObject.name} selecting new speeds: Up/Down \"{verticalSpeed}\", Left/Right \"{horizontalSpeed}\"");
        }
        public void SetTexture(int index)
        {
            string name = floaterVariants[index].name;
            log.LogDebug($"{gameObject.name}'s selected floater was \"{name}\"");
            switch (name)
            {
                case "v0":
                    {
                        floater = JarFloater.V0;
                        break;
                    }
                case "v1":
                    {
                        floater = JarFloater.V1;
                        break;
                    }
                case "v2":
                    {
                        floater = JarFloater.V2;
                        break;
                    }
                default:
                    {
                        floater = JarFloater.Other;
                        break;
                    }
            }
            particleRenderer.material.mainTexture = floaterVariants[index];
            particleRenderer.material.SetTexture("_EmissionMap", floaterVariants[index]);
            RandomSpeed();
        }
        [PunRPC]
        public void FloaterTextureRPC(int index)
        {
            SetTexture(index);
        }
    }
}