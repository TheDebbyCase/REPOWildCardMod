using Photon.Pun;
using System.Collections.Generic;
using UnityEngine;
namespace REPOWildCardMod.Valuables
{
    public class GiwiWormValuable : MonoBehaviour
    {
        readonly BepInEx.Logging.ManualLogSource log = WildCardMod.instance.log;
        public Rigidbody[] rigidBodies;
        public Transform[] allTransforms;
        public List<Transform> heldTransforms = new List<Transform>();
        public int localHeldTransformIndex = -1;
        public Sound giwiSounds;
        public PhysGrabObject physGrabObject;
        public PhotonView photonView;
        public Animator animator;
        public float animTimer = 0f;
        public float animLerp;
        public float startTime = 0.5f;
        public float targetTime = 0.5f;
        public float animSpeed;
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
                localHeldTransformIndex = System.Array.IndexOf(allTransforms, PhysGrabber.instance.grabbedObjectTransform);
                if (SemiFunc.IsMasterClientOrSingleplayer())
                {
                    AddTransformRPC(localHeldTransformIndex);
                }
                else
                {
                    photonView.RPC("AddTransformRPC", RpcTarget.MasterClient, localHeldTransformIndex);
                }
                PhysGrabber.instance.OverridePullDistanceIncrement(-0.5f * Time.fixedDeltaTime);
            }
            else if (localHeldTransformIndex != -1)
            {
                if (SemiFunc.IsMasterClientOrSingleplayer())
                {
                    RemoveTransformRPC(localHeldTransformIndex);
                }
                else
                {
                    photonView.RPC("RemoveTransformRPC", RpcTarget.MasterClient, localHeldTransformIndex);
                }
                localHeldTransformIndex = -1;
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
                Vector3 position = physGrabObject.rb.transform.InverseTransformPoint(massCenter);
                if (SemiFunc.IsMultiplayer())
                {
                    photonView.RPC("CenterOfMassRPC", RpcTarget.All, position);
                }
                else
                {
                    CenterOfMassRPC(position);
                }
                for (int i = 1; i < rigidBodies.Length; i++)
                {
                    Rigidbody rigidBody = rigidBodies[i];
                    rigidBody.AddForce((Random.insideUnitSphere + (Vector3.up * 0.05f)) * 0.25f, ForceMode.Impulse);
                    rigidBody.AddTorque(Random.insideUnitSphere, ForceMode.Impulse);
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
        [PunRPC]
        public void AddTransformRPC(int id)
        {
            if (!heldTransforms.Contains(allTransforms[id]))
            {
                heldTransforms.Add(allTransforms[id]);
                log.LogDebug($"{gameObject.name}'s {allTransforms[id].gameObject.name} has been added to the heldTransforms list!");
            }
        }
        [PunRPC]
        public void RemoveTransformRPC(int id)
        {
            if (heldTransforms.Contains(allTransforms[id]))
            {
                heldTransforms.Remove(allTransforms[id]);
                log.LogDebug($"{gameObject.name}'s {allTransforms[id].gameObject.name} has been removed from the heldTransforms list!");
            }
        }
        [PunRPC]
        public void CenterOfMassRPC(Vector3 position)
        {
            physGrabObject.rb.centerOfMass = position;
        }
    }
}