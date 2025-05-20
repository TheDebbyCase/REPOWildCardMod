using Photon.Pun;
using REPOLib.Modules;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine;
namespace REPOWildCardMod.Valuables
{
    public class DragonBall : MonoBehaviour
    {
        static readonly BepInEx.Logging.ManualLogSource log = WildCardMod.instance.log;
        public static List<DragonBall> levelDragonBalls;
        public PhotonView photonView;
        public PhysGrabObject physGrabObject;
        public Mesh[] starMeshes;
        public MeshFilter meshFilter;
        public Sound shenronApproach;
        public Sound shenronVoice;
        public Sound shenronWish;
        public GameObject hudAudio;
        public AudioClip spawnValuableClip;
        public int starNumber;
        public List<int> availableStars = new List<int>();
        public List<string> wishableUpgrades;
        public PlayerAvatar masterPlayer;
        public void Awake()
        {
            hudAudio = WildCardMod.instance.miscPrefabsList.Find((x) => x.name == "Wildcard HUD Audio");
            if (SemiFunc.IsMasterClientOrSingleplayer())
            {
                if (levelDragonBalls == null)
                {
                    levelDragonBalls = new List<DragonBall>() { this };
                }
                else
                {
                    levelDragonBalls.Add(this);
                }
                for (int i = 0; i < StatsManager.instance.dictionaryOfDictionaries["dragonBallsUnique"].Keys.Count; i++)
                {
                    if (StatsManager.instance.dictionaryOfDictionaries["dragonBallsUnique"][(i + 1).ToString()] == 0)
                    {
                        availableStars.Add(i + 1);
                    }
                }
            }
        }
        public void Start()
        {
            if (SemiFunc.IsMasterClientOrSingleplayer())
            {
                if (SemiFunc.IsMultiplayer())
                {
                    photonView.RPC("MasterIDRPC", RpcTarget.All, SemiFunc.PlayerAvatarLocal().photonView.ViewID);
                }
                else
                {
                    masterPlayer = SemiFunc.PlayerAvatarLocal();
                }
                for (int i = 0; i < levelDragonBalls.Count; i++)
                {
                    if (levelDragonBalls[i] == this)
                    {
                        continue;
                    }
                    else if (availableStars.Contains(levelDragonBalls[i].starNumber))
                    {
                        availableStars.Remove(levelDragonBalls[i].starNumber);
                    }
                }
                if (availableStars.Count == 0)
                {
                    physGrabObject.DestroyPhysGrabObject();
                    return;
                }
                int randomStars = availableStars[Random.Range(0, availableStars.Count)];
                if (SemiFunc.IsMultiplayer())
                {
                    ChooseStarsRPC(randomStars);
                    photonView.RPC("ChooseStarsRPC", RpcTarget.Others, randomStars);
                }
                else
                {
                    ChooseStarsRPC(randomStars);
                }
            }
        }
        [PunRPC]
        public void MasterIDRPC(int id)
        {
            masterPlayer = SemiFunc.PlayerAvatarGetFromPhotonID(id);
        }
        [PunRPC]
        public void ChooseStarsRPC(int index)
        {
            starNumber = index;
            meshFilter.mesh = starMeshes[index - 1];
        }
        public void Update()
        {
            shenronApproach.PlayLoop(StatsManager.instance.dictionaryOfDictionaries["playerUpgradeDragonBalls"][SemiFunc.PlayerGetSteamID(masterPlayer)] >= 6 && physGrabObject.grabbed, 2f, 0.5f);
        }
        [PunRPC]
        public void AllAddPlayerBallRPC()
        {
            log.LogDebug("Adding a Dragon Ball point");
            StatsUI.instance.Fetch();
            StatsUI.instance.ShowStats();
            CameraGlitch.Instance.PlayUpgrade();
            GameDirector.instance.CameraImpact.ShakeDistance(5f, 1f, 6f, SemiFunc.PlayerAvatarLocal().transform.position, 0.2f);
        }
        public void MasterAddPlayerBall()
        {
            if (SemiFunc.IsMultiplayer())
            {
                photonView.RPC("AllAddPlayerBallRPC", RpcTarget.All);
            }
            else
            {
                AllAddPlayerBallRPC();
            }
            string steamID = SemiFunc.PlayerGetSteamID(SemiFunc.PlayerAvatarLocal());
            StatsManager.instance.dictionaryOfDictionaries["playerUpgradeDragonBalls"][steamID]++;
            StatsManager.instance.dictionaryOfDictionaries["dragonBallsUnique"][starNumber.ToString()]++;
            if (StatsManager.instance.dictionaryOfDictionaries["playerUpgradeDragonBalls"][steamID] >= 7)
            {
                StatsManager.instance.dictionaryOfDictionaries["playerUpgradeDragonBalls"][steamID] = 0;
                StatsManager.instance.DictionaryFill("dragonBallsUnique", 0);
                DragonBallWish();
            }
            if (SemiFunc.IsMultiplayer())
            {
                photonView.RPC("PropogateBallsRPC", RpcTarget.Others, StatsManager.instance.dictionaryOfDictionaries["playerUpgradeDragonBalls"][steamID], steamID);
            }
            List<PlayerAvatar> players = SemiFunc.PlayerGetAll();
            for (int i = 0; i < players.Count; i++)
            {
                players[i].playerHealth.MaterialEffectOverride(PlayerHealth.Effect.Upgrade);
            }
        }
        public void DragonBallWish()
        {
            if (SemiFunc.IsMasterClientOrSingleplayer())
            {
                if (SemiFunc.IsMultiplayer())
                {
                    GameObject newAudio = PhotonNetwork.InstantiateRoomObject("Misc/Wildcard HUD Audio", Vector3.zero, Quaternion.identity);
                    GameObject newNewAudio = PhotonNetwork.InstantiateRoomObject("Misc/Wildcard HUD Audio", Vector3.zero, Quaternion.identity);
                    photonView.RPC("GenerateHUDElementsRPC", RpcTarget.All, newAudio.GetComponent<PhotonView>().ViewID, newNewAudio.GetComponent<PhotonView>().ViewID);
                }
                else
                {
                    GenerateHUDElementsRPC();
                }
            }
        }
        [PunRPC]
        public void GenerateHUDElementsRPC(int firstID = 0, int secondID = 0)
        {
            GameObject newAudio;
            GameObject newNewAudio;
            if (SemiFunc.IsMultiplayer())
            {
                newAudio = PhotonView.Find(firstID).gameObject;
                newNewAudio = PhotonView.Find(secondID).gameObject;
            }
            else
            {
                newAudio = Instantiate(hudAudio);
                newNewAudio = Instantiate(hudAudio);
            }
            shenronWish.Source = newAudio.GetComponent<AudioSource>();
            shenronVoice.Source = newNewAudio.GetComponent<AudioSource>();
            newAudio.transform.parent = PlayerController.instance.transform;
            newNewAudio.transform.parent = PlayerController.instance.transform;
            newAudio.transform.localPosition = Vector3.zero;
            newNewAudio.transform.localPosition = Vector3.zero;
            ShenronHUD shenronHUD = newAudio.AddComponent<ShenronHUD>();
            shenronHUD.shenronWish = new Sound { Source = shenronWish.Source, Sounds = shenronWish.Sounds, Type = shenronWish.Type, Volume = shenronWish.Volume, VolumeRandom = shenronWish.VolumeRandom, Pitch = shenronWish.Pitch, PitchRandom = shenronWish.PitchRandom, SpatialBlend = shenronWish.SpatialBlend, ReverbMix = shenronWish.ReverbMix, Doppler = shenronWish.Doppler };
            shenronHUD.shenronVoice = new Sound { Source = shenronVoice.Source, Sounds = shenronVoice.Sounds, Type = shenronVoice.Type, Volume = shenronVoice.Volume, VolumeRandom = shenronVoice.VolumeRandom, Pitch = shenronVoice.Pitch, PitchRandom = shenronVoice.PitchRandom, SpatialBlend = shenronVoice.SpatialBlend, ReverbMix = shenronVoice.ReverbMix, Doppler = shenronVoice.Doppler };
            shenronHUD.spawnValuableClip = spawnValuableClip;
            shenronHUD.photonView = newAudio.GetComponent<PhotonView>();
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
            if (SemiFunc.IsMasterClientOrSingleplayer())
            {
                DestroyBall();
            }
        }
        public void DestroyBall()
        {
            if (levelDragonBalls.Contains(this))
            {
                levelDragonBalls.Remove(this);
                if (SemiFunc.IsMultiplayer())
                {
                    physGrabObject.photonView.RPC("DestroyPhysGrabObjectRPC", RpcTarget.All);
                }
                else
                {
                    physGrabObject.DestroyPhysGrabObjectRPC();
                }
            }
        }
    }
    public class ShenronHUD : MonoBehaviour
    {
        static readonly BepInEx.Logging.ManualLogSource log = WildCardMod.instance.log;
        public PhotonView photonView;
        public float timer;
        public Sound shenronWish;
        public Sound shenronVoice;
        public bool voiceImpulse = true;
        public bool upgradesGiven = false;
        public List<string> wishableUpgrades;
        public AudioClip spawnValuableClip;
        public int valuableType;
        public int[] randomAmounts = new int[] { Random.Range(15, 21), Random.Range(9, 13), Random.Range(6, 10), Random.Range(3, 7) };
        public GameObject chosenValuable;
        public float valuableTimerInterval = -1f;
        public float valuableTimer = 0f;
        public Vector3 valuableSpawnPosition;
        public int spawnTimes = 0;
        public Color emissionColor = new Color(0.65f, 0.65f, 0f);
        public void Start()
        {
            timer = 15f;
            shenronWish.LowPassIgnoreColliders.Add(PlayerController.instance.col);
            shenronVoice.LowPassIgnoreColliders.Add(PlayerController.instance.col);
            wishableUpgrades = StatsManager.instance.FetchPlayerUpgrades(SemiFunc.PlayerGetSteamID(SemiFunc.PlayerAvatarLocal())).Keys.ToList();
            wishableUpgrades.Remove("Dragon Balls");
            wishableUpgrades.Remove("Chaos Emeralds");
            wishableUpgrades.Remove("Map Player Count");
            wishableUpgrades.Remove("Throw");
            wishableUpgrades.Remove("Head Charge");
            wishableUpgrades.Remove("Head Power");
            for (int i = 0; i < wishableUpgrades.Count; i++)
            {
                log.LogDebug($"Dragon Ball Wishable Upgrade {i}: \"{wishableUpgrades[i]}\"");
            }
        }
        public void Update()
        {
            timer -= Time.deltaTime;
            shenronWish.PlayLoop(timer > 5f, 2f, 0.5f);
            if (!SemiFunc.RunIsLevel() && timer > 5f)
            {
                timer = 5f;
            }
            if (SemiFunc.IsMultiplayer())
            {
                List<PlayerAvatar> players = SemiFunc.PlayerGetAll();
                for (int i = 0; i < players.Count; i++)
                {
                    players[i].playerHealth.bodyMaterial.SetColor("_EmissionColor", Color.Lerp(Color.black, emissionColor, Mathf.Clamp01((Mathf.Pow(0.5f - ((timer - 2.5f) / 12.5f), 2f) * -4f) + 1f)));
                }
            }
            if (valuableTimer > 0f && spawnTimes < randomAmounts[valuableType])
            {
                valuableTimer -= Time.deltaTime;
                if (valuableTimer <= 0f)
                {
                    SpawnValuable();
                    spawnTimes++;
                    valuableTimer = valuableTimerInterval;
                }
            }
            if (timer <= 14f)
            {
                if (voiceImpulse)
                {
                    voiceImpulse = false;
                    shenronVoice.Play(transform.position);
                }
                if (timer <= 10f)
                {
                    if (SemiFunc.IsMultiplayer())
                    {
                        PlayerAvatar.instance.voiceChat.OverridePitch(0.8f, 1f, 0.25f);
                    }
                    if (!upgradesGiven)
                    {
                        upgradesGiven = true;
                        StatsUI.instance.Fetch();
                        StatsUI.instance.ShowStats();
                        CameraGlitch.Instance.PlayUpgrade();
                        GameDirector.instance.CameraImpact.ShakeDistance(5f, 1f, 6f, SemiFunc.PlayerAvatarLocal().transform.position, 0.2f);
                        TryUpgrade();
                        TryValuableReward();
                    }
                    CurrencyUI.instance.Show();
                    if (timer <= 0f)
                    {
                        Destroy(this.gameObject);
                    }
                }
            }
        }
        public void SpawnValuable()
        {
            if (SemiFunc.IsMultiplayer())
            {
                PhotonNetwork.InstantiateRoomObject(ResourcesHelper.GetValuablePrefabPath(chosenValuable), valuableSpawnPosition, Random.rotationUniform);
                photonView.RPC("SpawnValuableRPC", RpcTarget.All, valuableSpawnPosition);
            }
            else
            {
                Instantiate(chosenValuable, valuableSpawnPosition, Random.rotationUniform);
                SpawnValuableRPC(valuableSpawnPosition);
            }
        }
        [PunRPC]
        public void SpawnValuableRPC(Vector3 spawnPos)
        {
            Instantiate(AssetManager.instance.prefabTeleportEffect, spawnPos, Quaternion.identity).transform.localScale = Vector3.one * 2f;
            shenronVoice.Play(transform.position);
        }
        public void TryValuableReward()
        {
            if (RoundDirector.instance.allExtractionPointsCompleted)
            {
                SemiFunc.StatSetRunCurrency(SemiFunc.StatGetRunCurrency() + 20);
                SemiFunc.StatSetRunTotalHaul(SemiFunc.StatGetRunTotalHaul() + 20);
                CurrencyUI.instance.FetchCurrency();
                if (SemiFunc.IsMasterClientOrSingleplayer())
                {
                    RoundDirector.instance.totalHaul += 20000;
                }
            }
            else
            {
                shenronVoice.Sounds = new AudioClip[] { spawnValuableClip };
                shenronVoice.VolumeRandom = 0.1f;
                shenronVoice.PitchRandom = 0.15f;
                if (SemiFunc.IsMasterClientOrSingleplayer())
                {
                    valuableType = Random.Range(0, 4);
                    valuableSpawnPosition = SemiFunc.LevelPointsGetClosestToPlayer().transform.position + (Vector3.up * 1.5f);
                    LevelValuables levelPool = LevelGenerator.Instance.Level.ValuablePresets[Random.Range(0, LevelGenerator.Instance.Level.ValuablePresets.Count)];
                    List<GameObject> valuablePool = null;
                    switch (valuableType)
                    {
                        case 0:
                            {
                                valuablePool = levelPool.tiny;
                                break;
                            }
                        case 1:
                            {
                                valuablePool = levelPool.small;
                                break;
                            }
                        case 2:
                            {
                                valuablePool = levelPool.medium;
                                break;
                            }
                        case 3:
                            {
                                valuablePool = levelPool.big;
                                break;
                            }
                    }
                    valuableTimerInterval = 5f / randomAmounts[valuableType];
                    valuablePool.RemoveAll(x => x.name == "Valuable Dragon Ball" || x.name == "Valuable Dummy Item Smith Note");
                    chosenValuable = valuablePool[Random.Range(0, valuablePool.Count)];
                    valuableTimer = valuableTimerInterval;
                    log.LogDebug($"Dragon Ball Wish spawning {randomAmounts[valuableType]} {chosenValuable.name}s!");
                }
            }
        }
        public void TryUpgrade()
        {
            if (SemiFunc.IsMasterClientOrSingleplayer())
            {
                if (SemiFunc.IsMultiplayer())
                {
                    List<string> upgrades = wishableUpgrades;
                    List<PlayerAvatar> players = SemiFunc.PlayerGetAll();
                    for (int i = 0; i < players.Count; i++)
                    {
                        players[i].playerHealth.MaterialEffectOverride(PlayerHealth.Effect.Upgrade);
                        if (upgrades.Count == 0)
                        {
                            upgrades = wishableUpgrades;
                        }
                        if (upgrades.Count == 0)
                        {
                            log.LogWarning("Something went wrong with the Dragon Balls upgrades system, no upgrades were available!");
                            return;
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
                        if (WildCardMod.instance.oldSharedUpgradesPresent)
                        {
                            List<PlayerAvatar> players = SemiFunc.PlayerGetAll();
                            for (int i = 0; i < players.Count; i++)
                            {
                                for (int j = 0; j < 6; j++)
                                {
                                    PunManager.instance.UpgradePlayerHealth(SemiFunc.PlayerGetSteamID(players[i]));
                                }
                            }
                            break;
                        }
                        for (int i = 0; i < 6; i++)
                        {
                            PunManager.instance.UpgradePlayerHealth(steamID);
                        }
                        break;
                    }
                case "Stamina":
                    {
                        if (WildCardMod.instance.oldSharedUpgradesPresent)
                        {
                            List<PlayerAvatar> players = SemiFunc.PlayerGetAll();
                            for (int i = 0; i < players.Count; i++)
                            {
                                for (int j = 0; j < 6; j++)
                                {
                                    PunManager.instance.UpgradePlayerEnergy(SemiFunc.PlayerGetSteamID(players[i]));
                                }
                            }
                            break;
                        }
                        for (int i = 0; i < 6; i++)
                        {
                            PunManager.instance.UpgradePlayerEnergy(steamID);
                        }
                        break;
                    }
                case "Launch":
                    {
                        if (WildCardMod.instance.oldSharedUpgradesPresent)
                        {
                            List<PlayerAvatar> players = SemiFunc.PlayerGetAll();
                            for (int i = 0; i < players.Count; i++)
                            {
                                for (int j = 0; j < 6; j++)
                                {
                                    PunManager.instance.UpgradePlayerTumbleLaunch(SemiFunc.PlayerGetSteamID(players[i]));
                                }
                            }
                            break;
                        }
                        for (int i = 0; i < 6; i++)
                        {
                            PunManager.instance.UpgradePlayerTumbleLaunch(steamID);
                        }
                        break;
                    }
                case "Speed":
                    {
                        if (WildCardMod.instance.oldSharedUpgradesPresent)
                        {
                            List<PlayerAvatar> players = SemiFunc.PlayerGetAll();
                            for (int i = 0; i < players.Count; i++)
                            {
                                for (int j = 0; j < 6; j++)
                                {
                                    PunManager.instance.UpgradePlayerSprintSpeed(SemiFunc.PlayerGetSteamID(players[i]));
                                }
                            }
                            break;
                        }
                        for (int i = 0; i < 6; i++)
                        {
                            PunManager.instance.UpgradePlayerSprintSpeed(steamID);
                        }
                        break;
                    }
                case "Strength":
                    {
                        if (WildCardMod.instance.oldSharedUpgradesPresent)
                        {
                            List<PlayerAvatar> players = SemiFunc.PlayerGetAll();
                            for (int i = 0; i < players.Count; i++)
                            {
                                for (int j = 0; j < 6; j++)
                                {
                                    PunManager.instance.UpgradePlayerGrabStrength(SemiFunc.PlayerGetSteamID(players[i]));
                                }
                            }
                            break;
                        }
                        for (int i = 0; i < 6; i++)
                        {
                            PunManager.instance.UpgradePlayerGrabStrength(steamID);
                        }
                        break;
                    }
                case "Range":
                    {
                        if (WildCardMod.instance.oldSharedUpgradesPresent)
                        {
                            List<PlayerAvatar> players = SemiFunc.PlayerGetAll();
                            for (int i = 0; i < players.Count; i++)
                            {
                                for (int j = 0; j < 6; j++)
                                {
                                    PunManager.instance.UpgradePlayerGrabRange(SemiFunc.PlayerGetSteamID(players[i]));
                                }
                            }
                            break;
                        }
                        for (int i = 0; i < 6; i++)
                        {
                            PunManager.instance.UpgradePlayerGrabRange(steamID);
                        }
                        break;
                    }
                case "Extra Jump":
                    {
                        if (WildCardMod.instance.oldSharedUpgradesPresent)
                        {
                            List<PlayerAvatar> players = SemiFunc.PlayerGetAll();
                            for (int i = 0; i < players.Count; i++)
                            {
                                for (int j = 0; j < 6; j++)
                                {
                                    PunManager.instance.UpgradePlayerExtraJump(SemiFunc.PlayerGetSteamID(players[i]));
                                }
                            }
                            break;
                        }
                        for (int i = 0; i < 6; i++)
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
                            log.LogWarning($"Dragon Ball wish for upgrade: {upgrade} failed, retrying...");
                            wishableUpgrades.Remove(upgrade);
                            if (wishableUpgrades.Count == 0)
                            {
                                wishableUpgrades = StatsManager.instance.FetchPlayerUpgrades(steamID).Keys.ToList();
                                wishableUpgrades.Remove("Dragon Balls");
                                wishableUpgrades.Remove("Chaos Emeralds");
                                wishableUpgrades.Remove("Map Player Count");
                                wishableUpgrades.Remove("Throw");
                                wishableUpgrades.Remove("Head Charge");
                                wishableUpgrades.Remove("Head Power");
                            }
                            MegaUpgrade(steamID, wishableUpgrades[Random.Range(0, wishableUpgrades.Count)]);
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
            if (WildCardMod.instance.oldSharedUpgradesPresent)
            {
                List<PlayerAvatar> players = SemiFunc.PlayerGetAll();
                for (int i = 0; i < players.Count; i++)
                {
                    MoreUpgrades.Classes.MoreUpgradesManager.instance.Upgrade(upgrade, SemiFunc.PlayerGetSteamID(players[i]), 5);
                }
                return;
            }
            MoreUpgrades.Classes.MoreUpgradesManager.instance.Upgrade(upgrade, steamID, 5);
        }
        public void OnDestroy()
        {
            List<PlayerAvatar> players = SemiFunc.PlayerGetAll();
            for (int i = 0; i < players.Count; i++)
            {
                if (players[i].playerHealth.bodyMaterial.GetColor("_EmissionColor") != Color.black)
                {
                    players[i].playerHealth.bodyMaterial.SetColor("_EmissionColor", Color.black);
                }
            }
        }
    }
}