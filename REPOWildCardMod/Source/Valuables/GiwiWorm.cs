using Photon.Pun;
using System;
using System.Collections.Generic;
using UnityEngine;
namespace REPOWildCardMod.Valuables
{
    public class GiwiWormValuable : MonoBehaviour
    {
        readonly BepInEx.Logging.ManualLogSource log = WildCardMod.log;
        public Rigidbody[] rigidBodies;
        public Transform[] allTransforms;
        public List<Transform> heldTransforms = new List<Transform>();
        public Sound giwiSounds;
        public PhysGrabObject physGrabObject;
        public PhotonView photonView;
        public Animator animator;
        public float animTimer = 0f;
        public float animLerp;
        public float startTime = 0.5f;
        public float targetTime = 0.5f;
        public float animSpeed;
        private readonly System.Random random = new System.Random();
        public void Start()
        {
            if (GameManager.Multiplayer() && !PhotonNetwork.IsMasterClient)
            {
                for (int i = 0; i < rigidBodies.Length; i++)
                {
                    rigidBodies[i].collisionDetectionMode = CollisionDetectionMode.Discrete;
                }
            }
            allTransforms = GetComponentsInChildren<Transform>();
        }
        public void FixedUpdate()
        {
            if (physGrabObject.grabbedLocal)
            {
                PhysGrabber.instance.OverridePullDistanceIncrement(-0.5f * Time.fixedDeltaTime);
                if (SemiFunc.IsMasterClientOrSingleplayer())
                {
                    AddTransformRPC(Array.IndexOf(allTransforms, PhysGrabber.instance.grabbedObjectTransform));
                }
                else
                {
                    photonView.RPC("AddTransformRPC", RpcTarget.MasterClient, Array.IndexOf(allTransforms, PhysGrabber.instance.grabbedObjectTransform));
                }
            }
            else if (SemiFunc.IsMasterClientOrSingleplayer() && heldTransforms.Count > 0)
            {
                heldTransforms.Clear();
            }
            if (physGrabObject.grabbed)
            {
                if (SemiFunc.IsMasterClientOrSingleplayer())
                {
                    Vector3 massCenter = Vector3.zero;
                    for (int i = 0; i < heldTransforms.Count; i++)
                    {
                        if (i == 0)
                        {
                            massCenter = heldTransforms[i].position;
                        }
                        else
                        {
                            massCenter = (massCenter + heldTransforms[i].position) / 2f;
                        }
                    }
                    physGrabObject.rb.centerOfMass = physGrabObject.rb.transform.InverseTransformPoint(massCenter);
                    for (int i = 1; i < rigidBodies.Length; i++)
                    {
                        Rigidbody rigidBody = rigidBodies[i];
                        rigidBody.AddForce((UnityEngine.Random.insideUnitSphere + (Vector3.up * 0.05f)) * 0.25f, ForceMode.Impulse);
                        rigidBody.AddTorque(UnityEngine.Random.insideUnitSphere, ForceMode.Impulse);
                    }
                }
                if (!giwiSounds.Source.isPlaying)
                {
                    EnemyDirector.instance.SetInvestigate(this.transform.position, 10f);
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
                if (physGrabObject.rb.centerOfMass != Vector3.zero)
                {
                    physGrabObject.rb.centerOfMass = Vector3.zero;
                }
            }
            if (animTimer <= 0)
            {
                animLerp = 0f;
                if (SemiFunc.IsMasterClientOrSingleplayer())
                {
                    animTimer = (float)random.Next(5, 21) / 10f;
                    float speed = UnityEngine.Random.value * 2f;
                    float motionTime = UnityEngine.Random.value;
                    if (GameManager.Multiplayer())
                    {
                        photonView.RPC("TargetTimeRPC", RpcTarget.All, motionTime, targetTime, speed);
                    }
                    else
                    {
                        TargetTimeRPC(motionTime, targetTime, speed);
                    }
                }
            }
            else if (animLerp < 1f)
            {
                animator.SetFloat("Motion Time", Mathf.Lerp(startTime, targetTime, animLerp));
                animLerp += Time.deltaTime * animSpeed;
            }
            animTimer -= Time.deltaTime;
        }
        [PunRPC]
        public void TargetTimeRPC(float tTime, float sTime, float speed)
        {
            targetTime = tTime;
            startTime = sTime;
            animSpeed = speed;
        }
        [PunRPC]
        public void AddTransformRPC(int id)
        {
            if (!heldTransforms.Contains(allTransforms[id]))
            {
                heldTransforms.Add(allTransforms[id]);
            }
        }
    }
}