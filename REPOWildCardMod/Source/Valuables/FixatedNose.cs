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
                physGrabObject.DestroyPhysGrabObject();
            }
        }
        public void ImpactSquish()
        {
            animator.SetLayerWeight(1, Mathf.Clamp01(physGrabObject.impactDetector.impactForce / 150f));
            animator.SetTrigger("Squish");
            TrapStart();
        }
        public void NoseExplode()
        {
            if (Vector3.Distance(transform.position, PlayerAvatar.instance.clientPosition) < 10f)
            {
                CameraGlitch.Instance.PlayShort();
            }
            enemyInvestigate = true;
            enemyInvestigateRange = 10f;
            log.LogDebug($"{gameObject.name} is exploding!");
            explodeScript.Spawn(transform.position, 0.245f, 5, 5, 2.5f);
        }
    }
}