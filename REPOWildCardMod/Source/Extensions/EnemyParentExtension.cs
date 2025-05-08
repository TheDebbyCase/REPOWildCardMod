using REPOWildCardMod.Items;
using System.Collections.Generic;
namespace REPOWildCardMod.Extensions
{
    public static class EnemyParentExtension
    {
        public static Dictionary<EnemyParent, WormInfectionData> wormDataDictionary = new Dictionary<EnemyParent, WormInfectionData>();
        public static void InitializeWormData(this EnemyParent enemyParent, WormAttach newWorm)
        {
            enemyParent.WormData().worm = newWorm;
            if (enemyParent.WormData().worm != null)
            {
                enemyParent.WormData().hasWorm = true;
            }
        }
        public static WormInfectionData WormData(this EnemyParent enemyParent)
        {
            if (!wormDataDictionary.ContainsKey(enemyParent))
            {
                wormDataDictionary.Add(enemyParent, new WormInfectionData());
            }
            return wormDataDictionary[enemyParent];
        }
    }
}