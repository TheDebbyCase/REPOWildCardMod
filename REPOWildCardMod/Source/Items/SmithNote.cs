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
        public ItemBattery itemBattery;
        public ItemToggle itemToggle;
        public ItemEquippable itemEquippable;
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
        public float animNormal = 0f;
        public bool opened;
        public bool charged;
        public bool overriding = false;
        public Coroutine crawlerCoroutine = null;
        public Vector3 forceRotation = new Vector3(-75f, 0f, 15f);
        public void Start()
        {
            RefreshPlayerList();
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
                RefreshPlayerList();
                bookSound.Sounds = new AudioClip[] { audioClips[0] };
                bookSound.Play(transform.position);
                for (int i = 0; i < pageText.Length; i++)
                {
                    pageText[i].gameObject.SetActive(true);
                }
                if (SemiFunc.IsMasterClientOrSingleplayer())
                {
                    string pageOneString = string.Empty;
                    string pageTwoString = string.Empty;
                    List<string> playerNames = playersDead.Keys.ToList();
                    playerNames.Sort();
                    if (playersDead.Count <= 6)
                    {
                        for (int i = 0; i < Mathf.Min(playersDead.Count, 3); i++)
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
                        pageOneString.Remove(pageOneString.Length - 2);
                        SetPageText(0, pageOneString);
                        if (playersDead.Count > 3)
                        {
                            for (int i = 3; i < Mathf.Min(playersDead.Count, 6); i++)
                            {
                                if (playersDead[playerNames[i]])
                                {
                                    pageTwoString += $"<color=\"red\"><s>{playerNames[i]}<s></color>\n";
                                }
                                else
                                {
                                    pageTwoString += $"{playerNames[i]}\n";
                                }
                            }
                            pageTwoString.Remove(pageTwoString.Length - 2);
                            SetPageText(1, pageTwoString);
                        }
                        else
                        {
                            SetPageText(1, string.Empty);
                        }
                    }
                    else
                    {
                        for (int i = 0; i < Mathf.Max(Mathf.FloorToInt((float)playersDead.Count / 2f), 6); i++)
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
                        pageOneString.Remove(pageOneString.Length - 2);
                        SetPageText(0, pageOneString);
                        for (int i = Mathf.Max(Mathf.FloorToInt((float)playersDead.Count / 2f), 6); i < playersDead.Count; i++)
                        {
                            if (playersDead[playerNames[i]])
                            {
                                pageTwoString += $"<s>{playerNames[i]}<s>\n";
                            }
                            else
                            {
                                pageTwoString += $"{playerNames[i]}\n";
                            }
                        }
                        pageTwoString.Remove(pageTwoString.Length - 2);
                        SetPageText(1, pageTwoString);
                    }
                }
                opened = true;
            }
            else if (!physGrabObject.grabbed && opened)
            {
                opened = false;
            }
            if (itemBattery.batteryLife <= 0f && charged)
            {
                titleText.text = ":(";
                charged = false;
            }
            else if (itemBattery.batteryLife > 0f && !charged)
            {
                titleText.text = "SMITH\nNOTE";
                charged = true;
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
                if (!SemiFunc.IsMultiplayer() && itemToggle.toggleState && charged)
                {
                    KillPlayer(PlayerAvatar.instance.playerName);
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
                    for (int i = 0; i < particleSystems.Length; i++)
                    {
                        ParticleSystem.MainModule main = particleSystems[i].main;
                        main.startColor = new ParticleSystem.MinMaxGradient(particlesStartColor);
                        main.gravityModifierMultiplier = 1f;
                        main.loop = true;
                        particleSystems[i].Play();
                    }
                    musicPlaying = true;
                }
                musicSound.PlayLoop(true, 0.75f, 0.75f);
            }
            else if (!itemToggle.toggleState)
            {
                musicSound.PlayLoop(false, 0.75f, 0.75f);
                if (musicPlaying)
                {
                    for (int i = 0; i < particleSystems.Length; i++)
                    {
                        ParticleSystem.MainModule main = particleSystems[i].main;
                        if (!charged)
                        {
                            main.startColor = new ParticleSystem.MinMaxGradient(Color.red);
                        }
                        main.gravityModifierMultiplier = 0f;
                        main.loop = false;
                    }
                    musicPlaying = false;
                }
            }
        }
        public void RefreshPlayerList()
        {
            Dictionary<string, bool> newDict = new Dictionary<string, bool>();
            for (int i = 0; i < GameDirector.instance.PlayerList.Count; i++)
            {
                PlayerAvatar player = GameDirector.instance.PlayerList[i];
                newDict.Add(player.playerName, player.deadSet || (playersDead.Count > 0 && playersDead[player.playerName]));
                log.LogDebug($"{player.playerName} added to Smith Note list, dead: {newDict[player.playerName]}");
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
        public void KillPlayer(string player)
        {
            RefreshPlayerList();
            player = playersDead.Keys.ToList().Find((x) => WildCardMod.utils.TextIsSimilar(player, x));
            log.LogDebug($"Smith Note killing player: {player}");
            bookSound.Sounds = new AudioClip[] { audioClips[1] };
            bookSound.Play(transform.position);
            if (crawlerCoroutine != null)
            {
                StopCoroutine(crawlerCoroutine);
                crawlerCoroutine = null;
            }
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
        [PunRPC]
        public void KillPlayerRPC(string player, string killer)
        {
            RefreshPlayerList();
            if (player != null && playersDead.ContainsKey(player))
            {
                playersDead[player] = true;
                if (PlayerAvatar.instance == SemiFunc.PlayerGetFromName(player))
                {
                    StartCoroutine(KillCoroutine(killer));
                }
                itemBattery.SetBatteryLife(0);
                itemToggle.ToggleItem(toggle: false);
            }
        }
        public IEnumerator CrawlingCrossCoroutine(string name)
        {
            int id = Array.FindIndex(pageText, (x) => x.text.Contains(name));
            string newText;
            for (int i = 1; i < name.Length; i++)
            {
                if (i > 1)
                {
                    yield return new WaitForSeconds(2f / (name.Length - 1));
                }
                newText = pageText[id].text.Replace(name, $"<color=\"red\"><s>{name[..i]}</s>{name[i..]}</color>");
                SetPageText(id, newText);
            }
            crawlerCoroutine = null;
        }
        public IEnumerator KillCoroutine(string killer)
        {
            log.LogDebug($"Smith Note kill sent by player: {killer}");
            bookSound.Sounds = new AudioClip[] { audioClips[4] };
            bookSound.Play(CameraGlitch.Instance.transform.position, 0.5f);
            KillerSoundRPC(false, killer);
            yield return new WaitForSeconds(5f);
            if (SemiFunc.IsMultiplayer())
            {
                ChatManager.instance.PossessSelfDestruction();
                photonView.RPC("KillerSoundRPC", RpcTarget.All, killer);
            }
            else
            {
                KillerSoundRPC(true, killer);
                PlayerAvatar.instance.playerHealth.health = 0;
                PlayerAvatar.instance.playerHealth.Hurt(1, savingGrace: false);
            }
        }
        [PunRPC]
        public void KillerSoundRPC(bool dead, string killer)
        {
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