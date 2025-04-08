using Photon.Pun;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
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
        public List<string> upgradeKeys;
        public void Start()
        {
            upgradeKeys = StatsManager.instance.dictionaryOfDictionaries.Keys.ToList();
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
                wishableUpgrades.Remove("Extra Jump");
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
                        for (int j = 0; j < upgrades.Count; j++)
                        {
                            if (j >= upgrades.Count)
                            {
                                break;
                            }
                            if (StatsManager.instance.dictionaryOfDictionaries[upgradeKeys.Find((x) => x.ToLower().Contains(upgrades[j].Trim().ToLower()))][SemiFunc.PlayerGetSteamID(players[i])] >= StatsManager.instance.itemDictionary[StatsManager.instance.itemDictionary.Keys.ToList().Find((y) => y.Contains(upgrades[j]))].maxAmount)
                            {
                                upgrades.RemoveAt(j);
                            }
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
            int max = StatsManager.instance.itemDictionary[StatsManager.instance.itemDictionary.Keys.ToList().Find((y) => y.Contains(upgrade))].maxAmount - StatsManager.instance.dictionaryOfDictionaries[upgradeKeys.Find((x) => x.ToLower().Contains(upgrade.Trim().ToLower()))][steamID];
            log.LogDebug($"Dragon Ball Max Buy Number: {max}");
            switch (upgrade)
            {
                case "Health":
                    {
                        StatsManager.instance.playerUpgradeHealth[steamID] += Mathf.Min(10, max) - 1;
                        PunManager.instance.UpgradePlayerHealth(steamID);
                        break;
                    }
                case "Stamina":
                    {
                        StatsManager.instance.playerUpgradeStamina[steamID] += Mathf.Min(10, max) - 1;
                        PunManager.instance.UpgradePlayerEnergy(steamID);
                        break;
                    }
                case "Launch":
                    {
                        StatsManager.instance.playerUpgradeLaunch[steamID] += Mathf.Min(10, max) - 1;
                        PunManager.instance.UpgradePlayerTumbleLaunch(steamID);
                        break;
                    }
                case "Speed":
                    {
                        StatsManager.instance.playerUpgradeSpeed[steamID] += Mathf.Min(10, max) - 1;
                        PunManager.instance.UpgradePlayerSprintSpeed(steamID);
                        break;
                    }
                case "Strength":
                    {
                        StatsManager.instance.playerUpgradeStrength[steamID] += Mathf.Min(10, max) - 1;
                        PunManager.instance.UpgradePlayerGrabStrength(steamID);
                        break;
                    }
                case "Range":
                    {
                        StatsManager.instance.playerUpgradeRange[steamID] += Mathf.Min(10, max) - 1;
                        PunManager.instance.UpgradePlayerGrabRange(steamID);
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
            MoreUpgrades.Classes.UpgradeItem upgradeItem = MoreUpgrades.Plugin.instance.upgradeItems.Find((x) => x.name == upgrade || x.name == new string(upgrade.Where((char y) => !char.IsWhiteSpace(y)).ToArray()));
            if (upgradeItem == null)
            {
                log.LogWarning($"Dragon Ball wish upgrade tried to use MoreUpgrades' \"{upgrade}\" but something went wrong");
                return;
            }
            MoreUpgrades.Classes.MoreUpgradesManager.instance.Upgrade(new string(upgrade.Where((char x) => !char.IsWhiteSpace(x)).ToArray()), steamID, Mathf.Min(10, upgradeItem.upgradeItemBase.maxPurchaseAmount - upgradeItem.playerUpgrades[steamID]));
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