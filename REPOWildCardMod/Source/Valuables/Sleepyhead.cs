using Photon.Pun;
using UnityEngine;
namespace REPOWildCardMod.Valuables
{
    public class Sleepyhead : Trap
    {
        public int state = 1;
        public Sound[] mouseSounds;
        public Texture[] faceTextures;
        public Texture[] particleTextures;
        public MeshRenderer meshRenderer;
        public ParticleSystem particleSystem;
        public ParticleSystemRenderer particleSystemRenderer;
        public bool angry = true;
        public float angerTimer = 0f;
        public override void Update()
        {
            base.Update();
            if (physGrabObject.grabbed)
            {
                TrapStart();
                trapTriggered = true;
            }
            else if (trapTriggered)
            {
                trapTriggered = false;
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
            StateImpulse(state, angry);
            mouseSounds[0].PlayLoop(!angry, 1f, 1f);
            mouseSounds[1].PlayLoop(angry, 1f, 1f);
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
            }
            else if (!angerState && id == 1)
            {
                angry = true;
                enemyInvestigate = true;
                trapActive = true;
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
                photonView.RPC("SetStateRPC", RpcTarget.All, index);
            }
            else
            {
                SetStateRPC(index);
            }
        }
        public void SetStateRPC(int index)
        {
            state = index;
        }
    }
}