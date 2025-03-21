using UnityEngine;
namespace REPOWildCardMod.Valuables
{
    public class FixatedNoseTrap : Trap
    {
        readonly BepInEx.Logging.ManualLogSource log = WildCardMod.log;
        public ParticleScriptExplosion explodeScript;
        public PhysicMaterial physMat;
        public Animator animator;
        public override void Start()
        {
            base.Start();
            physGrabObject.OverrideMaterial(physMat, -123f);
        }
        public void ImpactSquish()
        {
            animator.SetLayerWeight(1, Mathf.Clamp01(physGrabObject.impactDetector.impactForce / 150f));
            animator.SetTrigger("Squish");
        }
        public void NoseExplode()
        {
            enemyInvestigateRange = 15f;
            if (Vector3.Distance(transform.position, PlayerController.instance.playerAvatar.transform.position) < 15f)
            {
                CameraGlitch.Instance.PlayTiny();
            }
            log.LogDebug($"{gameObject.name} is exploding!");
            explodeScript.Spawn(transform.position, 0.245f, 5, 5, 2.5f);
        }
    }
}