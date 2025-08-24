using Photon.Pun;
using REPOLib.Modules;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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
        public int extractFinishedPlayers = 0;
        public int playersBallsSet = 0;
        public Coroutine extractCoroutine;
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
            if (!LevelGenerator.Instance.Generated)
            {
                return;
            }
            if (masterPlayer != null)
            {
                shenronApproach.PlayLoop(StatsManager.instance.dictionaryOfDictionaries["playerUpgradeDragonBalls"][SemiFunc.PlayerGetSteamID(masterPlayer)] >= 6 && physGrabObject.grabbed, 2f, 0.5f);
            }
        }
        [PunRPC]
        public void AllAddPlayerBallRPC()
        {
            log.LogDebug("Adding a Dragon Ball point");
            playersBallsSet = 0;
            StatsUI.instance.Fetch();
            StatsUI.instance.ShowStats();
            CameraGlitch.Instance.PlayUpgrade();
            GameDirector.instance.CameraImpact.ShakeDistance(5f, 1f, 6f, SemiFunc.PlayerAvatarLocal().transform.position, 0.2f);
        }
        [PunRPC]
        public void ExtractCompleteRPC()
        {
            extractFinishedPlayers += 1;
            log.LogDebug($"extractFinishedPlayers == {extractFinishedPlayers}");
            if (extractCoroutine == null)
            {
                extractCoroutine = StartCoroutine(ExtractCompleteCoroutine());
            }
        }
        public IEnumerator ExtractCompleteCoroutine()
        {
            yield return new WaitUntil(() => extractFinishedPlayers >= SemiFunc.PlayerGetList().Count);
            log.LogDebug("Calling DestroyBall()");
            DestroyBall();
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
            List<PlayerAvatar> players = SemiFunc.PlayerGetAll();
            for (int i = 0; i < players.Count; i++)
            {
                players[i].playerHealth.MaterialEffectOverride(PlayerHealth.Effect.Upgrade);
            }
            if (SemiFunc.IsMultiplayer())
            {
                photonView.RPC("PropogateBallsRPC", RpcTarget.All, StatsManager.instance.dictionaryOfDictionaries["playerUpgradeDragonBalls"][steamID]);
            }
            ExtractCompleteRPC();
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
            if (SemiFunc.PlayerAvatarLocal().spectating)
            {
                newAudio.transform.parent = SpectateCamera.instance.transform;
                newNewAudio.transform.parent = SpectateCamera.instance.transform;
            }
            else
            {
                newAudio.transform.parent = PlayerController.instance.transform;
                newNewAudio.transform.parent = PlayerController.instance.transform;
            }
            newAudio.transform.localPosition = Vector3.zero;
            newNewAudio.transform.localPosition = Vector3.zero;
            ShenronHUD shenronHUD = newAudio.AddComponent<ShenronHUD>();
            shenronHUD.shenronWish = new Sound { Source = shenronWish.Source, Sounds = shenronWish.Sounds, Type = shenronWish.Type, Volume = shenronWish.Volume, VolumeRandom = shenronWish.VolumeRandom, Pitch = shenronWish.Pitch, PitchRandom = shenronWish.PitchRandom, SpatialBlend = shenronWish.SpatialBlend, ReverbMix = shenronWish.ReverbMix, Doppler = shenronWish.Doppler };
            shenronHUD.shenronVoice = new Sound { Source = shenronVoice.Source, Sounds = shenronVoice.Sounds, Type = shenronVoice.Type, Volume = shenronVoice.Volume, VolumeRandom = shenronVoice.VolumeRandom, Pitch = shenronVoice.Pitch, PitchRandom = shenronVoice.PitchRandom, SpatialBlend = shenronVoice.SpatialBlend, ReverbMix = shenronVoice.ReverbMix, Doppler = shenronVoice.Doppler };
            shenronHUD.spawnValuableClip = spawnValuableClip;
            shenronHUD.photonView = newAudio.GetComponent<PhotonView>();
        }
        [PunRPC]
        public void PropogateBallsRPC(int balls)
        {
            List<PlayerAvatar> players = SemiFunc.PlayerGetAll();
            for (int i = 0; i < players.Count; i++)
            {
                StatsManager.instance.dictionaryOfDictionaries["playerUpgradeDragonBalls"][SemiFunc.PlayerGetSteamID(players[i])] = balls;
            }
            photonView.RPC("ExtractCompleteRPC", RpcTarget.MasterClient);
        }
        public void DestroyBall()
        {
            if (SemiFunc.IsMultiplayer())
            {
                physGrabObject.photonView.RPC("DestroyPhysGrabObjectRPC", RpcTarget.All);
            }
            else
            {
                physGrabObject.DestroyPhysGrabObjectRPC();
            }
        }
        public void OnDestroy()
        {
            if (levelDragonBalls.Contains(this))
            {
                levelDragonBalls.Remove(this);
            }
        }
    }
    public class ShenronHUD : MonoBehaviour
    {
        static readonly BepInEx.Logging.ManualLogSource log = WildCardMod.instance.log;
        public static List<string> wishBlacklist = new List<string>() { "Dragon Balls", "Chaos Emeralds", "Map Player Count", "Throw", "Head Charge", "Head Power" };
        public static string[] vanillaUpgrades = new string[] { "Health", "Stamina", "Launch", "Speed", "Strength", "Range", "Extra Jump", "Crouch Rest", "Tumble Wings" };
        public int[] randomAmounts = new int[] { Random.Range(Mathf.Max(0, WildCardMod.instance.ModConfig.wishValuableAmounts[0].Value), Mathf.Max(1, WildCardMod.instance.ModConfig.wishValuableAmounts[0].Value + 1, WildCardMod.instance.ModConfig.wishValuableAmounts[1].Value + 1)), Random.Range(Mathf.Max(0, WildCardMod.instance.ModConfig.wishValuableAmounts[2].Value), Mathf.Max(1, WildCardMod.instance.ModConfig.wishValuableAmounts[2].Value + 1, WildCardMod.instance.ModConfig.wishValuableAmounts[3].Value + 1)), Random.Range(Mathf.Max(0, WildCardMod.instance.ModConfig.wishValuableAmounts[4].Value), Mathf.Max(1, WildCardMod.instance.ModConfig.wishValuableAmounts[4].Value + 1, WildCardMod.instance.ModConfig.wishValuableAmounts[5].Value + 1)), Random.Range(Mathf.Max(0, WildCardMod.instance.ModConfig.wishValuableAmounts[6].Value), Mathf.Max(1, WildCardMod.instance.ModConfig.wishValuableAmounts[6].Value + 1, WildCardMod.instance.ModConfig.wishValuableAmounts[7].Value + 1)) };
        public PhotonView photonView;
        public float timer;
        public Sound shenronWish;
        public Sound shenronVoice;
        public bool voiceImpulse = true;
        public bool upgradesGiven = false;
        public List<string> wishableUpgrades;
        public Dictionary<string, WishUpgrade> upgradesInfo = new Dictionary<string, WishUpgrade>();
        public AudioClip spawnValuableClip;
        public int valuableType;
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
            wishBlacklist.AddRange(WildCardMod.instance.ModConfig.wishBlacklist.Value.Replace(" ", "").Split(","));
            if (WildCardMod.instance.ModConfig.vanillaOnlyUpgrades.Value)
            {
                wishBlacklist.AddRange(wishableUpgrades.Where((x) => !vanillaUpgrades.Contains(x)));
            }
            wishableUpgrades.RemoveAll(wishBlacklist.Contains);
            List<PlayerAvatar> players = SemiFunc.PlayerGetAll();
            for (int i = 0; i < wishableUpgrades.Count; i++)
            {
                log.LogDebug($"Dragon Ball Wishable Upgrade {i}: \"{wishableUpgrades[i]}\"");
                string[] valueString = WildCardMod.instance.ModConfig.wishMaxPerDict[wishableUpgrades[i]].Value.Replace(" ", "").Split(",");
                if (valueString.Length == 2 && int.TryParse(valueString[0], out int cap) && int.TryParse(valueString[1], out int per))
                {
                    WishUpgrade newUpgradeInfo = new WishUpgrade(cap, per);
                    for (int j = 0; j < players.Count; j++)
                    {
                        newUpgradeInfo.playersAtCap.Add(players[j].steamID, false);
                    }
                    upgradesInfo.Add(wishableUpgrades[i], newUpgradeInfo);
                }
                else
                {
                    log.LogWarning($"{wishableUpgrades[i]} Upgrade Wish config was set up wrong!");
                }
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
                    float lerp = Mathf.Clamp01((Mathf.Pow(0.5f - ((timer - 2.5f) / 12.5f), 2f) * -4f) + 1f);
                    players[i].playerHealth.bodyMaterial.SetColor("_EmissionColor", Color.Lerp(Color.black, emissionColor, lerp));
                    players[i].playerDeathHead.headRenderer.material.SetColor("_EmissionColor", Color.Lerp(Color.black, emissionColor, lerp));
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
                int currencyToAdd = Mathf.Max(0, WildCardMod.instance.ModConfig.wishCurrencyAmount.Value);
                SemiFunc.StatSetRunCurrency(SemiFunc.StatGetRunCurrency() + currencyToAdd);
                SemiFunc.StatSetRunTotalHaul(SemiFunc.StatGetRunTotalHaul() + currencyToAdd);
                CurrencyUI.instance.FetchCurrency();
                if (SemiFunc.IsMasterClientOrSingleplayer())
                {
                    RoundDirector.instance.totalHaul += currencyToAdd * 1000;
                }
            }
            else
            {
                if (randomAmounts.All((x) => x == 0))
                {
                    return;
                }
                shenronVoice.Sounds = new AudioClip[] { spawnValuableClip };
                shenronVoice.VolumeRandom = 0.1f;
                shenronVoice.PitchRandom = 0.15f;
                if (SemiFunc.IsMasterClientOrSingleplayer())
                {
                    valuableSpawnPosition = SemiFunc.LevelPointsGetClosestToPlayer().transform.position + (Vector3.up * 1.5f);
                    LevelValuables levelPool = LevelGenerator.Instance.Level.ValuablePresets[Random.Range(0, LevelGenerator.Instance.Level.ValuablePresets.Count)];
                    List<GameObject> valuablePool = null;
                    while (valuablePool == null)
                    {
                        valuableType = Random.Range(0, 4);
                        if (randomAmounts[valuableType] == 0)
                        {
                            continue;
                        }
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
            int cap = upgradesInfo[upgrade].cap;
            int per = upgradesInfo[upgrade].per;
            int amount = per;
            if (cap > 0)
            {
                int upgradeAmount = StatsManager.instance.FetchPlayerUpgrades(steamID)[upgrade];
                int capDiff = cap - upgradeAmount;
                amount = Mathf.Min(per, capDiff);
                upgradesInfo[upgrade].playersAtCap[steamID] = capDiff <= 0;
            }
            if (amount <= 0)
            {
                wishableUpgrades.Remove(upgrade);
                if (WishableCapped(steamID, out wishableUpgrades))
                {
                    MegaUpgrade(steamID, wishableUpgrades[Random.Range(0, wishableUpgrades.Count)]);
                }
                return;
            }
            if (vanillaUpgrades.Contains(upgrade))
            {
                if (WildCardMod.instance.oldSharedUpgradesPresent)
                {
                    List<PlayerAvatar> players = SemiFunc.PlayerGetAll();
                    for (int i = 0; i < players.Count; i++)
                    {
                        string id = SemiFunc.PlayerGetSteamID(players[i]);
                        amount = Mathf.Min(per, cap - StatsManager.instance.FetchPlayerUpgrades(id)[upgrade]);
                        if (amount <= 0)
                        {
                            continue;
                        }
                        DoUpgrade(id, amount, upgrade);
                    }
                }
                else
                {
                    DoUpgrade(steamID, amount, upgrade);
                }
            }
            else if (WildCardMod.instance.moreUpgradesPresent)
            {
                MoreUpgradesUpgrade(steamID, upgrade);
            }
            else
            {
                log.LogWarning($"Dragon Ball wish for upgrade: {upgrade} failed, retrying...");
                wishBlacklist.Add(upgrade);
                if (WishableCapped(steamID, out wishableUpgrades))
                {
                    MegaUpgrade(steamID, wishableUpgrades[Random.Range(0, wishableUpgrades.Count)]);
                }
            }
        }
        public bool WishableCapped(string steamID, out List<string> cappedWishables)
        {
            if (wishableUpgrades.Count == 0)
            {
                wishableUpgrades = StatsManager.instance.FetchPlayerUpgrades(steamID).Keys.ToList();
                wishableUpgrades.RemoveAll(wishBlacklist.Contains);
            }
            cappedWishables = new List<string>(wishableUpgrades);
            for (int i = 0; i < wishableUpgrades.Count; i++)
            {
                if (upgradesInfo[wishableUpgrades[i]].playersAtCap[steamID])
                {
                    cappedWishables.Remove(wishableUpgrades[i]);
                }
            }
            return cappedWishables.Count > 0;
        }
        [MethodImpl(MethodImplOptions.NoInlining)]
        public void MoreUpgradesUpgrade(string steamID, string upgrade)
        {
            if (MoreUpgrades.Plugin.instance.upgradeItems.Find((x) => x.name == upgrade) == null)
            {
                wishBlacklist.Add(upgrade);
                log.LogWarning($"Dragon Ball wish upgrade tried to use MoreUpgrades' \"{upgrade}\" but something went wrong");
                return;
            }
            log.LogDebug($"Using MoreUpgrades to upgrade \"{upgrade}\"!");
            int cap = upgradesInfo[upgrade].cap;
            int per = upgradesInfo[upgrade].per;
            int amount = Mathf.Min(per, cap - StatsManager.instance.FetchPlayerUpgrades(steamID)[upgrade]);
            if (amount <= 0)
            {
                wishableUpgrades.Remove(upgrade);
                if (wishableUpgrades.Count == 0)
                {
                    wishableUpgrades = StatsManager.instance.FetchPlayerUpgrades(steamID).Keys.ToList();
                    wishableUpgrades.RemoveAll(wishBlacklist.Contains);
                }
                MegaUpgrade(steamID, wishableUpgrades[Random.Range(0, wishableUpgrades.Count)]);
                return;
            }
            if (WildCardMod.instance.oldSharedUpgradesPresent)
            {
                List<PlayerAvatar> players = SemiFunc.PlayerGetAll();
                for (int i = 0; i < players.Count; i++)
                {
                    MoreUpgrades.Classes.MoreUpgradesManager.instance.Upgrade(upgrade, SemiFunc.PlayerGetSteamID(players[i]), amount);
                }
                return;
            }
            MoreUpgrades.Classes.MoreUpgradesManager.instance.Upgrade(upgrade, steamID, amount);
        }
        public void DoUpgrade(string id, int amount, string upgrade)
        {
            string methodString = $"Upgrade{upgrade}RPC".Replace(" ", string.Empty);
            MethodInfo method = typeof(ShenronHUD).GetMethod(methodString);
            if (method == null)
            {
                log.LogWarning($"Upgrade method for \"{upgrade}\" could not be found!");
                return;
            }
            if (SemiFunc.IsMultiplayer())
            {
                photonView.RPC(methodString, RpcTarget.Others, id, amount);
            }
            method.Invoke(this, new object[] { id, amount });
        }
        [PunRPC]
        public void UpgradeHealthRPC(string id, int amount)
        {
            for (int i = 0; i < amount; i++)
            {
                PunManager.instance.UpgradePlayerHealth(id);
            }
        }
        [PunRPC]
        public void UpgradeStaminaRPC(string id, int amount)
        {
            for (int i = 0; i < amount; i++)
            {
                PunManager.instance.UpgradePlayerEnergy(id);
            }
        }
        [PunRPC]
        public void UpgradeLaunchRPC(string id, int amount)
        {
            for (int i = 0; i < amount; i++)
            {
                PunManager.instance.UpgradePlayerTumbleLaunch(id);
            }
        }
        [PunRPC]
        public void UpgradeSpeedRPC(string id, int amount)
        {
            for (int i = 0; i < amount; i++)
            {
                PunManager.instance.UpgradePlayerSprintSpeed(id);
            }
        }
        [PunRPC]
        public void UpgradeStrengthRPC(string id, int amount)
        {
            for (int i = 0; i < amount; i++)
            {
                PunManager.instance.UpgradePlayerGrabStrength(id);
            }
        }
        [PunRPC]
        public void UpgradeRangeRPC(string id, int amount)
        {
            for (int i = 0; i < amount; i++)
            {
                PunManager.instance.UpgradePlayerGrabRange(id);
            }
        }
        [PunRPC]
        public void UpgradeExtraJumpRPC(string id, int amount)
        {
            for (int i = 0; i < amount; i++)
            {
                PunManager.instance.UpgradePlayerExtraJump(id);
            }
        }
        [PunRPC]
        public void UpgradeCrouchRestRPC(string id, int amount)
        {
            for (int i = 0; i < amount; i++)
            {
                PunManager.instance.UpgradePlayerCrouchRest(id);
            }
        }
        [PunRPC]
        public void UpgradeTumbleWingsRPC(string id, int amount)
        {
            for (int i = 0; i < amount; i++)
            {
                PunManager.instance.UpgradePlayerTumbleWings(id);
            }
        }
        public void OnDestroy()
        {
            List<PlayerAvatar> players = SemiFunc.PlayerGetAll();
            for (int i = 0; i < players.Count; i++)
            {
                if (players[i].playerHealth.bodyMaterial.GetColor("_EmissionColor") != Color.black)
                {
                    players[i].playerHealth.bodyMaterial.SetColor("_EmissionColor", Color.black);
                    players[i].playerDeathHead.headRenderer.material.SetColor("_EmissionColor", Color.black);
                }
            }
        }
        public class WishUpgrade
        {
            public WishUpgrade(int cap = 0, int per = 5)
            {
                this.cap = cap;
                this.per = per;
            }
            public int cap;
            public int per;
            public Dictionary<string, bool> playersAtCap = new Dictionary<string, bool>();
        }
    }
}