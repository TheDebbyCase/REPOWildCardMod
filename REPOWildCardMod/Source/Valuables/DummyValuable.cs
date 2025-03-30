using Photon.Pun;
using REPOLib.Modules;
using UnityEngine;
namespace REPOWildCardMod.Valuables
{
    public class DummyValuable : MonoBehaviour
    {
        readonly BepInEx.Logging.ManualLogSource log = WildCardMod.instance.log;
        public ScriptableObject script;
        public void Awake()
        {
            if (SemiFunc.IsMasterClientOrSingleplayer())
            {
                switch (script.GetType().ToString())
                {
                    case "Item":
                        {
                            Item item = StatsManager.instance.itemDictionary[(script as Item).itemAssetName];
                            GameObject prefab;
                            log.LogDebug($"Spawning {item.itemName} from dummy!");
                            if (SemiFunc.IsMultiplayer())
                            {
                                prefab = PhotonNetwork.InstantiateRoomObject(ResourcesHelper.GetItemPrefabPath(item), new Vector3(transform.position.x, transform.position.y + 0.125f, transform.position.z), item.spawnRotationOffset, 0);
                            }
                            else
                            {
                                prefab = Instantiate(item.prefab, new Vector3(transform.position.x, transform.position.y + 0.125f, transform.position.z + 0.25f), item.spawnRotationOffset);
                            }
                            prefab.SetActive(true);
                            log.LogDebug($"Spawned {prefab.name}!");
                            break;
                        }
                    default:
                        {
                            log.LogWarning($"{name} was unable to spawn!");
                            break;
                        }
                }
                ValuableDirector.instance.totalMaxAmount++;
                ValuableDirector.instance.valuableTargetAmount--;
            }
        }
        public void Start()
        {
            if (SemiFunc.IsMasterClientOrSingleplayer())
            {
                transform.GetComponent<PhysGrabObject>().DestroyPhysGrabObject();
            }
        }
    }
}