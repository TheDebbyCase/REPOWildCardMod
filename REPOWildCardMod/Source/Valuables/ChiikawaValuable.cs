using Photon.Pun;
using System;
using System.Collections;
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
        public MeshRenderer faceRenderer;
        public Material faceMaterial;
        public Sound chiikawaSounds;
        public Animator animator;
        public float faceTimer;
        public void Awake()
        {
            for (int i = 0; i < types.Length; i++)
            {
                types[i].chosenTransforms = new Transform[] { types[i].chosenBody, types[i].chosenHead, types[i].chosenLeftEar, types[i].chosenRightEar, types[i].chosenLeftArm, types[i].chosenRightArm, types[i].chosenLeftLeg, types[i].chosenRightLeg, types[i].chosenTail };
            }
        }
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
            log.LogDebug($"Chiikawa character selected: \"{chiikawa.name}\"");
            for (int i = 0; i < chiikawa.chosenTransforms.Length; i++)
            {
                chiikawa.chosenTransforms[i].gameObject.SetActive(true);
            }
            PhysAudio newAudio = ScriptableObject.CreateInstance<PhysAudio>();
            newAudio.impactLight = valuableObject.audioPreset.impactLight;
            newAudio.impactMedium = valuableObject.audioPreset.impactMedium;
            newAudio.impactHeavy = valuableObject.audioPreset.impactHeavy;
            newAudio.breakLight = valuableObject.audioPreset.breakLight;
            newAudio.breakMedium = valuableObject.audioPreset.breakMedium;
            newAudio.breakHeavy = valuableObject.audioPreset.breakHeavy;
            newAudio.destroy = valuableObject.audioPreset.destroy;
            newAudio.breakLight.Sounds = chiikawa.audioClips;
            newAudio.breakMedium.Sounds = chiikawa.audioClips;
            newAudio.breakHeavy.Sounds = chiikawa.audioClips;
            valuableObject.audioPreset = newAudio;
            chiikawaSounds.Sounds = chiikawa.audioClips;
            gameObject.name = $"Valuable {chiikawa.name}";
            physGrabObject.OverrideMaterial(new PhysicMaterial { dynamicFriction = 0.25f, staticFriction = 0.05f, bounciness = chiikawa.bounciness, frictionCombine = PhysicMaterialCombine.Average, bounceCombine = PhysicMaterialCombine.Maximum }, -123f);
            animator.SetLayerWeight(1, (chiikawa.wiggle * 0.5f) + 0.5f);
            faceRenderer = chiikawa.chosenHead.GetComponent<MeshRenderer>();
            if (chiikawa.newMainMaterial != null)
            {
                GradientColorKey[] colorKeys = valuableObject.particleColors.colorKeys;
                for (int i = 0; i < colorKeys.Length; i++)
                {
                    colorKeys[i] = new GradientColorKey(chiikawa.newMainMaterial.color, colorKeys[i].time);
                }
                valuableObject.particleColors.colorKeys = colorKeys;
                for (int i = 0; i < chiikawa.chosenTransforms.Length; i++)
                {
                    Transform[] children = chiikawa.chosenTransforms[i].GetComponentsInChildren<Transform>();
                    for (int j = 0; j < children.Length; j++)
                    {
                        if (children[j].TryGetComponent<MeshRenderer>(out MeshRenderer renderer))
                        {
                            if (renderer.materials.Length == 0)
                            {
                                renderer.materials = new Material[] { chiikawa.newMainMaterial };
                            }
                            else if (renderer.materials.Length == 2)
                            {
                                Material newMaterial = chiikawa.newMainMaterial;
                                if (chiikawa.overrideHeadMaterial != null)
                                {
                                    newMaterial = chiikawa.overrideHeadMaterial;
                                }
                                renderer.materials = new Material[] { newMaterial, faceMaterial };
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
                        chiikawaSounds.Play(chiikawa.chosenHead.position);
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
        public Transform chosenBody;
        public Transform chosenHead;
        public Transform chosenLeftEar;
        public Transform chosenRightEar;
        public Transform chosenLeftArm;
        public Transform chosenRightArm;
        public Transform chosenLeftLeg;
        public Transform chosenRightLeg;
        public Transform chosenTail;
        public Material newMainMaterial;
        public Material overrideHeadMaterial;
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