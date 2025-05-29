using Photon.Pun;
using UnityEngine;
namespace REPOWildCardMod.Valuables
{
    public class CloverBee : MonoBehaviour
    {
        readonly BepInEx.Logging.ManualLogSource log = WildCardMod.instance.log;
        public PhotonView photonView;
        public PhysGrabObject physGrabObject;
        public Texture2D[] eyeTextures;
        public MeshRenderer meshRenderer;
        public Sound angryBees;
        public Sound happyBees;
        public Animator animator;
        public Color originalEmission;
        public Color originalFresnelEmission;
        public float blinkTimer;
        public float unblinkTimer;
        public float balanceForce = 4f;
        public float angerTimer;
        public float playerDamageTimer;
        public float floatHeight = 0.75f;
        public float floatPower = 5f;
        public float glidePower = 0.5f;
        public void Start()
        {
            originalEmission = meshRenderer.materials[0].GetColor("_EmissionColor");
            originalFresnelEmission = meshRenderer.materials[0].GetColor("_FresnelEmissionColor");
        }
        public void FixedUpdate()
        {
            if (SemiFunc.IsMasterClientOrSingleplayer())
            {
                Quaternion rotator = Quaternion.FromToRotation(transform.up, Vector3.up);
                physGrabObject.rb.AddTorque(new Vector3(rotator.x, rotator.y, rotator.z) * balanceForce);
                if (physGrabObject.grabbed)
                {
                    if (angerTimer > 0f)
                    {
                        physGrabObject.rb.AddForce((Random.insideUnitSphere / 2f) + (transform.up / 1.3f), ForceMode.Impulse);
                    }
                }
                else if (Physics.Raycast(physGrabObject.rb.worldCenterOfMass, -Vector3.up, out RaycastHit hit, floatHeight, LayerMask.GetMask("Default", "PhysGrabObject", "PhysGrabObjectCart", "PhysGrabObjectHinge", "Enemy", "Player"), QueryTriggerInteraction.Ignore))
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
            bool angry = angerTimer > 0f;
            angryBees.PlayLoop(angry, 2f, 1f);
            happyBees.PlayLoop(!angry, 2f, 1f);
            if (physGrabObject.impactHappenedTimer > 0f)
            {
                if (animator.GetFloat("Wing Speed") != 0f)
                {
                    animator.SetFloat("Wing Speed", 0f);
                }
            }
            else
            {
                if (animator.GetFloat("Wing Speed") != 1f)
                {
                    animator.SetFloat("Wing Speed", 1f);
                }
            }
            if (angerTimer > 0f)
            {
                if (physGrabObject.grabbedLocal && playerDamageTimer <= 0f)
                {
                    PlayerAvatar.instance.playerHealth.Hurt(1, true);
                    playerDamageTimer = 0.5f;
                }
                else if (playerDamageTimer > 0f)
                {
                    playerDamageTimer -= Time.deltaTime;
                }
                angerTimer -= Time.deltaTime;
            }
            else if (meshRenderer.materials[0].GetTexture("_BaseTexture") == eyeTextures[2])
            {
                SetFresnelTexture(meshRenderer.materials[0], eyeTextures[0]);
                SetEmission(meshRenderer.materials[0], true);
                SetEmission(meshRenderer.materials[1], true);
                log.LogDebug("Clover Bee Calm");
            }
            else
            {
                if (blinkTimer > 0f)
                {
                    blinkTimer -= Time.deltaTime;
                }
                else
                {
                    if (meshRenderer.materials[0].GetTexture("_BaseTexture") == eyeTextures[0])
                    {
                        SetFresnelTexture(meshRenderer.materials[0], eyeTextures[1]);
                        unblinkTimer = 0.25f;
                    }
                }
                if (unblinkTimer > 0f)
                {
                    unblinkTimer -= Time.deltaTime;
                }
                else if (meshRenderer.materials[0].GetTexture("_BaseTexture") == eyeTextures[1])
                {
                    SetFresnelTexture(meshRenderer.materials[0], eyeTextures[0]);
                    blinkTimer = Random.Range(0.75f, 2f);
                }
            }
        }
        public void OnBreak()
        {
            SetFresnelTexture(meshRenderer.materials[0], eyeTextures[2]);
            SetEmission(meshRenderer.materials[0], false, Color.red / 2f);
            SetEmission(meshRenderer.materials[1], false, Color.red / 2f);
            log.LogDebug("Clover Bee Angry");
            angerTimer = Random.Range(0.5f, 1.5f);
        }
        public void SetFresnelTexture(Material mat, Texture texture)
        {
            mat.SetTexture("_BaseTexture", texture);
            mat.SetTexture("_EmissionTexture", texture);
        }
        public void SetEmission(Material mat, bool original, Color colour = new Color())
        {
            if (original)
            {
                mat.SetColor("_EmissionColor", originalEmission);
                mat.SetColor("_FresnelEmissionColor", originalFresnelEmission);
            }
            else
            {
                mat.SetColor("_EmissionColor", colour);
                mat.SetColor("_FresnelEmissionColor", colour);
            }
        }
    }
}