using Photon.Pun;
using System.Collections;
using UnityEngine;
namespace REPOWildCardMod.Valuables
{
    public class PixelJar : MonoBehaviour
    {
        public ParticleSystem particle;
        public ParticleSystemRenderer particleRenderer;
        public Animator animator;
        public bool settled = true;
        public PhysGrabObject physGrabObject;
        public Texture2D[] floaterVariants;
        public int floaterID;
        public float bobLerp;
        public float verticalSpeed;
        public float horizontalSpeed;
        public PhotonView photonView;
        public void Start()
        {
            if (SemiFunc.IsMasterClientOrSingleplayer())
            {
                StartCoroutine(FloaterCoroutine(new System.Random().Next(0, floaterVariants.Length)));
            }
        }
        public IEnumerator FloaterCoroutine(int index)
        {
            yield return new WaitUntil(() => LevelGenerator.Instance.Generated);
            if (SemiFunc.IsMultiplayer())
            {
                photonView.RPC("FloaterTextureRPC", RpcTarget.All, index);
            }
            else
            {
                SetTexture(index);
            }
        }
        public void FixedUpdate()
        {
            if (bobLerp > 0.25f)
            {
                if (settled)
                {
                    settled = false;
                }
                animator.SetFloat("Vertical Speed", Mathf.Clamp(bobLerp * verticalSpeed, 1f, 2f));
                animator.SetFloat("Horizontal Speed", Mathf.Clamp(bobLerp * horizontalSpeed, 1f, 2f));
                animator.SetLayerWeight(1, bobLerp);
                animator.SetLayerWeight(2, bobLerp);
                bobLerp -= Time.deltaTime / 2f;
            }
            else if (!settled)
            {
                settled = true;
                animator.SetFloat("Vertical Speed", 1f);
                animator.SetFloat("Horizontal Speed", 1f);
                animator.SetLayerWeight(1, 0.25f);
                animator.SetLayerWeight(2, 0.25f);
                bobLerp = 0.25f;
            }
        }
        public void ImpactBob()
        {
            bobLerp = Mathf.Clamp(physGrabObject.impactDetector.impactForce / 75f, 0.5f, 2f);
            RandomSpeed();
        }
        public void DestroyParticle()
        {
            particle.Stop();
            particle.Clear();
        }
        public void RandomSpeed()
        {
            bool negative;
            negative = Random.value > 0.5f;
            if (negative)
            {
                verticalSpeed = (Random.value + 1f) * -1f;
                horizontalSpeed = Random.value + 1f;
            }
            else
            {
                verticalSpeed = Random.value + 1f;
                horizontalSpeed = (Random.value + 1f) * -1f;
            }
        }
        public void SetTexture(int index)
        {
            floaterID = index;
            particleRenderer.material.mainTexture = floaterVariants[floaterID];
            particleRenderer.material.SetTexture("_EmissionMap", floaterVariants[floaterID]);
            RandomSpeed();
        }
        [PunRPC]
        public void FloaterTextureRPC(int index)
        {
            SetTexture(index);
        }
    }
}