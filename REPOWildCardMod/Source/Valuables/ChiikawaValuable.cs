using Photon.Pun;
using System;
using System.Collections;
using System.Linq;
using UnityEngine;
namespace REPOWildCardMod.Valuables
{
    public class ChiikawaValuable : MonoBehaviour
    {
        readonly BepInEx.Logging.ManualLogSource log = WildCardMod.instance.log;
        public PhotonView photonView;
        public ValuableObject valuableObject;
        public PhysGrabObject physGrabObject;
        public ChiikawaType[] types;
        public ChiikawaType chiikawa;
        public bool chiikawaChosen;
        public Transform[] parentTransforms;
        public MeshRenderer faceRenderer;
        public Material faceMaterial;
        public Sound chiikawaSounds;
        public Animator animator;
        public float faceTimer;
        public void Start()
        {
            if (SemiFunc.IsMasterClientOrSingleplayer())
            {
                int index = UnityEngine.Random.Range(0, types.Length);
                while (types[index].audioClips.Length == 0)
                {
                    log.LogDebug($"{types[index].name} has not been set up, selecting a new chiikawa");
                    index = UnityEngine.Random.Range(0, types.Length);
                }
                if (SemiFunc.IsMultiplayer())
                {
                    photonView.RPC("SelectTypeRPC", RpcTarget.All, index);
                }
                else
                {
                    SelectTypeRPC(index);
                }
            }
            StartCoroutine(ChiikawaSetup());
        }
        public IEnumerator ChiikawaSetup()
        {
            yield return new WaitUntil(() => chiikawaChosen);
            if (chiikawa.chosenTransforms.Length != parentTransforms.Length)
            {
                log.LogWarning($"{chiikawa.name} has an invalid number of chosen transforms!");
            }
            for (int i = 0; i < chiikawa.chosenTransforms.Length; i++)
            {
                chiikawa.chosenTransforms[i].gameObject.SetActive(true);
            }
            valuableObject.audioPreset.breakLight.Sounds = chiikawa.audioClips;
            valuableObject.audioPreset.breakMedium.Sounds = chiikawa.audioClips;
            valuableObject.audioPreset.breakHeavy.Sounds = chiikawa.audioClips;
            chiikawaSounds.Sounds = chiikawa.audioClips;
            log.LogDebug($"Chiikawa character selected: \"{chiikawa.name}\"");
            gameObject.name = $"Valuable {chiikawa.name}";
            physGrabObject.OverrideMaterial(new PhysicMaterial { dynamicFriction = 0.25f, staticFriction = 0.05f, bounciness = chiikawa.bounciness, frictionCombine = PhysicMaterialCombine.Average, bounceCombine = PhysicMaterialCombine.Maximum }, -123f);
            animator.SetLayerWeight(1, chiikawa.wiggle);
            faceRenderer = chiikawa.chosenFace;
            if (chiikawa.newMainMaterial != null)
            {
                for (int i = 0; i < parentTransforms.Length; i++)
                {
                    for (int j = 0; j < parentTransforms[i].childCount; j++)
                    {
                        Transform bodyPart = parentTransforms[i].GetChild(j);
                        if (bodyPart.TryGetComponent<MeshRenderer>(out MeshRenderer renderer) && chiikawa.chosenTransforms.Contains(bodyPart))
                        {
                            if (renderer.materials.Length == 0)
                            {
                                renderer.materials = new Material[] { chiikawa.newMainMaterial };
                            }
                            else if (renderer.materials.Length == 2)
                            {
                                renderer.materials = new Material[] { chiikawa.newMainMaterial, faceMaterial };
                            }
                        }
                    }
                }
            }
        }
        [PunRPC]
        public void SelectTypeRPC(int i)
        {
            chiikawa = types[i];
            chiikawaChosen = true;
        }
        public void Update()
        {
            if (chiikawaSounds.Sounds.Length > 0)
            {
                if (physGrabObject.grabbed)
                {
                    if (!chiikawaSounds.Source.isPlaying)
                    {
                        chiikawaSounds.Play(faceRenderer.transform.position);
                    }
                    if (!animator.GetBool("Grabbed"))
                    {
                        animator.SetBool("Grabbed", true);
                    }
                    if (faceRenderer.materials[1].mainTexture != chiikawa.upsetFace)
                    {
                        faceRenderer.materials[1].mainTexture = chiikawa.upsetFace;
                        
                    }
                    if (faceTimer != 2f)
                    {
                        faceTimer = 2f;
                    }
                }
                else
                {
                    if (animator.GetBool("Grabbed"))
                    {
                        animator.SetBool("Grabbed", false);
                    }
                    if (faceTimer <= 0f && faceRenderer.materials[1].mainTexture != chiikawa.neutralFace)
                    {
                        faceRenderer.materials[1].mainTexture = chiikawa.neutralFace;
                    }
                    if (faceTimer > 0f)
                    {
                        faceTimer -= Time.deltaTime;
                    }
                }
            }
        }
        public void FixedUpdate()
        {
            if (SemiFunc.IsMasterClientOrSingleplayer() && physGrabObject.grabbed && chiikawaChosen)
            {
                physGrabObject.rb.AddForce((UnityEngine.Random.insideUnitSphere * chiikawa.wiggle) / 4f, ForceMode.Impulse);
                physGrabObject.rb.AddTorque(UnityEngine.Random.insideUnitSphere * chiikawa.wiggle, ForceMode.Impulse);
            }
        }
    }
    [Serializable]
    public class ChiikawaType
    {
        public string name;
        [Space(20)]
        [Header("Body Parts")]
        public Transform[] chosenTransforms;
        public MeshRenderer chosenFace;
        public Material newMainMaterial;
        [Space(10)]
        [Header("Face Textures")]
        public Texture2D neutralFace;
        public Texture2D upsetFace;
        [Space(10)]
        [Header("Attributes")]
        public AudioClip[] audioClips;
        [Range(0.01f, 1f)]
        public float wiggle;
        [Range(0.01f, 0.8f)]
        public float bounciness;
    }
}