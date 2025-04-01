using Photon.Pun;
using System.Collections.Generic;
using UnityEngine;
namespace REPOWildCardMod.Valuables
{
    public class DragonBall : MonoBehaviour
    {
        static readonly BepInEx.Logging.ManualLogSource log = WildCardMod.instance.log;
        public PhotonView photonView;
        public Mesh[] starMeshes;
        public MeshFilter meshFilter;
        public void Start()
        {
            if (SemiFunc.IsMasterClientOrSingleplayer())
            {
                if (SemiFunc.IsMultiplayer())
                {
                    photonView.RPC("PickFilterRPC", RpcTarget.All, Random.Range(0, starMeshes.Length));
                }
                else
                {
                    PickFilterRPC(Random.Range(0, starMeshes.Length));
                }
            }
        }
        [PunRPC]
        public void PickFilterRPC(int index)
        {
            meshFilter.mesh = starMeshes[index];
        }
        public void AddPlayerBall()
        {
            List<PlayerAvatar> players = SemiFunc.PlayerGetAll();
            for (int i = 0; i < players.Count; i++)
            {
                if (players[i].isLocal)
                {
                    StatsUI.instance.Fetch();
                    StatsUI.instance.ShowStats();
                    CameraGlitch.Instance.PlayUpgrade();
                }
                GameDirector.instance.CameraImpact.ShakeDistance(5f, 1f, 6f, players[i].clientPosition, 0.2f);
                if (SemiFunc.IsMasterClientOrSingleplayer())
                {
                    players[i].playerHealth.MaterialEffectOverride(PlayerHealth.Effect.Upgrade);
                }
            }
            log.LogDebug("Adding a Dragon Ball point");
            Dictionary<string, int> dictionary = StatsManager.instance.dictionaryOfDictionaries["playerUpgradeDragonBalls"];
            for (int i = 0; i < players.Count; i++)
            {
                string steamID = SemiFunc.PlayerGetSteamID(players[i]);
                dictionary[steamID]++;
                if (dictionary[steamID] >= 7)
                {
                    if (SemiFunc.IsMasterClientOrSingleplayer())
                    {
                        RoundDirector.instance.totalHaul += Mathf.CeilToInt(Mathf.Max(10000, Mathf.Min(50000, (39000 / ((Mathf.Pow((float)dictionary.Count + 3f, 2f)) / 16f)) + 11000)) / (float)dictionary.Count) * 2;
                    }
                    dictionary[steamID] = 0;
                }
            }
            StatsManager.instance.dictionaryOfDictionaries["playerUpgradeDragonBalls"] = dictionary;
        }
    }
}