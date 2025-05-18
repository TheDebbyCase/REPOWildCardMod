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
        public void AddPlayerBall()
        {
            log.LogDebug("Adding a Dragon Ball point");
            StatsUI.instance.Fetch();
            StatsUI.instance.ShowStats();
            CameraGlitch.Instance.PlayUpgrade();
            GameDirector.instance.CameraImpact.ShakeDistance(5f, 1f, 6f, SemiFunc.PlayerAvatarLocal().transform.position, 0.2f);
            if (SemiFunc.IsMasterClientOrSingleplayer())
            {
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
        }
        public void DragonBallWish()
        {
            if (SemiFunc.IsMasterClientOrSingleplayer())
            {
                if (SemiFunc.IsMultiplayer())
                {
                    photonView.RPC("GenerateHUDElementsRPC", RpcTarget.All);
                }
                else
                {
                    GenerateHUDElementsRPC();
                }
            }
        }
        [PunRPC]
        public void GenerateHUDElementsRPC()
        {
            GameObject newAudio = Instantiate(hudAudio);
            GameObject newNewAudio = Instantiate(hudAudio);
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
        public void OnDestroy()
        {
            if (SemiFunc.IsMasterClientOrSingleplayer() && levelDragonBalls.Contains(this))
            {
                levelDragonBalls.Remove(this);
            }
        }
    }
    public class ShenronHUD : MonoBehaviour
    {
        static readonly BepInEx.Logging.ManualLogSource log = WildCardMod.instance.log;
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
            shenronVoice.Play(transform.position);
            if (SemiFunc.IsMasterClientOrSingleplayer())
            {
                if (SemiFunc.IsMultiplayer())
                {
                    PhotonNetwork.InstantiateRoomObject(ResourcesHelper.GetValuablePrefabPath(chosenValuable), valuableSpawnPosition, Random.rotationUniform);
                }
                else
                {
                    Instantiate(chosenValuable, valuableSpawnPosition, Random.rotationUniform);
                }
            }
            Instantiate(AssetManager.instance.prefabTeleportEffect, valuableSpawnPosition, Quaternion.identity).transform.localScale = Vector3.one * 2f;
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
                shenronVoice.VolumeRandom = 0.2f;
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
                    valuablePool.RemoveAll(x => x.name == "Valuable Dragon Ball" || x.name == "Valuable Dummy Item Smith Note" || x.name == "Valuable Alpharad Dice");
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
                            log.LogWarning($"Dragon Ball wish for upgrade: {upgrade} failed, retrying...");
                            wishableUpgrades.Remove(upgrade);
                            TryUpgrade();
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