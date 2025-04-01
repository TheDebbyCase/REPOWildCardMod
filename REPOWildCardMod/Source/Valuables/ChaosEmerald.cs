using Photon.Pun;
using UnityEngine;
namespace REPOWildCardMod.Valuables
{
    public class ChaosEmerald : MonoBehaviour
    {
        public PhotonView photonView;
        public ValuableObject valuableObject;
        public Color[] colors;
        public MeshRenderer meshRenderer;
        public void Start()
        {
            if (SemiFunc.IsMasterClientOrSingleplayer())
            {
                if (SemiFunc.IsMultiplayer())
                {
                    photonView.RPC("PickColorRPC", RpcTarget.All, Random.Range(0, colors.Length));
                }
                else
                {
                    PickColorRPC(Random.Range(0, colors.Length));
                }
            }
        }
        [PunRPC]
        public void PickColorRPC(int index)
        {
            meshRenderer.material.SetColor("_BaseColor", colors[index]);
            GradientColorKey[] colorKeys = valuableObject.particleColors.colorKeys;
            for (int i = 0; i < colorKeys.Length; i++)
            {
                colorKeys[i] = new GradientColorKey(colors[index], colorKeys[i].time);
            }
            valuableObject.particleColors.colorKeys = colorKeys;
            if (index == 6)
            {
                meshRenderer.material.SetColor("_EmissionColor", new Color(0.4f, 0.4f, 0.4f, 1f));
            }
            else
            {
                meshRenderer.material.SetColor("_EmissionColor", new Color(colors[index].r / 2f, colors[index].g / 2f, colors[index].b / 2f, 1f));
            }
        }
    }
}