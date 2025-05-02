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
                switch (script)
                {
                    case Item item:
                        {
                            item = StatsManager.instance.itemDictionary[item.itemAssetName];
                            log.LogDebug($"Spawning {item.itemName} from dummy!");
                            GameObject prefab = REPOLib.Modules.Items.SpawnItem(item, transform.position, transform.rotation); ;
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