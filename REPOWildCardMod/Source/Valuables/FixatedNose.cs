﻿using Photon.Pun;
using UnityEngine;
namespace REPOWildCardMod.Valuables
{
    public class FixatedNoseTrap : Trap
    {
        readonly BepInEx.Logging.ManualLogSource log = WildCardMod.instance.log;
        public ParticleScriptExplosion explodeScript;
        public PhysicMaterial physMat;
        public Animator animator;
        public override void Start()
        {
            base.Start();
            physGrabObject.OverrideMaterial(physMat, -123f);
        }
        public override void Update()
        {
            base.Update();
            if (trapStart && SemiFunc.IsMasterClientOrSingleplayer())
            {
                physGrabObject.impactDetector.DestroyObject();
            }
        }
        public void ImpactSquish()
        {
            if (SemiFunc.IsMasterClientOrSingleplayer())
            {
                float force = physGrabObject.impactDetector.impactForce;
                if (GameManager.Multiplayer())
                {
                    photonView.RPC("SquishRPC", RpcTarget.All, force);
                }
                else
                {
                    SquishRPC(force);
                }
                if (!physGrabObject.impactDetector.inCart && !physGrabObject.roomVolumeCheck.inExtractionPoint)
                {
                    TrapStart();
                }
            }
        }
        [PunRPC]
        public void SquishRPC(float force)
        {
            animator.SetLayerWeight(1, Mathf.Clamp01(force / 150f));
            animator.SetTrigger("Squish");
        }
        public void NoseExplode()
        {
            if (Vector3.Distance(transform.position, PlayerAvatar.instance.transform.position) < 10f)
            {
                CameraGlitch.Instance.PlayShort();
            }
            log.LogDebug($"{gameObject.name} is exploding!");
            if (SemiFunc.IsMasterClientOrSingleplayer())
            {
                enemyInvestigate = true;
                enemyInvestigateRange = 10f;
                float random = Random.value;
                if (SemiFunc.IsMultiplayer())
                {
                    photonView.RPC("ExplosionRPC", RpcTarget.All, random);
                }
                else
                {
                    ExplosionRPC(random);
                }
            }
        }
        [PunRPC]
        public void ExplosionRPC(float random)
        {
            if (random < 0.9f)
            {
                explodeScript.Spawn(transform.position, 0.245f, 5, 5, 2.5f);
            }
            else
            {
                explodeScript.Spawn(transform.position, 2f, 20, 20, 5f);
            }
        }
    }
}