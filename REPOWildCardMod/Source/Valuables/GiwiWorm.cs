using Photon.Pun;
using UnityEngine;
namespace REPOWildCardMod.Valuables
{
    public class GiwiWormValuable : MonoBehaviour
    {
        readonly BepInEx.Logging.ManualLogSource log = WildCardMod.instance.log;
        public Rigidbody[] rigidBodies;
        public Sound giwiSounds;
        public PhysGrabObject physGrabObject;
        public PhotonView photonView;
        public Animator animator;
        public float animTimer = 0f;
        public float animLerp;
        public float startTime = 0.5f;
        public float targetTime = 0.5f;
        public float animSpeed;
        public float dropTimer;
        public void Start()
        {
            if (GameManager.Multiplayer() && !PhotonNetwork.IsMasterClient)
            {
                for (int i = 0; i < rigidBodies.Length; i++)
                {
                    rigidBodies[i].collisionDetectionMode = CollisionDetectionMode.Discrete;
                }
            }
        }
        public void Update()
        {
            if (physGrabObject.grabbedLocal)
            {
                PhysGrabber.instance.OverrideGrabPoint(rigidBodies[1].transform);
                PhysGrabber.instance.OverridePullDistanceIncrement(-0.5f * Time.fixedDeltaTime);
                physGrabObject.OverrideGrabStrength(40f);
            }
            if (physGrabObject.grabbed)
            {
                if (!giwiSounds.Source.isPlaying)
                {
                    EnemyDirector.instance.SetInvestigate(transform.position, 10f);
                    giwiSounds.Play(rigidBodies[10].transform.position);
                    log.LogDebug($"{giwiSounds.Source.clip.name}");
                }
            }
            else
            {
                if (giwiSounds.Source.isPlaying)
                {
                    giwiSounds.Stop();
                }
            }
            if (animTimer <= 0)
            {
                animLerp = 0f;
                if (SemiFunc.IsMasterClientOrSingleplayer())
                {
                    animTimer = Random.Range(0.5f, 2f);
                    float speed = Mathf.Max(Random.value, 0.125f) * 2f;
                    float motionTime = Random.value;
                    if (GameManager.Multiplayer())
                    {
                        photonView.RPC("TargetTimeRPC", RpcTarget.All, motionTime, targetTime, speed, animTimer);
                    }
                    else
                    {
                        TargetTimeRPC(motionTime, targetTime, speed, animTimer);
                    }
                }
            }
            else if (animLerp < 1f)
            {
                animator.SetFloat("Motion Time", Mathf.Lerp(startTime, targetTime, Mathf.Clamp01(animLerp)));
                animLerp += Time.deltaTime * animSpeed;
            }
            else
            {
                animTimer -= Time.deltaTime;
            }
        }
        public void FixedUpdate()
        {
            if (physGrabObject.grabbed && SemiFunc.IsMasterClientOrSingleplayer())
            {
                for (int i = 1; i < rigidBodies.Length; i++)
                {
                    rigidBodies[i].AddForce(new Vector3(Random.Range(-5f, 5f) / Mathf.Sqrt(i), Random.Range(0f, 0.25f), Random.Range(-5f, 5f) / Mathf.Sqrt(i)), ForceMode.Impulse);
                }
                if (dropTimer != 5f)
                {
                    dropTimer = 5f;
                }
            }
            else if (SemiFunc.IsMasterClientOrSingleplayer())
            {
                if (dropTimer > 0f)
                {
                    rigidBodies[10].AddForce(new Vector3(Random.Range(-10f, 10f), Random.Range(0f, 0.5f), Random.Range(-10f, 10f)), ForceMode.Impulse);
                    dropTimer -= Time.deltaTime;
                }
            }
        }
        [PunRPC]
        public void TargetTimeRPC(float tTime, float sTime, float speed, float animTime)
        {
            log.LogDebug($"{gameObject.name}'s Animation Values: \"{tTime}\", \"{sTime}\", \"{speed}\", \"{animTime}\"");
            targetTime = tTime;
            startTime = sTime;
            animSpeed = speed;
            animTimer = animTime;
        }
    }
}