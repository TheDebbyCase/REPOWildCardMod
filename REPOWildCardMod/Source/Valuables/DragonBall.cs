using Photon.Pun;
using Steamworks;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
namespace REPOWildCardMod.Valuables
{
    public class DragonBall : MonoBehaviour
    {
        static readonly BepInEx.Logging.ManualLogSource log = WildCardMod.instance.log;
        public PhotonView photonView;
        public Mesh[] starMeshes;
        public MeshFilter meshFilter;
        public List<string> wishableUpgrades;
        public void Start()
        {
            if (SemiFunc.IsMasterClientOrSingleplayer())
            {
                if (SemiFunc.IsMultiplayer())
                {
                    photonView.RPC("ChooseStarsRPC", RpcTarget.All, Random.Range(0, starMeshes.Length));
                }
                else
                {
                    ChooseStarsRPC(Random.Range(0, starMeshes.Length));
                }
                wishableUpgrades = StatsManager.instance.FetchPlayerUpgrades(SemiFunc.PlayerGetSteamID(SemiFunc.PlayerAvatarLocal())).Keys.ToList();
                wishableUpgrades.Remove("playerUpgradeDragonBalls");
                wishableUpgrades.Remove("playerUpgradeChaosEmeralds");
                wishableUpgrades.Remove("playerUpgradeExtraJump");
                wishableUpgrades.Remove("playerUpgradeMapPlayerCount");
                wishableUpgrades.Remove("playerUpgradeThrow");
            }
        }
        [PunRPC]
        public void ChooseStarsRPC(int index)
        {
            meshFilter.mesh = starMeshes[index];
        }
        public void AddPlayerBall()
        {
            log.LogDebug("Adding a Dragon Ball point");
            if (SemiFunc.IsMasterClientOrSingleplayer())
            {
                string steamID = SemiFunc.PlayerGetSteamID(SemiFunc.PlayerAvatarLocal());
                StatsManager.instance.dictionaryOfDictionaries["playerUpgradeDragonBalls"][steamID]++;
                if (StatsManager.instance.dictionaryOfDictionaries["playerUpgradeDragonBalls"][steamID] >= 7)
                {
                    StatsManager.instance.dictionaryOfDictionaries["playerUpgradeDragonBalls"][steamID] = 0;
                    DragonBallWish();
                }
                if (SemiFunc.IsMultiplayer())
                {
                    photonView.RPC("PropogateBallsRPC", RpcTarget.Others, StatsManager.instance.dictionaryOfDictionaries["playerUpgradeDragonBalls"][steamID], steamID);
                }
            }
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
        }
        public void DragonBallWish()
        {
            if (SemiFunc.IsMasterClientOrSingleplayer())
            {
                if (SemiFunc.IsMultiplayer())
                {
                    List<string> upgrades = wishableUpgrades;
                    List<PlayerAvatar> players = SemiFunc.PlayerGetAll();
                    for (int i = 0; i < players.Count; i++)
                    {
                        if (upgrades.Count == 0)
                        {
                            upgrades = wishableUpgrades;
                        }
                        int randomIndex = Random.Range(0, upgrades.Count);
                        MegaUpgrade(SemiFunc.PlayerGetSteamID(players[i]), upgrades[randomIndex]);
                        upgrades.RemoveAt(randomIndex);
                    }
                }
                else
                {
                    MegaUpgrade(SemiFunc.PlayerGetSteamID(SemiFunc.PlayerAvatarLocal()), wishableUpgrades[Random.Range(0, wishableUpgrades.Count)]);
                }
            }
        }
        public void MegaUpgrade(string steamID, string upgrade)
        {
            switch (upgrade.Replace("playerUpgrade", ""))
            {
                case "Health":
                    {
                        StatsManager.instance.playerUpgradeHealth[steamID] += 9;
                        PunManager.instance.UpgradePlayerHealth(steamID);
                        break;
                    }
                case "Stamina":
                    {
                        StatsManager.instance.playerUpgradeStamina[steamID] += 9;
                        PunManager.instance.UpgradePlayerEnergy(steamID);
                        break;
                    }
                case "Launch":
                    {
                        StatsManager.instance.playerUpgradeLaunch[steamID] += 9;
                        PunManager.instance.UpgradePlayerTumbleLaunch(steamID);
                        break;
                    }
                case "Speed":
                    {
                        StatsManager.instance.playerUpgradeSpeed[steamID] += 9;
                        PunManager.instance.UpgradePlayerSprintSpeed(steamID);
                        break;
                    }
                case "Strength":
                    {
                        StatsManager.instance.playerUpgradeStrength[steamID] += 9;
                        PunManager.instance.UpgradePlayerGrabStrength(steamID);
                        break;
                    }
                case "Range":
                    {
                        StatsManager.instance.playerUpgradeRange[steamID] += 9;
                        PunManager.instance.UpgradePlayerGrabRange(steamID);
                        break;
                    }
                default:
                    {
                        if (SemiFunc.IsMultiplayer())
                        {
                            photonView.RPC("MegaUpgradeMiscRPC", RpcTarget.All, upgrade, steamID);
                        }
                        else
                        {
                            MegaUpgradeMiscRPC(upgrade, steamID);
                        }
                        break;
                    }
            }
        }
        [PunRPC]
        public void MegaUpgradeMiscRPC(string upgrade, string steamID)
        {
            try
            {
                StatsManager.instance.dictionaryOfDictionaries[upgrade][steamID] += 10;
            }
            catch
            {
                log.LogError($"Attempted to upgrade \"{upgrade}\" on player \"{SemiFunc.PlayerAvatarGetFromSteamID(steamID).playerName}\" but something went wrong!");
            }
        }
        [PunRPC]
        public void PropogateBallsRPC(int balls, string masterID)
        {
            StatsManager.instance.dictionaryOfDictionaries["playerUpgradeDragonBalls"][masterID] = balls;
            string localSteamID = SemiFunc.PlayerGetSteamID(SemiFunc.PlayerAvatarLocal());
            if (balls != StatsManager.instance.dictionaryOfDictionaries["playerUpgradeDragonBalls"][localSteamID])
            {
                photonView.RPC("SetBallsRPC", RpcTarget.All, localSteamID, balls);
            }
        }
        [PunRPC]
        public void SetBallsRPC(string steamID, int balls)
        {
            StatsManager.instance.dictionaryOfDictionaries["playerUpgradeDragonBalls"][steamID] = balls;
        }
    }
}