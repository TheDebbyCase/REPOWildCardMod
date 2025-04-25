using Photon.Pun;
using System;
using UnityEngine;
namespace REPOWildCardMod.Valuables
{
    public class GiwiWormValuable : MonoBehaviour
    {
        readonly BepInEx.Logging.ManualLogSource log = WildCardMod.instance.log;
        public GiwiRigidbody[] giwiRigidbodies;
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
                for (int i = 0; i < giwiRigidbodies.Length; i++)
                {
                    giwiRigidbodies[i].rb.collisionDetectionMode = CollisionDetectionMode.Discrete;
                }
            }
        }
        public void Update()
        {
            if (physGrabObject.grabbedLocal)
            {
                PhysGrabber.instance.OverridePullDistanceIncrement(-0.5f * Time.deltaTime);
                physGrabObject.OverrideGrabStrength(40f);
            }
            if (physGrabObject.grabbed)
            {
                if (!giwiSounds.Source.isPlaying)
                {
                    EnemyDirector.instance.SetInvestigate(transform.position, 10f);
                    giwiSounds.Play(giwiRigidbodies[10].rb.transform.position);
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
                    animTimer = UnityEngine.Random.Range(0.5f, 2f);
                    float speed = Mathf.Max(UnityEngine.Random.value, 0.125f) * 2f;
                    float motionTime = UnityEngine.Random.value;
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
            if (SemiFunc.IsMasterClientOrSingleplayer())
            {
                if (physGrabObject.grabbed || dropTimer > 0f)
                {
                    for (int i = 1; i < giwiRigidbodies.Length; i++)
                    {
                        giwiRigidbodies[i].newDirTimer -= Time.fixedDeltaTime;
                        if (giwiRigidbodies[i].newDirTimer <= 0f)
                        {
                            Vector3 vertVector;
                            if (physGrabObject.grabbed)
                            {
                                vertVector = new Vector3(1f, 1f, 1f);
                            }
                            else
                            {
                                vertVector = new Vector3(1f, 0.1f, 1f);
                            }
                            giwiRigidbodies[i].direction = Vector3.Scale(UnityEngine.Random.onUnitSphere, vertVector);
                            giwiRigidbodies[i].newDirTimer = UnityEngine.Random.Range(0.1f, 0.5f);
                        }
                        giwiRigidbodies[i].Wiggle(UnityEngine.Random.Range(1f, 10f) * ((float)Mathf.Min(i, 6) / 2f), UnityEngine.Random.Range(1f, 6f) * -1f);
                    }
                    if (physGrabObject.grabbed && dropTimer != 5f)
                    {
                        dropTimer = 5f;
                    }
                }
                if (!physGrabObject.grabbed)
                {
                    dropTimer -= Time.fixedDeltaTime;
                }
            }
        }
        [PunRPC]
        public void TargetTimeRPC(float tTime, float sTime, float speed, float animTime)
        {
            targetTime = tTime;
            startTime = sTime;
            animSpeed = speed;
            animTimer = animTime;
        }
    }
    [Serializable]
    public class GiwiRigidbody
    {
        public Rigidbody rb;
        public Vector3 direction;
        public float newDirTimer;
        public void Wiggle(float forceIntensity, float torqueIntensity)
        {
            rb.AddForce(direction * forceIntensity);
            rb.AddTorque(UnityEngine.Random.onUnitSphere * torqueIntensity);
        }
    }
}