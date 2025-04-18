using UnityEngine;
using Photon.Pun;
using REPOWildCardMod.Utils;
namespace REPOWildCardMod.Items
{
    public class WormJar : MonoBehaviour
    {
        static readonly BepInEx.Logging.ManualLogSource log = WildCardMod.instance.log;
        readonly WildCardUtils utils = WildCardMod.instance.utils;
        public PhotonView photonView;
        public PhysGrabObject physGrabObject;
        public PhysGrabObjectImpactDetector impactDetector;
        public PlayerAvatar lastPlayerGrabbed;
        public GameObject wormPrefab;
        public float noDestroyCooldown;
        public void Update()
        {
            if (SemiFunc.IsMasterClientOrSingleplayer())
            {
                if (physGrabObject.playerGrabbing.Count == 1 && lastPlayerGrabbed != physGrabObject.playerGrabbing[0].playerAvatar)
                {
                    lastPlayerGrabbed = physGrabObject.playerGrabbing[0].playerAvatar;
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
        }
        public void BreakJar()
        {
            if (SemiFunc.IsMasterClientOrSingleplayer())
            {
                Enemy enemy = SemiFunc.EnemyGetNearest(transform.position, 0.5f, true);
                if (enemy == null || (enemy != null && enemy.gameObject.GetComponent<WormAttach>() != null))
                {
                    impactDetector.destroyDisable = true;
                    noDestroyCooldown = 0.25f;
                }
                else
                {
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
                newWorm = PhotonNetwork.InstantiateRoomObject("Misc/Worm Attach", utils.FindEnemyTransform(enemy.EnemyParent.enemyName, "Head").position, Quaternion.identity);
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
        public Enemy enemy;
        public PlayerAvatar motherPlayer;
        public float lifetime;
        public bool onlyEventOriginal;
        public void Initialize(int playerID, int enemyIndex)
        {
            lifetime = Random.Range(30f, 120f);
            if (SemiFunc.IsMultiplayer())
            {
                enemy.photonView.RPC("InitializeRPC", RpcTarget.All, playerID, enemyIndex);
            }
            else
            {
                InitializeRPC(playerID, enemyIndex);
            }
        }
        [PunRPC]
        public void InitializeRPC(int playerID, int enemyIndex)
        {
            if (enemy.HasStateInvestigate)
            {
                onlyEventOriginal = enemy.StateInvestigate.OnlyEvent;
                ToggleInvestigate(false);
            }
            if (enemy.HasHealth)
            {
                enemy.Health.onHurt.AddListener(WormDeath);
            }
            if (enemy.HasRigidbody)
            {
                enemy.Rigidbody.onTouchPhysObject.AddListener(WormSpread);
            }
            transform.parent = utils.FindEnemyTransform(enemy.EnemyParent.enemyName, "Head");
            motherPlayer = SemiFunc.PlayerAvatarGetFromPhotonID(playerID);
            enemy = SemiFunc.EnemyGetFromIndex(enemyIndex);
            Vector3 overrideLocalPos;
            Vector3 overrideLocalRot;
            Vector3 overrideLocalScale;
            switch (enemy.EnemyParent.enemyName)
            {
                case "Animal":
                    {
                        overrideLocalPos = Vector3.zero;
                        overrideLocalRot = Vector3.zero;
                        overrideLocalScale = Vector3.one;
                        break;
                    }
                case "Apex Predator":
                    {
                        overrideLocalPos = Vector3.zero;
                        overrideLocalRot = Vector3.zero;
                        overrideLocalScale = Vector3.one;
                        break;
                    }
                case "Banger":
                    {
                        overrideLocalPos = Vector3.zero;
                        overrideLocalRot = Vector3.zero;
                        overrideLocalScale = Vector3.one;
                        break;
                    }
                case "Bowtie":
                    {
                        overrideLocalPos = Vector3.zero;
                        overrideLocalRot = Vector3.zero;
                        overrideLocalScale = Vector3.one;
                        break;
                    }
                case "Chef":
                    {
                        overrideLocalPos = Vector3.zero;
                        overrideLocalRot = Vector3.zero;
                        overrideLocalScale = Vector3.one;
                        break;
                    }
                case "Clown":
                    {
                        overrideLocalPos = Vector3.zero;
                        overrideLocalRot = Vector3.zero;
                        overrideLocalScale = Vector3.one;
                        break;
                    }
                case "Gnome":
                    {
                        overrideLocalPos = Vector3.zero;
                        overrideLocalRot = Vector3.zero;
                        overrideLocalScale = Vector3.one;
                        break;
                    }
                case "Headman":
                    {
                        overrideLocalPos = Vector3.zero;
                        overrideLocalRot = Vector3.zero;
                        overrideLocalScale = Vector3.one;
                        break;
                    }
                case "Hidden":
                    {
                        overrideLocalPos = Vector3.zero;
                        overrideLocalRot = Vector3.zero;
                        overrideLocalScale = Vector3.one;
                        break;
                    }
                case "Huntsman":
                    {
                        overrideLocalPos = Vector3.zero;
                        overrideLocalRot = Vector3.zero;
                        overrideLocalScale = Vector3.one;
                        break;
                    }
                case "Mentalist":
                    {
                        overrideLocalPos = Vector3.zero;
                        overrideLocalRot = Vector3.zero;
                        overrideLocalScale = Vector3.one;
                        break;
                    }
                case "Peeper":
                    {
                        overrideLocalPos = Vector3.zero;
                        overrideLocalRot = Vector3.zero;
                        overrideLocalScale = Vector3.one;
                        break;
                    }
                case "Reaper":
                    {
                        overrideLocalPos = Vector3.zero;
                        overrideLocalRot = Vector3.zero;
                        overrideLocalScale = Vector3.one;
                        break;
                    }
                case "Robe":
                    {
                        overrideLocalPos = Vector3.zero;
                        overrideLocalRot = Vector3.zero;
                        overrideLocalScale = Vector3.one;
                        break;
                    }
                case "Rugrat":
                    {
                        overrideLocalPos = Vector3.zero;
                        overrideLocalRot = Vector3.zero;
                        overrideLocalScale = Vector3.one;
                        break;
                    }
                case "Shadow Child":
                    {
                        overrideLocalPos = Vector3.zero;
                        overrideLocalRot = Vector3.zero;
                        overrideLocalScale = Vector3.one;
                        break;
                    }
                case "Spewer":
                    {
                        overrideLocalPos = Vector3.zero;
                        overrideLocalRot = Vector3.zero;
                        overrideLocalScale = Vector3.one;
                        break;
                    }
                case "Trudge":
                    {
                        overrideLocalPos = Vector3.zero;
                        overrideLocalRot = Vector3.zero;
                        overrideLocalScale = Vector3.one;
                        break;
                    }
                case "Upscream":
                    {
                        overrideLocalPos = Vector3.zero;
                        overrideLocalRot = Vector3.zero;
                        overrideLocalScale = Vector3.one;
                        break;
                    }
                default:
                    {
                        overrideLocalPos = Vector3.zero;
                        overrideLocalRot = Vector3.zero;
                        overrideLocalScale = Vector3.one;
                        break;
                    }
            }
            transform.localPosition = overrideLocalPos;
            transform.localRotation = Quaternion.Euler(overrideLocalRot);
            transform.localScale = overrideLocalScale;
        }
        public void Update()
        {
            if (SemiFunc.IsMasterClientOrSingleplayer())
            {
                if (lifetime > 0f)
                {
                    if (enemy.EnemyParent.SpawnedTimer > 0f)
                    {
                        lifetime -= Time.deltaTime;
                    }
                    if (enemy.HasVision)
                    {
                        enemy.Vision.DisableVision(1f);
                    }
                    enemy.EnemyParent.forceLeave = true;
                }
                else
                {
                    WormDeath();
                }
            }
        }
        public void ToggleInvestigate(bool original)
        {
            if (SemiFunc.IsMultiplayer())
            {
                enemy.photonView.RPC("ToggleInvestigateRPC", RpcTarget.All, original);
            }
            else
            {
                ToggleInvestigateRPC(original);
            }
        }
        [PunRPC]
        public void ToggleInvestigateRPC(bool original)
        {
            if (original)
            {
                enemy.StateInvestigate.OnlyEvent = onlyEventOriginal;
                for (int i = 0; i < enemy.StateInvestigate.onInvestigateTriggered.GetPersistentEventCount(); i++)
                {
                    enemy.StateInvestigate.onInvestigateTriggered.SetPersistentListenerState(i, UnityEngine.Events.UnityEventCallState.RuntimeOnly);
                }
            }
            else
            {
                enemy.StateInvestigate.OnlyEvent = true;
                for (int i = 0; i < enemy.StateInvestigate.onInvestigateTriggered.GetPersistentEventCount(); i++)
                {
                    enemy.StateInvestigate.onInvestigateTriggered.SetPersistentListenerState(i, UnityEngine.Events.UnityEventCallState.Off);
                }
            }
        }
        public void WormSpread()
        {
            if (SemiFunc.IsMasterClientOrSingleplayer())
            {
                PhysGrabObject physGrabObject = enemy.Rigidbody.onTouchPhysObjectPhysObject;
                if (physGrabObject.isEnemy)
                {
                    NewWorm(motherPlayer.photonView.ViewID, SemiFunc.EnemyGetIndex(physGrabObject.GetComponent<EnemyRigidbody>().enemy));
                }
            }
        }
        public void NewWorm(int playerID, int enemyIndex)
        {
            GameObject newWorm;
            Enemy enemy = SemiFunc.EnemyGetFromIndex(enemyIndex);
            if (SemiFunc.IsMultiplayer())
            {
                newWorm = PhotonNetwork.InstantiateRoomObject("Misc/Worm Attach", utils.FindEnemyTransform(enemy.EnemyParent.enemyName, "Head").position, Quaternion.identity);
            }
            else
            {
                newWorm = Instantiate(gameObject);
            }
            newWorm.GetComponent<WormAttach>().Initialize(playerID, enemyIndex);
        }
        public void WormDeath()
        {
            if (SemiFunc.IsMultiplayer())
            {
                enemy.photonView.RPC("WormDeathRPC", RpcTarget.All);
            }
            else
            {
                WormDeathRPC();
            }
        }
        [PunRPC]
        public void WormDeathRPC()
        {
            if (enemy.HasStateInvestigate)
            {
                if (SemiFunc.IsMasterClientOrSingleplayer())
                {
                    ToggleInvestigate(true);
                }
                enemy.StateInvestigate.Set(motherPlayer.PlayerVisionTarget.VisionTransform.transform.position);
            }
            if (enemy.HasRigidbody)
            {
                enemy.Rigidbody.onTouchPhysObject.RemoveListener(WormSpread);
            }
            if (enemy.HasHealth)
            {
                enemy.Health.onHurt.RemoveListener(WormDeath);
            }
            Destroy(this);
        }
    }
}