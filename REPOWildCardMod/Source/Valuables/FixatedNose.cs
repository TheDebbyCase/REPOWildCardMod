using UnityEngine;
namespace REPOWildCardMod.Valuables
{
    public class FixatedNose : MonoBehaviour
    {
        public ParticleScriptExplosion explodeScript;
        public void NoseExplode()
        {
            explodeScript.Spawn(this.transform.position, 0.245f, 5, 5, 2.5f);
        }
    }
}