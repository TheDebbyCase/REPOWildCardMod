using UnityEngine;
namespace REPOWildCardMod.Valuables
{
    public class FixatedNoseTrap : Trap
    {
        readonly BepInEx.Logging.ManualLogSource log = WildCardMod.log;
        public ParticleScriptExplosion explodeScript;
        public PhysicMaterial physMat;
        public override void Start()
        {
            base.Start();
            physGrabObject.OverrideMaterial(physMat, -123f);
        }
        public void NoseExplode()
        {
            enemyInvestigateRange = 15f;
            explodeScript.Spawn(this.transform.position, 0.245f, 5, 5, 2.5f);
        }
    }
}