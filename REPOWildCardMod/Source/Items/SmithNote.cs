using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
namespace REPOWildCardMod.Items
{
    public class SmithNote : MonoBehaviour
    {
        readonly BepInEx.Logging.ManualLogSource log = WildCardMod.log;
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
        public Dictionary<string, List<EnemyParent>> spawnedEnemies = new Dictionary<string, List<EnemyParent>>();
        public float animNormal = 0f;
        public bool opened;
        public bool charged;
        public bool overriding = false;
        public Coroutine crawlerCoroutine = null;
        public Vector3 forceRotation = new Vector3(-75f, 0f, 15f);
        public bool enemyExhaust;
        private System.Random random;
        public void Start()
        {
            random = new System.Random();
            enemyExhaust = false;
        }
        public void FixedUpdate()
        {
            if (physGrabObject.grabbed && SemiFunc.IsMasterClientOrSingleplayer())
            {
                int nonRotatingGrabbers = physGrabObject.playerGrabbing.Count;
                for (int i = 0; i < physGrabObject.playerGrabbing.Count; i++)
                {
                    if (physGrabObject.playerGrabbing[i].isRotating)
                    {
                        nonRotatingGrabbers--;
                    }
                }
                if (nonRotatingGrabbers == physGrabObject.playerGrabbing.Count)
                {
                    physGrabObject.TurnXYZ(Quaternion.Euler(forceRotation.x, 0f, 0f), Quaternion.Euler(0f, forceRotation.y, 0f), Quaternion.Euler(0f, 0f, forceRotation.z));
                }
            }
        }
        public void Update()
        {
            if (SemiFunc.IsMasterClientOrSingleplayer() && SemiFunc.RunIsLevel() && RoundDirector.instance.allExtractionPointsCompleted && (roomVolumeCheck.inTruck || itemEquippable.isEquipped) && StatsManager.instance.itemsPurchased[itemAttributes.item.itemAssetName] == 0)
            {
                StatsManager.instance.ItemPurchase(itemAttributes.item.itemAssetName);
            }
            if (physGrabObject.grabbedLocal && !overriding && itemEquippable.currentState == ItemEquippable.ItemState.Idle && (!PhysGrabber.instance.overrideGrab || PhysGrabber.instance.overrideGrabTarget != physGrabObject))
            {
                itemEquippable.ForceGrab();
                itemEquippable.forceGrabTimer = 0.2f;
                PhysGrabber.instance.OverrideGrabDistance(0.8f);
                overriding = true;
            }
            else if (!physGrabObject.grabbedLocal && overriding)
            {
                overriding = false;
            }
            if (physGrabObject.grabbed && !opened)
            {
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
                opened = true;
            }
            else if (!physGrabObject.grabbed && opened)
            {
                opened = false;
            }
            if ((itemBattery.batteryLife >= 1f) && !charged)
            {
                titleText.text = "SMITH\nNOTE";
                charged = true;
            }
            else if (charged)
            {
                titleText.text = ":(";
                charged = false;
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
                if (!SemiFunc.IsMultiplayer() && itemToggle.toggleState && charged && SemiFunc.RunIsLevel() && !playersDead[PlayerAvatar.instance.playerName])
                {
                    RefreshLists();
                    KillMessage(spawnedEnemies.ElementAt(random.Next(0, spawnedEnemies.Count)).Key);
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
            if (itemToggle.toggleState && charged)
            {
                if (!musicPlaying)
                {
                    musicSound.PlayLoop(true, 0.75f, 0.75f);
                    if (enemyExhaust)
                    {
                        for (int i = 0; i < particleSystems.Length; i++)
                        {
                            ParticleSystem.MainModule main = particleSystems[i].main;
                            main.startColor = new ParticleSystem.MinMaxGradient(Color.red);
                            main.gravityModifierMultiplier = 0f;
                        }
                    }
                    else
                    {
                        for (int i = 0; i < particleSystems.Length; i++)
                        {
                            ParticleSystem.MainModule main = particleSystems[i].main;
                            main.startColor = new ParticleSystem.MinMaxGradient(particlesStartColor);
                            main.gravityModifierMultiplier = 1f;
                            particleSystems[i].Play();
                        }
                    }
                    musicPlaying = true;
                }
            }
            else if (!itemToggle.toggleState)
            {
                musicSound.PlayLoop(false, 0.75f, 0.75f);
                if (musicPlaying)
                {
                    musicPlaying = false;
                }
            }
        }
        public void CalculatePages()
        {
            string pageOneString = string.Empty;
            string pageTwoString = string.Empty;
            List<string> playerNames = playersDead.Keys.ToList();
            List<string> enemyNames = spawnedEnemies.Keys.ToList();
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
                CalculatePages();
                crawlerCoroutine = null;
            }
            RefreshPlayerList();
            if (SemiFunc.RunIsLevel())
            {
                RefreshEnemiesList();
            }
            else
            {
                spawnedEnemies.Clear();
            }
        }
        public void RefreshEnemiesList()
        {
            Dictionary<string, List<EnemyParent>> newDict = new Dictionary<string, List<EnemyParent>>();
            int enemyCount = EnemyDirector.instance.enemiesSpawned.Count;
            for (int i = 0; i < enemyCount; i++)
            {
                EnemyParent enemy = EnemyDirector.instance.enemiesSpawned[i];
                if (!newDict.ContainsKey(enemy.enemyName))
                {
                    newDict.Add(enemy.enemyName, new List<EnemyParent> { enemy });
                }
                else
                {
                    newDict[enemy.enemyName].Add(enemy);
                }
                log.LogDebug($"{enemy.enemyName} added to Smith Note list");
            }
            if (spawnedEnemies != newDict)
            {
                spawnedEnemies = newDict;
            }
        }
        public void RefreshPlayerList()
        {
            Dictionary<string, bool> newDict = new Dictionary<string, bool>();
            for (int i = 0; i < GameDirector.instance.PlayerList.Count; i++)
            {
                PlayerAvatar player = GameDirector.instance.PlayerList[i];
                string name = WildCardMod.utils.CleanText(player.playerName);
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
            log.LogDebug($"Page {id} text: \"{text}\"");
            pageText[id].text = text;
        }
        public void KillMessage(string message)
        {
            RefreshLists();
            string player = playersDead.Keys.ToList().Find((x) => WildCardMod.utils.TextIsSimilar(message, x));
            string enemy = spawnedEnemies.Keys.ToList().Find((x) => WildCardMod.utils.TextIsSimilar(message, x));
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
            else if (!enemyExhaust)
            {
                if (SemiFunc.IsMultiplayer())
                {
                    photonView.RPC("KillEnemyRPC", RpcTarget.MasterClient, enemy, PlayerAvatar.instance.steamID);
                }
                else
                {
                    KillEnemyRPC(enemy, PlayerAvatar.instance.steamID);
                }
            }
            else
            {
                log.LogDebug($"Smith note is exhausted and so cannot kill {enemy}!");
                if (SemiFunc.IsMultiplayer())
                {
                    photonView.RPC("BookWiggleRPC", RpcTarget.All);
                }
                else
                {
                    BookWiggleRPC();
                }
            }
        }
        [PunRPC]
        public void BookWiggleRPC()
        {
            notValuableObject.audioPreset.impactHeavy.Play(transform.position);
            if (SemiFunc.IsMasterClientOrSingleplayer())
            {
                physGrabObject.rb.AddForce(UnityEngine.Random.insideUnitSphere * 2f, ForceMode.Impulse);
                physGrabObject.rb.AddTorque(UnityEngine.Random.insideUnitSphere * 2f, ForceMode.Impulse);
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
            EnemyParent selectedEnemy = spawnedEnemies[enemy].ElementAt(random.Next(0, spawnedEnemies[enemy].Count));
            EnemyHealth enemyHealth = selectedEnemy.GetComponentInChildren<EnemyHealth>();
            if (selectedEnemy.enemyName == "Gnome" || selectedEnemy.enemyName == "Banger")
            {
                for (int i = 0; i < spawnedEnemies[enemy].Count; i++)
                {
                    enemyHealth = spawnedEnemies[enemy][i].GetComponentInChildren<EnemyHealth>();
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
            int id = Array.FindIndex(pageText, (x) => x.text.Contains(name));
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
                if (enemy)
                {
                    enemyExhaust = true;
                }
                itemBattery.SetBatteryLife(0);
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