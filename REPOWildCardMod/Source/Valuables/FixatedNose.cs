using System.Collections;
using UnityEngine;
namespace REPOWildCardMod.Valuables
{
    public class FixatedNose : MonoBehaviour
    {
        readonly BepInEx.Logging.ManualLogSource log = WildCardMod.log;
        public AudioSource source;
        public float lerp;
        public AnimationCurve curve;
        public void Awake()
        {
            log.LogDebug("Fixated Nose has Spawned!");
        }
        public void Honk(bool breaking)
        {
            source.Play();
            if (breaking)
            {
                StartCoroutine(HonkCoroutine());
            }
        }
        public IEnumerator HonkCoroutine()
        {
            yield return null;
            source.pitch = Mathf.Lerp(1f, 0f, curve.Evaluate(lerp));
            lerp += Time.deltaTime * 2f;
        }
    }
}