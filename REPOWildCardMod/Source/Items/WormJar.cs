using UnityEngine;
using Photon.Pun;
using REPOWildCardMod.Utils;
using REPOLib.Extensions;
using UnityEngine.Events;
namespace REPOWildCardMod.Items
{
    public class WormJar : MonoBehaviour
    {
        static readonly BepInEx.Logging.ManualLogSource log = WildCardMod.instance.log;
        readonly WildCardUtils utils = WildCardMod.instance.utils;
        public PhotonView photonView;
        public PhysGrabObject physGrabObject;
        public PhysGrabObjectImpactDetector impactDetector;
        public ItemAttributes itemAttributes;
        public PlayerAvatar lastPlayerGrabbed;
        public GameObject wormPrefab;
        public Sprite icon;
        public float noDestroyCooldown;
        public bool firstRunGrab = true;
        public void Awake()
        {
            if (itemAttributes.icon != icon)
            {
                itemAttributes.icon = icon;
            }
            wormPrefab = WildCardMod.instance.miscPrefabsList.Find((x) => x.name == "Worm Attach");
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
                Enemy enemy = SemiFunc.EnemyGetNearest(transform.position, 1f, true);
                if (enemy == null || (enemy != null && enemy.gameObject.GetComponent<WormAttach>() != null))
                {
                    impactDetector.destroyDisable = true;
                    noDestroyCooldown = 0.1f;
                }
                else
                {
                    log.LogDebug($"Breaking Worm Jar");
                    NewWorm(lastPlayerGrabbed.photonView.ViewID, SemiFunc.EnemyGetIndex(enemy));
                }
            }
        }
        public void NewWorm(int playerID, int enemyIndex)
        {
            GameObject newWorm;
            Enemy enemy = SemiFunc.EnemyGetFromIndex(enemyIndex);
            if (SemiFunc.IsMultiplayer())
            {
                newWorm = PhotonNetwork.InstantiateRoomObject($"Misc/Worm Attach", utils.FindEnemyTransform(enemy.EnemyParent.enemyName, "Head").position, Quaternion.identity);
            }
            else
            {
                newWorm = Instantiate(wormPrefab);
            }
            newWorm.GetComponent<WormAttach>().Initialize(playerID, enemyIndex);
        }
    }
    public class WormAttach : MonoBehaviour
    {
        static readonly BepInEx.Logging.ManualLogSource log = WildCardMod.instance.log;
        readonly WildCardUtils utils = WildCardMod.instance.utils;
        public PhotonView photonView;
        public Sound musicLoop;
        public Enemy enemy;
        public PlayerAvatar motherPlayer;
        public SphereCollider spreadOverlap;
        public float lifetime;
        public float investigateTimer;
        public bool lowLife = false;
        public void Initialize(int playerID, int enemyIndex)
        {
            lifetime = Random.Range(60f, 120f);
            if (SemiFunc.IsMultiplayer())
            {
                photonView.RPC("InitializeRPC", RpcTarget.All, playerID, enemyIndex);
            }
            else
            {
                InitializeRPC(playerID, enemyIndex);
            }
        }
        [PunRPC]
        public void InitializeRPC(int playerID, int enemyIndex)
        {
            gameObject.FixAudioMixerGroups();
            enemy = SemiFunc.EnemyGetFromIndex(enemyIndex);
            log.LogDebug($"Worm has infected a \"{enemy.EnemyParent.enemyName}\"");
            if (enemy.HasRigidbody && enemy.Rigidbody.hasPlayerCollision)
            {
                musicLoop.LowPassIgnoreColliders.Add(enemy.Rigidbody.playerCollision);
            }
            transform.parent = utils.FindEnemyTransform(enemy.EnemyParent.enemyName, "Head");
            motherPlayer = SemiFunc.PlayerAvatarGetFromPhotonID(playerID);
            if (enemy.HasHealth)
            {
                if (enemy.Health.onHurt == null)
                {
                    enemy.Health.onHurt = new UnityEvent();
                }
                enemy.Health.onHurt.AddListener(() => { WormDeath(); });
            }
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
            log.LogDebug($"Worm added to \"{enemy.EnemyParent.enemyName}\", new local position: \"{overrideLocalPos}\", new local rotation: \"{overrideLocalRot}\", new local scale: \"{overrideLocalScale}\"");
            if (enemy.HasRigidbody)
            {
                spreadOverlap.radius = (enemy.Rigidbody.transform.GetComponentInChildren<Collider>().bounds.size.magnitude * 2f) / overrideLocalScale.magnitude;
            }
            transform.localPosition = overrideLocalPos;
            transform.localRotation = Quaternion.Euler(overrideLocalRot);
            transform.localScale = overrideLocalScale;
        }
        public void Update()
        {
            musicLoop.PlayLoop(!lowLife, 2f, 1f);
            if (SemiFunc.IsMasterClientOrSingleplayer())
            {
                if (lifetime > 0f)
                {
                    if (lifetime < 5f != lowLife)
                    {
                        LowLife(lifetime < 5f);
                    }
                    if (enemy.EnemyParent.SpawnedTimer > 0f)
                    {
                        lifetime -= Time.deltaTime;
                    }
                    if (enemy.HasVision && enemy.Vision.DisableTimer <= 1f)
                    {
                        enemy.Vision.DisableVision(1f);
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
            if (SemiFunc.IsMasterClientOrSingleplayer() && enemy.EnemyParent.transform.GetComponentInChildren<WormAttach>() == null)
            {
                log.LogDebug($"Worm spreading from a \"{enemy.EnemyParent.enemyName}\" to a \"{newEnemy.EnemyParent.enemyName}\"");
                NewWorm(motherPlayer.photonView.ViewID, SemiFunc.EnemyGetIndex(newEnemy));
            }
        }
        public void NewWorm(int playerID, int enemyIndex)
        {
            GameObject newWorm;
            Enemy enemy = SemiFunc.EnemyGetFromIndex(enemyIndex);
            if (SemiFunc.IsMultiplayer())
            {
                newWorm = PhotonNetwork.InstantiateRoomObject("Misc/Worm Attach", utils.FindEnemyTransform(enemy.EnemyParent.enemyName, "Head").position, Quaternion.identity);
                newWorm.FixAudioMixerGroups();
            }
            else
            {
                newWorm = Instantiate(gameObject);
            }
            newWorm.GetComponent<WormAttach>().Initialize(playerID, enemyIndex);
        }
        public void WormDeath()
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
        [PunRPC]
        public void WormDeathRPC()
        {
            if (enemy.HasHealth)
            {
                enemy.Health.onHurt.RemoveListener(WormDeath);
            }
            log.LogDebug($"Killing Worm");
            Destroy(gameObject);
        }
        public void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.CompareTag("Phys Grab Object"))
            {
                PhysGrabObject physGrabObject = other.gameObject.GetComponent<PhysGrabObject>();
                if (physGrabObject == null)
                {
                    physGrabObject = other.gameObject.GetComponentInParent<PhysGrabObject>();
                }
                if (physGrabObject != null && physGrabObject.isEnemy)
                {
                    WormSpread(physGrabObject.transform.GetComponent<EnemyRigidbody>().enemy);
                }
            }
        }
    }
}