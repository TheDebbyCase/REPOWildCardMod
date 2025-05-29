using Photon.Pun;
using REPOWildCardMod.Utils;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
namespace REPOWildCardMod.Items
{
    public class SmithNote : MonoBehaviour
    {
        readonly BepInEx.Logging.ManualLogSource log = WildCardMod.instance.log;
        readonly WildCardUtils utils = WildCardMod.instance.utils;
        public PhotonView photonView;
        public PhysGrabObject physGrabObject;
        public ItemAttributes itemAttributes;
        public ItemBattery itemBattery;
        public ItemToggle itemToggle;
        public ItemEquippable itemEquippable;
        public RoomVolumeCheck roomVolumeCheck;
        public NotValuableObject notValuableObject;
        public Sound bookSound;
        public Sound musicSound;
        public bool musicPlaying;
        public AudioClip[] audioClips;
        public Animator animator;
        public TextMeshPro titleText;
        public TextMeshPro[] pageText;
        public ParticleSystem[] particleSystems;
        public Color particlesStartColor;
        public Dictionary<string, bool> playersDead = new Dictionary<string, bool>();
        public Dictionary<string, List<EnemyParent>> currentEnemies = new Dictionary<string, List<EnemyParent>>();
        public float animNormal = 0f;
        public bool opened;
        public bool charged;
        public bool overriding = false;
        public Coroutine crawlerCoroutine = null;
        public Vector3 forceRotation = new Vector3(-75f, 0f, 15f);
        public TutorialDirector.TutorialPage tutorial;
        public bool neverGrab = true;
        public bool firstActivate = true;
        public void Awake()
        {
            if (WildCardMod.instance.usingBeta)
            {
                log.LogWarning("Smith Note may not work as expected due to REPO beta changes!");
            }
        }
        public void Start()
        {
            if (TutorialDirector.instance.tutorialPages.Find((x) => x.pageName == tutorial.pageName) == null)
            {
                TutorialDirector.instance.tutorialPages.Add(tutorial);
            }
        }
        public void FixedUpdate()
        {
            if (physGrabObject.grabbed && SemiFunc.IsMasterClientOrSingleplayer())
            {
                for (int i = 0; i < physGrabObject.playerGrabbing.Count; i++)
                {
                    if (physGrabObject.playerGrabbing[i].isRotating)
                    {
                        return;
                    }
                }
                physGrabObject.TurnXYZ(Quaternion.Euler(forceRotation.x, 0f, 0f), Quaternion.Euler(0f, forceRotation.y, 0f), Quaternion.Euler(0f, 0f, forceRotation.z));
            }
        }
        public void Update()
        {
            if (SemiFunc.IsMasterClientOrSingleplayer() && SemiFunc.RunIsLevel() && RoundDirector.instance.allExtractionPointsCompleted && (roomVolumeCheck.inTruck || itemEquippable.isEquipped) && StatsManager.instance.itemsPurchased[itemAttributes.item.itemAssetName] == 0)
            {
                StatsManager.instance.ItemPurchase(itemAttributes.item.itemAssetName);
            }
            if (physGrabObject.grabbedLocal && !overriding && !itemEquippable.isEquipped && (!PhysGrabber.instance.overrideGrab || PhysGrabber.instance.overrideGrabTarget != physGrabObject))
            {
                if (neverGrab)
                {
                    ChatManager.instance.PossessChatScheduleStart(9);
                    ChatManager.instance.PossessChat(ChatManager.PossessChatID.LovePotion, "I should type my target in chat", 2f, Color.blue);
                    ChatManager.instance.PossessChatScheduleEnd();
                    bool tips = GameplayManager.instance.tips;
                    GameplayManager.instance.tips = true;
                    TutorialDirector.instance.ActivateTip(tutorial.pageName, 0.5f, true);
                    if (!tips)
                    {
                        GameplayManager.instance.tips = false;
                    }
                    TutorialDirector.instance.shownTips.Remove(tutorial.pageName);
                    neverGrab = false;
                }
                PhysGrabber.instance.OverrideGrabDistance(0.5f);
                overriding = true;
            }
            else if (!physGrabObject.grabbedLocal && overriding)
            {
                overriding = false;
            }
            if (physGrabObject.grabbed && !opened)
            {
                opened = true;
                bookSound.Sounds = new AudioClip[] { audioClips[0] };
                bookSound.Play(transform.position);
                if (SemiFunc.IsMasterClientOrSingleplayer())
                {
                    RefreshLists();
                }
                for (int i = 0; i < pageText.Length; i++)
                {
                    pageText[i].gameObject.SetActive(true);
                }
            }
            else if (!physGrabObject.grabbed && opened)
            {
                opened = false;
            }
            if (charged != itemBattery.batteryLife >= 50f)
            {
                charged = itemBattery.batteryLife >= 50f;
            }
            if (itemBattery.batteryLife > 0f)
            {
                if (titleText.text != "SMITH\nNOTE")
                {
                    titleText.text = "SMITH\nNOTE";
                }
            }
            else if (titleText.text != ":(")
            {
                titleText.text = ":(";
            }
            if (opened)
            {
                if (animNormal < 1f)
                {
                    animNormal += Time.deltaTime;
                }
                else if (animNormal > 1f)
                {
                    animNormal = 1f;
                }
                if (!SemiFunc.IsMultiplayer() && itemToggle.toggleState && SemiFunc.RunIsLevel() && !playersDead[PlayerAvatar.instance.playerName])
                {
                    RefreshLists();
                    KillMessage(currentEnemies.ElementAt(Random.Range(0, currentEnemies.Count)).Key);
                }
            }
            else
            {
                if (animNormal > 0f)
                {
                    animNormal -= Time.deltaTime;
                }
                else if (animNormal < 0f)
                {
                    if (crawlerCoroutine != null)
                    {
                        StopCoroutine(crawlerCoroutine);
                        CalculatePages();
                        crawlerCoroutine = null;
                    }
                    for (int i = 0; i < pageText.Length; i++)
                    {
                        pageText[i].gameObject.SetActive(false);
                    }
                    animNormal = 0f;
                }
                if (itemToggle.toggleState)
                {
                    itemToggle.ToggleItem(toggle: false);
                }
            }
            if (animator.GetFloat("Normal") != animNormal)
            {
                animator.SetFloat("Normal", animNormal);
            }
            if (itemToggle.toggleState && itemBattery.batteryLife > 0f)
            {
                musicSound.PlayLoop(true, 0.75f, 0.75f);
                if (!musicPlaying)
                {
                    for (int i = 0; i < particleSystems.Length; i++)
                    {
                        ParticleSystem.MainModule main = particleSystems[i].main;
                        main.startColor = new ParticleSystem.MinMaxGradient(particlesStartColor);
                        main.duration = 5f;
                        main.loop = true;
                        main.gravityModifierMultiplier = 1f;
                        particleSystems[i].Play();
                    }
                    musicPlaying = true;
                }
            }
            else if (!itemToggle.toggleState)
            {
                musicSound.PlayLoop(false, 0.75f, 0.75f);
                if (musicPlaying)
                {
                    for (int i = 0; i < particleSystems.Length; i++)
                    {
                        ParticleSystem.MainModule main = particleSystems[i].main;
                        main.duration = 1f;
                        main.loop = false;
                    }
                    musicPlaying = false;
                }
            }
        }
        public void CalculatePages()
        {
            string pageOneString = string.Empty;
            string pageTwoString = string.Empty;
            List<string> playerNames = playersDead.Keys.ToList();
            List<string> enemyNames = currentEnemies.Keys.ToList();
            playerNames.Sort();
            if (playersDead.Count <= 12)
            {
                for (int i = 0; i < playersDead.Count; i++)
                {
                    if (playersDead[playerNames[i]])
                    {
                        pageOneString += $"<color=\"red\"><s>{playerNames[i]}<s></color>\n";
                    }
                    else
                    {
                        pageOneString += $"{playerNames[i]}\n";
                    }
                }
                pageOneString.TrimEnd('\n');
                SetPageText(0, pageOneString);
                for (int i = 0; i < Mathf.Min(enemyNames.Count, 12); i++)
                {
                    pageTwoString += $"{enemyNames[i]}\n";
                }
                pageTwoString.TrimEnd('\n');
                SetPageText(1, pageTwoString);
            }
            else
            {
                List<string> combinedList = (List<string>)playerNames.Concat(enemyNames);
                for (int i = 0; i < Mathf.Min(Mathf.FloorToInt((float)combinedList.Count / 2f), 12); i++)
                {
                    if (playersDead.ContainsKey(combinedList[i]) && playersDead[combinedList[i]])
                    {
                        pageOneString += $"<color=\"red\"><s>{combinedList[i]}<s></color>\n";
                    }
                    else
                    {
                        pageOneString += $"{combinedList[i]}\n";
                    }
                }
                pageOneString.TrimEnd('\n');
                SetPageText(0, pageOneString);
                for (int i = 0; i < Mathf.Min(Mathf.CeilToInt((float)combinedList.Count / 2f), 12); i++)
                {
                    if (playersDead.ContainsKey(combinedList[i]) && playersDead[combinedList[i]])
                    {
                        pageTwoString += $"<color=\"red\"><s>{combinedList[i]}<s></color>\n";
                    }
                    else
                    {
                        pageTwoString += $"{combinedList[i]}\n";
                    }
                }
                pageTwoString.TrimEnd('\n');
                SetPageText(1, pageTwoString);
            }
        }
        public void RefreshLists()
        {
            if (GameManager.Multiplayer())
            {
                photonView.RPC("RefreshListsRPC", RpcTarget.All);
            }
            else
            {
                RefreshListsRPC();
            }
        }
        [PunRPC]
        public void RefreshListsRPC()
        {
            if (crawlerCoroutine != null)
            {
                StopCoroutine(crawlerCoroutine);
                crawlerCoroutine = null;
            }
            RefreshEnemiesList();
            RefreshPlayerList();
            CalculatePages();
        }
        public void RefreshEnemiesList()
        {
            for (int i = 0; i < EnemyDirector.instance.enemiesSpawned.Count; i++)
            {
                EnemyParent newEnemy = EnemyDirector.instance.enemiesSpawned[i];
                if (currentEnemies.ContainsKey(newEnemy.enemyName))
                {
                    currentEnemies[newEnemy.enemyName].Clear();
                }
                if (newEnemy.DespawnedTimer <= 0f)
                {
                    log.LogDebug($"{newEnemy.enemyName} added to Smith Note enemies list");
                    if (currentEnemies.ContainsKey(newEnemy.enemyName))
                    {
                        currentEnemies[newEnemy.enemyName].Add(newEnemy);
                    }
                    else
                    {
                        currentEnemies.Add(newEnemy.enemyName, new List<EnemyParent>() { newEnemy });
                    }
                }
                else if (currentEnemies.ContainsKey(newEnemy.enemyName) && currentEnemies[newEnemy.enemyName].Count == 0)
                {
                    log.LogDebug($"{newEnemy.enemyName} removed from Smith Note enemies list");
                    currentEnemies.Remove(newEnemy.enemyName);
                }
            }
        }
        public void RefreshPlayerList()
        {
            Dictionary<string, bool> newDict = new Dictionary<string, bool>();
            for (int i = 0; i < GameDirector.instance.PlayerList.Count; i++)
            {
                PlayerAvatar player = GameDirector.instance.PlayerList[i];
                string name = utils.CleanText(player.playerName);
                newDict.Add(name, player.deadSet || (playersDead.Count > 0 && playersDead[name]));
                log.LogDebug($"{name} added to Smith Note list, dead: {newDict[name]}");
            }
            if (playersDead != newDict)
            {
                playersDead = newDict;
            }
        }
        public void SetPageText(int id, string text)
        {
            if (GameManager.Multiplayer())
            {
                photonView.RPC("SetPageTextRPC", RpcTarget.All, id, text);
            }
            else
            {
                SetPageTextRPC(id, text);
            }
        }
        [PunRPC]
        public void SetPageTextRPC(int id, string text)
        {
            if (opened)
            {
                log.LogDebug($"Page {id + 1} text: \"{text.Replace("\n", ", ")}\"");
            }
            pageText[id].text = text;
        }
        public void KillMessage(string message)
        {
            RefreshLists();
            string player = playersDead.Keys.ToList().Find((x) => utils.TextIsSimilar(message, x));
            string enemy = currentEnemies.Keys.ToList().Find((x) => utils.TextIsSimilar(message, x));
            if ((player == null || player == string.Empty) && (enemy == null || enemy == string.Empty))
            {
                log.LogDebug($"Message reading \"{message}\" was not one of the Smith Note's targets");
                return;
            }
            else
            {
                bookSound.Sounds = new AudioClip[] { audioClips[1] };
                bookSound.Play(transform.position);
                log.LogDebug($"Smith Note killing {message}");
                if (crawlerCoroutine != null)
                {
                    StopCoroutine(crawlerCoroutine);
                    CalculatePages();
                    crawlerCoroutine = null;
                }
            }
            if (player != null && player != string.Empty)
            {
                crawlerCoroutine = StartCoroutine(CrawlingCrossCoroutine(player));
                if (SemiFunc.IsMultiplayer())
                {
                    photonView.RPC("KillPlayerRPC", RpcTarget.All, player, PlayerAvatar.instance.steamID);
                }
                else
                {
                    KillPlayerRPC(player, PlayerAvatar.instance.steamID);
                }
            }
            else
            {
                if (!charged)
                {
                    BookWiggle();
                    return;
                }
                if (SemiFunc.IsMultiplayer())
                {
                    photonView.RPC("KillEnemyRPC", RpcTarget.MasterClient, enemy, PlayerAvatar.instance.steamID);
                }
                else
                {
                    KillEnemyRPC(enemy, PlayerAvatar.instance.steamID);
                }
            }
        }
        public void BookWiggle()
        {
            if (SemiFunc.IsMultiplayer())
            {
                ChatManager.instance.PossessChatScheduleStart(9);
                ChatManager.instance.PossessChat(ChatManager.PossessChatID.LovePotion, "Book needs more power, sad face", 2f, Color.blue);
                ChatManager.instance.PossessChatScheduleEnd();
                photonView.RPC("BookWiggleRPC", RpcTarget.All);
            }
            else
            {
                BookWiggleRPC();
            }
        }
        [PunRPC]
        public void BookWiggleRPC()
        {
            notValuableObject.audioPreset.impactHeavy.Play(transform.position);
            if (SemiFunc.IsMasterClientOrSingleplayer())
            {
                physGrabObject.rb.AddForce(Random.insideUnitSphere * 2f, ForceMode.Impulse);
                physGrabObject.rb.AddTorque(Random.insideUnitSphere * 2f, ForceMode.Impulse);
            }
        }
        [PunRPC]
        public void KillEnemyRPC(string enemy, string killer)
        {
            StartCoroutine(KillEnemyCoroutine(enemy, killer));
        }
        public IEnumerator KillEnemyCoroutine(string enemy, string killer)
        {
            log.LogDebug($"Smith Note kill for enemy: {enemy}, sent by player: {SemiFunc.PlayerGetFromSteamID(killer).playerName}");
            if (SemiFunc.IsMultiplayer())
            {
                photonView.RPC("KilledRPC", RpcTarget.All, false, killer, true);
            }
            else
            {
                KilledRPC(false, killer, true);
            }
            yield return new WaitForSeconds(5f);
            EnemyParent selectedEnemy = currentEnemies[enemy].ElementAt(Random.Range(0, currentEnemies[enemy].Count));
            EnemyHealth enemyHealth = selectedEnemy.GetComponentInChildren<EnemyHealth>();
            if (selectedEnemy.enemyName == "Gnome" || selectedEnemy.enemyName == "Banger")
            {
                for (int i = 0; i < currentEnemies[enemy].Count; i++)
                {
                    enemyHealth = currentEnemies[enemy][i].GetComponentInChildren<EnemyHealth>();
                    if (enemyHealth != null)
                    {
                        enemyHealth.Death(Vector3.down);
                    }
                    else
                    {
                        log.LogDebug($"Smith Note failed to kill a {selectedEnemy.enemyName}");
                    }
                }
            }
            else if (enemyHealth != null)
            {
                enemyHealth.Death(Vector3.down);
            }
            else
            {
                log.LogDebug($"Smith Note failed to kill {selectedEnemy.enemyName}");
            }
            if (SemiFunc.IsMultiplayer())
            {
                photonView.RPC("KilledRPC", RpcTarget.All, true, killer, true);
            }
            else
            {
                KilledRPC(true, killer, true);
            }
        }
        [PunRPC]
        public void KillPlayerRPC(string player, string killer)
        {
            playersDead[player] = true;
            if (PlayerAvatar.instance == SemiFunc.PlayerGetFromName(player))
            {
                StartCoroutine(KillPlayerCoroutine(killer));
            }
        }
        public IEnumerator CrawlingCrossCoroutine(string name)
        {
            int id = System.Array.FindIndex(pageText, (x) => x.text.Contains(name));
            string newText;
            string prevReplacer = string.Empty;
            if (name.Length >= 2)
            {
                prevReplacer = $"<color=\"red\"><s>{name[..1]}</s>{name[1..]}</color>";
                newText = pageText[id].text.Replace(name, prevReplacer);
            }
            else
            {
                newText = pageText[id].text.Replace(name, $"<color=\"red\"><s>{name}</s></color>");
            }
            for (int i = 2; i < name.Length; i++)
            {
                yield return new WaitForSeconds(2f / (name.Length - 1));
                newText = pageText[id].text.Replace(prevReplacer, $"<color=\"red\"><s>{name[..i]}</s>{name[i..]}</color>");
                SetPageText(id, newText);
                prevReplacer = $"<color=\"red\"><s>{name[..i]}</s>{name[i..]}</color>";
            }
            CalculatePages();
            crawlerCoroutine = null;
        }
        public IEnumerator KillPlayerCoroutine(string killer)
        {
            log.LogDebug($"Smith Note kill sent by player: {SemiFunc.PlayerGetFromSteamID(killer).playerName}");
            bookSound.Sounds = new AudioClip[] { audioClips[4] };
            bookSound.Play(CameraGlitch.Instance.transform.position, 0.5f);
            if (SemiFunc.IsMultiplayer())
            {
                photonView.RPC("KilledRPC", RpcTarget.All, false, killer, false);
            }
            else
            {
                KilledRPC(false, killer, false);
            }
            yield return new WaitForSeconds(5f);
            if (SemiFunc.IsMultiplayer())
            {
                ChatManager.instance.PossessSelfDestruction();
                photonView.RPC("KilledRPC", RpcTarget.All, true, killer, false);
            }
            else
            {
                KilledRPC(true, killer, false);
                PlayerAvatar.instance.playerHealth.health = 0;
                PlayerAvatar.instance.playerHealth.Hurt(1, savingGrace: false);
            }
        }
        [PunRPC]
        public void KilledRPC(bool dead, string killer, bool enemy)
        {
            if (!dead)
            {
                if (SemiFunc.IsMasterClientOrSingleplayer())
                {
                    if (enemy)
                    {
                        if (WildCardMod.instance.ModConfig.noteDestroy.Value)
                        {
                            physGrabObject.DestroyPhysGrabObject();
                        }
                        else
                        {
                            itemBattery.BatteryFullPercentChange(itemBattery.batteryLifeInt - 3);
                        }
                    }
                    else
                    {
                        itemBattery.batteryLife -= 10f;
                    }
                }
                itemToggle.ToggleItem(toggle: false);
            }
            else
            {
                RefreshLists();
            }
            if (PlayerAvatar.instance == SemiFunc.PlayerAvatarGetFromSteamID(killer))
            {
                if (dead)
                {
                    bookSound.Sounds = new AudioClip[] { audioClips[3] };
                    bookSound.Play(CameraGlitch.Instance.transform.position, 0.1f);
                }
                else
                {
                    bookSound.Sounds = new AudioClip[] { audioClips[2] };
                    bookSound.Play(CameraGlitch.Instance.transform.position, 0.1f);
                }
            }
        }
    }
}