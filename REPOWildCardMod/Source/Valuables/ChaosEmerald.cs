using Photon.Pun;
using UnityEngine;
namespace REPOWildCardMod.Valuables
{
    public class ChaosEmerald : MonoBehaviour
    {
        public PhotonView photonView;
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
            if (index == 6)
            {
                meshRenderer.material.SetColor("_EmissionColor", new Color(0.2f, 0.2f, 0.2f, 1f));
            }
            else
            {
                meshRenderer.material.SetColor("_EmissionColor", new Color(colors[index].r / 4f, colors[index].g / 4f, colors[index].b / 4f, 1f));
            }
        }
    }
}