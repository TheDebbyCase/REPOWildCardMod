using Photon.Pun;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI;
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
                wishableUpgrades.Remove("Dragon Balls");
                wishableUpgrades.Remove("Chaos Emeralds");
                wishableUpgrades.Remove("Map Player Count");
                wishableUpgrades.Remove("Throw");
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
            log.LogDebug($"Dragon Ball Mega Upgrading: \"{upgrade}\"");
            switch (upgrade)
            {
                case "Health":
                    {
                        for (int i = 0; i < 11; i++)
                        {
                            PunManager.instance.UpgradePlayerHealth(steamID);
                        }
                        break;
                    }
                case "Stamina":
                    {
                        for (int i = 0; i < 11; i++)
                        {
                            PunManager.instance.UpgradePlayerEnergy(steamID);
                        }
                        break;
                    }
                case "Launch":
                    {
                        for (int i = 0; i < 11; i++)
                        {
                            PunManager.instance.UpgradePlayerTumbleLaunch(steamID);
                        }
                        break;
                    }
                case "Speed":
                    {
                        for (int i = 0; i < 11; i++)
                        {
                            PunManager.instance.UpgradePlayerSprintSpeed(steamID);
                        }
                        break;
                    }
                case "Strength":
                    {
                        for (int i = 0; i < 11; i++)
                        {
                            PunManager.instance.UpgradePlayerGrabStrength(steamID);
                        }
                        break;
                    }
                case "Range":
                    {
                        for (int i = 0; i < 11; i++)
                        {
                            PunManager.instance.UpgradePlayerGrabRange(steamID);
                        }
                        break;
                    }
                case "Extra Jump":
                    {
                        for (int i = 0; i < 11; i++)
                        {
                            PunManager.instance.UpgradePlayerExtraJump(steamID);
                        }                        
                        break;
                    }
                default:
                    {
                        if (WildCardMod.instance.moreUpgradesPresent)
                        {
                            MoreUpgradesUpgrade(steamID, upgrade);
                        }
                        else
                        {
                            log.LogWarning($"Dragon Ball wish for upgrade: {upgrade} failed");
                        }
                        break;
                    }
            }
        }
        [MethodImpl(MethodImplOptions.NoInlining)]
        public void MoreUpgradesUpgrade(string steamID, string upgrade)
        {
            if (MoreUpgrades.Plugin.instance.upgradeItems.Find((x) => x.name == upgrade) == null)
            {
                log.LogWarning($"Dragon Ball wish upgrade tried to use MoreUpgrades' \"{upgrade}\" but something went wrong");
                return;
            }
            log.LogDebug($"Using MoreUpgrades to upgrade \"{upgrade}\"!");
            MoreUpgrades.Classes.MoreUpgradesManager.instance.Upgrade(upgrade, steamID, 10);
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