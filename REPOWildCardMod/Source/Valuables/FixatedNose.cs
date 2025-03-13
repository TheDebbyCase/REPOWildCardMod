using UnityEngine;
namespace REPOWildCardMod.Valuables
{
    public class FixatedNoseTrap : Trap
    {
        readonly BepInEx.Logging.ManualLogSource log = WildCardMod.log;
        public ParticleScriptExplosion explodeScript;
        public Rigidbody rigidBody;
        public void NoseExplode()
        {
            enemyInvestigateRange = 15f;
            explodeScript.Spawn(this.transform.position, 0.245f, 5, 5, 2.5f);
        }
        public void NoseBounce()
        {
            if (SemiFunc.IsMasterClientOrSingleplayer() && Random.Range(0, 6) < 2)
            {
                Vector3 forceVector = Random.insideUnitSphere;
                Vector3 torqueVector = Random.insideUnitSphere;
                rigidBody.AddForce((new Vector3(forceVector.x, Mathf.Abs(forceVector.y), forceVector.z)) * 5f, ForceMode.Impulse);
                rigidBody.AddTorque((new Vector3(torqueVector.x, Mathf.Abs(torqueVector.y), torqueVector.z)) * 6f, ForceMode.Impulse);
            }
        }
    }
}