using System;
using UnityEngine;
namespace REPOWildCardMod.Valuables
{
    public class AlolanVulpixie : MonoBehaviour
    {
        readonly BepInEx.Logging.ManualLogSource log = WildCardMod.instance.log;
        public PhysGrabObject physGrabObject;
        public GameObject[] pixieMeshes;
        public Sound[] pixieSounds;
        public PhysicMaterial physMat;
        public Animator animator;
        public bool scrungle = false;
        public void Start()
        {
            physGrabObject.OverrideMaterial(physMat, -123f);
        }
        public void PixieImpact(bool sad)
        {
            int index = Convert.ToInt32(sad);
            scrungle = !scrungle;
            pixieMeshes[1].SetActive(scrungle);
            pixieMeshes[0].SetActive(!scrungle);
            pixieSounds[index].Play(pixieSounds[index].Source.transform.position);
            log.LogDebug($"Vulpixie Scrungle: {scrungle}, Vulpixie Sadge: {sad}");
        }
        public void ImpactSquish()
        {
            animator.SetLayerWeight(1, Mathf.Clamp01(physGrabObject.impactDetector.impactForce / 150f));
            animator.SetTrigger("Squish");
        }
    }
}