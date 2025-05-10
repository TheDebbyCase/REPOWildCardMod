using UnityEngine;
using Photon.Pun;
using REPOWildCardMod.Utils;
using REPOLib.Extensions;
using UnityEngine.Events;
using System;
using REPOWildCardMod.Extensions;
using System.Linq;
namespace REPOWildCardMod.Items
{
    public class WormJar : MonoBehaviour
    {
        static readonly BepInEx.Logging.ManualLogSource log = WildCardMod.instance.log;
        public PhotonView photonView;
        public PhysGrabObject physGrabObject;
        public PhysGrabObjectImpactDetector impactDetector;
        public ItemAttributes itemAttributes;
        public PlayerAvatar lastPlayerGrabbed;
        public Sprite icon;
        public float noDestroyCooldown;
        public bool firstRunGrab = true;
        public void Awake()
        {
            if (itemAttributes.icon != icon)
            {
                itemAttributes.icon = icon;
            }
        }
        public void Update()
        {
            if (itemAttributes.icon != icon)
            {
                itemAttributes.icon = icon;
            }
            if (SemiFunc.IsMasterClientOrSingleplayer())
            {
                if (physGrabObject.playerGrabbing.Count > 0 && lastPlayerGrabbed != physGrabObject.playerGrabbing[^1].playerAvatar)
                {
                    lastPlayerGrabbed = physGrabObject.playerGrabbing[^1].playerAvatar;
                    log.LogDebug($"Current Worm Jar mother: \"{lastPlayerGrabbed.playerName}\"");
                }
                if (noDestroyCooldown > 0f)
                {
                    noDestroyCooldown -= Time.deltaTime;
                }
                else if (impactDetector.destroyDisable)
                {
                    impactDetector.destroyDisable = false;
                }
            }
            if (SemiFunc.IsMultiplayer() && SemiFunc.RunIsLevel() && physGrabObject.grabbedLocal && firstRunGrab)
            {
                ChatManager.instance.PossessChatScheduleStart(9);
                ChatManager.instance.PossessChat(ChatManager.PossessChatID.LovePotion, "I should break this next to an enemy", 2f, Color.red + Color.blue);
                ChatManager.instance.PossessChatScheduleEnd();
                firstRunGrab = false;
            }
        }
        public void BreakJar()
        {
            if (SemiFunc.IsMasterClientOrSingleplayer())
            {
                Enemy enemy = SemiFunc.EnemyGetNearest(transform.position, 5f, false);
                WormInfectionData wormData = null;
                if (enemy != null)
                {
                    wormData = enemy.EnemyParent.WormData();
                }
                if (enemy == null || wormData == null || (wormData != null && (!wormData.hasWorm || (wormData.hasWorm && wormData.infected))))
                {
                    impactDetector.destroyDisable = true;
                    noDestroyCooldown = 0.1f;
                }
                else
                {
                    log.LogDebug($"Breaking Worm Jar");
                    wormData.worm.enabled = true;
                    wormData.worm.WakeUp(lastPlayerGrabbed.photonView.ViewID, SemiFunc.EnemyGetIndex(enemy));
                }
            }
        }
    }
    public class WormAttach : MonoBehaviour
    {
        static readonly BepInEx.Logging.ManualLogSource log = WildCardMod.instance.log;
        readonly WildCardUtils utils = WildCardMod.instance.utils;
        public PhotonView photonView;
        public Sound musicLoop;
        public Enemy enemy;
        public EnemyStateChaseSlow slowChaseRef;
        public PlayerAvatar motherPlayer;
        public float lifetime;
        public float investigateTimer;
        public bool lowLife = false;
        public float spreadTimer = 1f;
        public void Awake()
        {
            musicLoop.LoopClip = musicLoop.Sounds[0];
            musicLoop.Source.clip = musicLoop.LoopClip;
            musicLoop.AudioInfoFetched = true;
            gameObject.FixAudioMixerGroups();
            gameObject.SetActive(false);
        }
        public void Initialize(int enemyIndex)
        {
            if (SemiFunc.IsMultiplayer())
            {
                photonView.RPC("InitializeRPC", RpcTarget.All, enemyIndex);
            }
            else
            {
                InitializeRPC(enemyIndex);
            }
        }
        [PunRPC]
        public void InitializeRPC(int enemyIndex)
        {
            enemy = SemiFunc.EnemyGetFromIndex(enemyIndex);
            slowChaseRef = enemy.GetComponent<EnemyStateChaseSlow>();
            transform.parent = utils.FindEnemyTransform(enemy.EnemyParent, "Head");
            Vector3 overrideLocalPos = Vector3.zero;
            Vector3 overrideLocalRot = Vector3.zero;
            Vector3 overrideLocalScale = Vector3.one;
            switch (enemy.EnemyParent.enemyName)
            {
                case "Animal":
                    {
                        overrideLocalPos = new Vector3(-0.025f, 0.2f, -0.15f);
                        overrideLocalScale = new Vector3(0.7f, 0.7f, 0.7f);
                        break;
                    }
                case "Apex Predator":
                    {
                        overrideLocalPos = new Vector3(0f, 0.175f, 0f);
                        overrideLocalScale = new Vector3(0.5f, 0.5f, 0.5f);
                        break;
                    }
                case "Banger":
                    {
                        overrideLocalPos = new Vector3(0f, 0.25f, 0.05f);
                        overrideLocalScale = new Vector3(0.5f, 0.5f, 0.5f);
                        break;
                    }
                case "Bowtie":
                    {
                        overrideLocalPos = new Vector3(0f, 0.35f, 0f);
                        overrideLocalRot = new Vector3(0f, 90f, 0f);
                        overrideLocalScale = new Vector3(0.6f, 0.6f, 0.6f);
                        break;
                    }
                case "Chef":
                    {
                        overrideLocalPos = new Vector3(0f, 0.4f, 0.1f);
                        overrideLocalRot = new Vector3(15f, 0f, 0f);
                        overrideLocalScale = new Vector3(0.75f, 0.75f, 0.75f);
                        break;
                    }
                case "Clown":
                    {
                        overrideLocalPos = new Vector3(0f, 0.2f, 0f);
                        break;
                    }
                case "Gnome":
                    {
                        overrideLocalPos = new Vector3(0f, 0.3f, -0.075f);
                        overrideLocalRot = new Vector3(-15f, 0f, 0f);
                        overrideLocalScale = new Vector3(0.35f, 0.35f, 0.35f);
                        break;
                    }
                case "Headman":
                    {
                        overrideLocalPos = new Vector3(0f, 0.65f, 0.25f);
                        overrideLocalScale = new Vector3(1.5f, 1.5f, 1.5f);
                        break;
                    }
                case "Hidden":
                    {
                        overrideLocalPos = new Vector3(0f, 0.15f, -0.05f);
                        break;
                    }
                case "Huntsman":
                    {
                        overrideLocalPos = new Vector3(0.05f, 0.65f, 0f);
                        overrideLocalRot = new Vector3(0f, 90f, 0f);
                        overrideLocalScale = new Vector3(0.75f, 0.75f, 0.75f);
                        break;
                    }
                case "Mentalist":
                    {
                        overrideLocalPos = new Vector3(0f, 1.1f, -0.15f);
                        break;
                    }
                case "Peeper":
                    {
                        overrideLocalPos = new Vector3(0f, 0.05f, -0.025f);
                        break;
                    }
                case "Reaper":
                    {
                        overrideLocalPos = new Vector3(0f, 0.425f, 0f);
                        overrideLocalScale = new Vector3(0.75f, 0.75f, 0.75f);
                        break;
                    }
                case "Robe":
                    {
                        overrideLocalPos = new Vector3(0.05f, 0.3f, -0.125f);
                        overrideLocalRot = new Vector3(0f, 0f, -10f);
                        overrideLocalScale = new Vector3(1.25f, 1.25f, 1.25f);
                        break;
                    }
                case "Rugrat":
                    {
                        overrideLocalPos = new Vector3(0f, 0.4f, 0.15f);
                        overrideLocalScale = new Vector3(0.4f, 0.4f, 0.4f);
                        break;
                    }
                case "Shadow Child":
                    {
                        overrideLocalPos = new Vector3(-0.15f, 1.75f, 0f);
                        overrideLocalRot = new Vector3(10f, -90f, 0f);
                        overrideLocalScale = new Vector3(1.5f, 1.5f, 1.5f);
                        break;
                    }
                case "Spewer":
                    {
                        overrideLocalPos = new Vector3(0f, 0.115f, -0.185f);
                        overrideLocalRot = new Vector3(0f, 180f, 0f);
                        overrideLocalScale = new Vector3(0.25f, 0.25f, 0.25f);
                        break;
                    }
                case "Trudge":
                    {
                        overrideLocalPos = new Vector3(-0.015f, 0.25f, 0.05f);
                        overrideLocalRot = new Vector3(-10f, -45f, 0f);
                        overrideLocalScale = new Vector3(0.35f, 0.35f, 0.35f);
                        break;
                    }
                case "Upscream":
                    {
                        overrideLocalPos = new Vector3(0f, 0.375f, 0.1f);
                        overrideLocalScale = new Vector3(0.6f, 0.6f, 0.6f);
                        break;
                    }
            }
            transform.localPosition = overrideLocalPos;
            transform.localRotation = Quaternion.Euler(overrideLocalRot);
            transform.localScale = overrideLocalScale;
            enabled = false;
        }
        public void WakeUp(int playerID, int enemyIndex)
        {
            lifetime = 90f;
            if (SemiFunc.IsMultiplayer())
            {
                photonView.RPC("WakeUpRPC", RpcTarget.All, playerID, enemyIndex);
            }
            else
            {
                WakeUpRPC(playerID, enemyIndex);
            }
        }
        [PunRPC]
        public void WakeUpRPC(int playerID, int enemyIndex)
        {
            gameObject.SetActive(true);
            enemy = SemiFunc.EnemyGetFromIndex(enemyIndex);
            if (SemiFunc.IsMasterClientOrSingleplayer())
            {
                enemy.EnemyParent.WormData().infected = true;
            }
            log.LogDebug($"Worm has infected a \"{enemy.EnemyParent.enemyName}\"");
            if (enemy.HasRigidbody && enemy.Rigidbody.hasPlayerCollision)
            {
                musicLoop.LowPassIgnoreColliders.Add(enemy.Rigidbody.playerCollision);
            }
            motherPlayer = SemiFunc.PlayerAvatarGetFromPhotonID(playerID);
            if (enemy.HasHealth)
            {
                if (enemy.Health.onHurt == null)
                {
                    enemy.Health.onHurt = new UnityEvent();
                }
                enemy.Health.onHurt.AddListener(() => { WormDeath(); });
            }
            if (enemy.HasRigidbody)
            {
                for (int i = 0; i < enemy.Rigidbody.onGrabbed.GetPersistentEventCount(); i++)
                {
                    enemy.Rigidbody.onGrabbed.SetPersistentListenerState(i, UnityEventCallState.Off);
                }
                for (int i = 0; i < enemy.Rigidbody.onTouchPlayer.GetPersistentEventCount(); i++)
                {
                    enemy.Rigidbody.onTouchPlayer.SetPersistentListenerState(i, UnityEventCallState.Off);
                }
                for (int i = 0; i < enemy.Rigidbody.onTouchPlayerGrabbedObject.GetPersistentEventCount(); i++)
                {
                    enemy.Rigidbody.onTouchPlayerGrabbedObject.SetPersistentListenerState(i, UnityEventCallState.Off);
                }
            }
        }
        public void Update()
        {
            if (gameObject.activeSelf && SemiFunc.IsMasterClientOrSingleplayer())
            {
                if (lifetime > 0f)
                {
                    if (lifetime < 89f)
                    {
                        musicLoop.PlayLoop(!lowLife, 2f, 1f);
                    }
                    if (lifetime < 5f != lowLife)
                    {
                        LowLife(lifetime < 5f);
                    }
                    if (enemy.EnemyParent.SpawnedTimer > 0f)
                    {
                        lifetime -= Time.deltaTime;
                    }
                    enemy.DisableChase(1f);
                    if (slowChaseRef != null)
                    {
                        if (slowChaseRef.StateTimer > 0f)
                        {
                            slowChaseRef.StateTimer = 0f;
                        }
                    }
                    if (enemy.HasStateLookUnder)
                    {
                        if (enemy.StateLookUnder.LookTimer > 0f)
                        {
                            enemy.StateLookUnder.LookTimer = 0f;
                        }
                        if (enemy.StateLookUnder.WaitTimer > 0f)
                        {
                            enemy.StateLookUnder.WaitTimer = 0f;
                        }
                    }
                    if (enemy.HasVision)
                    {
                        if (enemy.Vision.DisableTimer <= 1f)
                        {
                            enemy.Vision.DisableVision(1f);
                        }
                    }
                    if (enemy.HasStateInvestigate)
                    {
                        if (investigateTimer > 0f)
                        {
                            investigateTimer -= Time.deltaTime;
                        }
                        else
                        {
                            LevelPoint levelPoint = SemiFunc.LevelPointGetFurthestFromPlayer(Vector3.zero, 10f);
                            if (levelPoint != null)
                            {
                                enemy.StateInvestigate.Set(levelPoint.transform.position);
                            }
                            investigateTimer = 10f;
                        }
                    }
                    if (!enemy.EnemyParent.forceLeave)
                    {
                        enemy.EnemyParent.forceLeave = true;
                    }
                    for (int i = 0; i < EnemyDirector.instance.enemiesSpawned.Count; i++)
                    {
                        EnemyParent spreadEnemy = EnemyDirector.instance.enemiesSpawned[i];
                        if (spreadEnemy.DespawnedTimer > 0f)
                        {
                            continue;
                        }
                        if (Vector3.Distance(transform.position, spreadEnemy.Enemy.CenterTransform.position) <= 2.5f && !spreadEnemy.WormData().infected)
                        {
                            WormSpread(spreadEnemy.Enemy);
                        }
                    }
                }
                else
                {
                    WormDeath();
                }
            }
        }
        public void LowLife(bool low)
        {
            if (SemiFunc.IsMultiplayer())
            {
                photonView.RPC("LowLifeRPC", RpcTarget.All, low);
            }
            else
            {
                LowLifeRPC(low);
            }
        }
        [PunRPC]
        public void LowLifeRPC(bool low)
        {
            lowLife = low;
            log.LogDebug($"Worm Nearing Death: \"{low}\"");
        }
        public void WormSpread(Enemy newEnemy)
        {
            if (gameObject.activeSelf)
            {
                WormInfectionData wormData = newEnemy.EnemyParent.WormData();
                if (wormData.hasWorm && !wormData.infected)
                {
                    WormAttach newWorm = wormData.worm;
                    log.LogDebug($"Worm spreading from a \"{enemy.EnemyParent.enemyName}\" to a \"{newEnemy.EnemyParent.enemyName}\"");
                    newWorm.enabled = true;
                    newWorm.WakeUp(motherPlayer.photonView.ViewID, SemiFunc.EnemyGetIndex(newEnemy));
                }
            }
        }
        public void WormDeath()
        {
            if (gameObject.activeSelf)
            {
                enemy.EnemyParent.forceLeave = false;
                if (enemy.HasStateInvestigate)
                {
                    enemy.StateInvestigate.Set(motherPlayer.transform.position);
                }
                if (SemiFunc.IsMultiplayer())
                {
                    photonView.RPC("WormDeathRPC", RpcTarget.All);
                }
                else
                {
                    WormDeathRPC();
                }
            }
        }
        [PunRPC]
        public void WormDeathRPC()
        {
            if (enemy.HasHealth)
            {
                enemy.Health.onHurt.RemoveListener(WormDeath);
            }
            if (enemy.HasRigidbody)
            {
                for (int i = 0; i < enemy.Rigidbody.onGrabbed.GetPersistentEventCount(); i++)
                {
                    enemy.Rigidbody.onGrabbed.SetPersistentListenerState(i, UnityEventCallState.RuntimeOnly);
                }
                for (int i = 0; i < enemy.Rigidbody.onTouchPlayer.GetPersistentEventCount(); i++)
                {
                    enemy.Rigidbody.onTouchPlayer.SetPersistentListenerState(i, UnityEventCallState.RuntimeOnly);
                }
                for (int i = 0; i < enemy.Rigidbody.onTouchPlayerGrabbedObject.GetPersistentEventCount(); i++)
                {
                    enemy.Rigidbody.onTouchPlayerGrabbedObject.SetPersistentListenerState(i, UnityEventCallState.RuntimeOnly);
                }
            }
            log.LogDebug($"Killing Worm");
            enemy.EnemyParent.WormData().infected = false;
            gameObject.SetActive(false);
            enabled = false;
        }
    }
    [Serializable]
    public class WormInfectionData
    {
        public bool hasWorm;
        public bool infected;
        public WormAttach worm;
        public WormInfectionData()
        {
            hasWorm = false;
            infected = false;
            worm = null;
        }
    }
}