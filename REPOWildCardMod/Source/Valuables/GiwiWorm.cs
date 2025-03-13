using Photon.Pun;
using UnityEngine;
namespace REPOWildCardMod.Valuables
{
    public class GiwiWormValuable : MonoBehaviour
    {
        readonly BepInEx.Logging.ManualLogSource log = WildCardMod.log;
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
        public PhysGrabber lastGrabber;
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
            giwiSounds.Source.outputAudioMixerGroup = AudioManager.instance.SoundMasterGroup;
            physGrabObject.OverrideGrabStrength(1.75f);
        }
        public void FixedUpdate()
        {
            if (physGrabObject.grabbedLocal)
            {
                lastGrabber = PhysGrabber.instance;
                lastGrabber.OverridePullDistanceIncrement(-0.5f * Time.fixedDeltaTime);
            }
            if (physGrabObject.grabbed)
            {
                if (SemiFunc.IsMasterClientOrSingleplayer())
                {
                    for (int i = 1; i < rigidBodies.Length; i++)
                    {
                        Rigidbody rigidBody = rigidBodies[i];
                        rigidBody.AddForce((Random.insideUnitSphere + (Vector3.up * 0.05f)) * 0.5f, ForceMode.Impulse);
                        rigidBody.AddTorque(Random.insideUnitSphere, ForceMode.Impulse);
                    }
                }
                if (!giwiSounds.Source.isPlaying)
                {
                    EnemyDirector.instance.SetInvestigate(this.transform.position, 10f);
                    giwiSounds.Play(rigidBodies[11].transform.position);
                    log.LogDebug($"{giwiSounds.Source.clip.name}");
                }
            }
            else if (giwiSounds.Source.isPlaying)
            {
                giwiSounds.Stop();
            }
            if (animTimer <= 0)
            {
                animLerp = 0f;
                if (SemiFunc.IsMasterClientOrSingleplayer())
                {
                    animTimer = (float)random.Next(5, 20) / 10f;
                    float speed = Random.value + 1f;
                    float motionTime = Random.value;
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
    }
}