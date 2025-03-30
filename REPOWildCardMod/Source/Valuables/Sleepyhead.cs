using Photon.Pun;
using UnityEngine;
namespace REPOWildCardMod.Valuables
{
    public class Sleepyhead : Trap
    {
        public int state = 0;
        public PhotonView sleepyPhotonView;
        public Sound[] mouseSounds;
        public Texture[] faceTextures;
        public Texture[] particleTextures;
        public MeshRenderer meshRenderer;
        public ParticleSystem particleSystem;
        public ParticleSystemRenderer particleSystemRenderer;
        public PhysicMaterial physMat;
        public bool angry = false;
        public float angerTimer = 0f;
        public bool impulse = true;
        public bool pickingUp = true;
        public override void Start()
        {
            base.Start();
            physGrabObject.OverrideMaterial(physMat, -123f);
        }
        public override void Update()
        {
            base.Update();
            if (physGrabObject.grabbed && !trapTriggered && pickingUp)
            {
                TrapStart();
                pickingUp = false;
            }
            else if (!physGrabObject.grabbed)
            {
                pickingUp = true;
            }
            if (SemiFunc.IsMasterClientOrSingleplayer())
            {
                if (trapStart && !angry)
                {
                    SetState(1);
                }
                if (angerTimer <= 0f && angry)
                {
                    SetState(0);
                }
                if (angerTimer > 0f)
                {
                    angerTimer -= Time.deltaTime;
                }
            }
            if (impulse)
            {
                StateImpulse(state, angry);
                impulse = false;
            }
            mouseSounds[0].PlayLoop(!angry, 1f, 2f);
            mouseSounds[1].PlayLoop(angry, 1f, 2f);
        }
        public void StateImpulse(int id, bool angerState)
        {
            meshRenderer.materials[1].mainTexture = faceTextures[id];
            particleSystemRenderer.material.mainTexture = particleTextures[id];
            if (angerState && id == 0)
            {
                trapStart = false;
                angry = false;
                enemyInvestigate = false;
                trapActive = false;
                trapTriggered = false;
            }
            else if (!angerState && id == 1)
            {
                angry = true;
                enemyInvestigateRange = 10f;
                enemyInvestigate = true;
                trapActive = true;
                trapTriggered = true;
                if (SemiFunc.IsMasterClientOrSingleplayer())
                {
                    angerTimer = Random.Range(7.5f, 20f);
                }
            }
        }
        public void SetState(int index)
        {
            if (SemiFunc.IsMultiplayer())
            {
                sleepyPhotonView.RPC("SetStateRPC", RpcTarget.All, index);
            }
            else
            {
                SetStateRPC(index);
            }
        }
        [PunRPC]
        public void SetStateRPC(int index)
        {
            if (state != index)
            {
                impulse = true;
            }
            state = index;
        }
    }
}