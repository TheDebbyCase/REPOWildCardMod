using System.Collections.Generic;
using UnityEngine;
namespace REPOWildCardMod.Valuables
{
    public class DragonBall : MonoBehaviour
    {
        static readonly BepInEx.Logging.ManualLogSource log = WildCardMod.instance.log;
        public StatsManager statsManager = StatsManager.instance;
        public void AddPlayerBall()
        {
            List<PlayerAvatar> players = SemiFunc.PlayerGetAll();
            for (int i = 0; i < players.Count; i++)
            {
                if (players[i].isLocal)
                {
                    StatsUI.instance.Fetch();
                    StatsUI.instance.ShowStats();
                    CameraGlitch.Instance.PlayUpgrade();
                }
                GameDirector.instance.CameraImpact.ShakeDistance(5f, 1f, 6f, players[i].clientPosition, 0.2f);
                if (SemiFunc.IsMasterClientOrSingleplayer())
                {
                    players[i].playerHealth.MaterialEffectOverride(PlayerHealth.Effect.Upgrade);
                }
            }
            log.LogDebug("Adding a Dragon Ball point to master client");
            if (SemiFunc.IsMasterClientOrSingleplayer())
            {
                Dictionary<string, int> dictionary = statsManager.dictionaryOfDictionaries["playerUpgradeDragonBalls"];
                PlayerAvatar local = SemiFunc.PlayerAvatarLocal();
                dictionary[local.steamID]++;
                if (dictionary[local.steamID] >= 7)
                {
                    RoundDirector.instance.totalHaul += Mathf.CeilToInt(Mathf.Max(10000, Mathf.Min(50000, (39000 / ((Mathf.Pow((float)dictionary.Count + 3f, 2f)) / 16f)) + 11000)));
                    dictionary[local.steamID] = 0;
                }
                statsManager.dictionaryOfDictionaries["playerUpgradeDragonBalls"] = dictionary;
            }
        }
    }
}