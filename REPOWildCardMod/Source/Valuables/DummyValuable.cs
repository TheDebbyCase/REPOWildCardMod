using UnityEngine;
namespace REPOWildCardMod.Valuables
{
    public class DummyValuable : MonoBehaviour
    {
        readonly BepInEx.Logging.ManualLogSource log = WildCardMod.instance.log;
        public GameObject prefab;
        public void Awake()
        {
            if (SemiFunc.IsMasterClientOrSingleplayer())
            {
                if (prefab.TryGetComponent(out ItemAttributes itemAttributes))
                {
                    log.LogDebug($"Spawning {itemAttributes.item.itemName} from dummy!");
                    GameObject newPrefab = REPOLib.Modules.Items.SpawnItem(itemAttributes.item, transform.position, transform.rotation);
                    newPrefab.SetActive(true);
                    log.LogDebug($"Spawned {prefab.name}!");
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