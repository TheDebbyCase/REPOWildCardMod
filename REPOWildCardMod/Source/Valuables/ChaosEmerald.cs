using Photon.Pun;
using REPOWildCardMod.Utils;
using System.Collections.Generic;
using UnityEngine;
namespace REPOWildCardMod.Valuables
{
    public class ChaosEmerald : MonoBehaviour
    {
        static readonly BepInEx.Logging.ManualLogSource log = WildCardMod.instance.log;
        public PhotonView photonView;
        public ValuableObject valuableObject;
        public PhysGrabObject physGrabObject;
        public Color[] colors;
        public MeshRenderer meshRenderer;
        public Sound sonicLoop;
        public void Start()
        {
            if (SemiFunc.IsMasterClientOrSingleplayer())
            {
                if (SemiFunc.IsMultiplayer())
                {
                    photonView.RPC("PickColorRPC", RpcTarget.All, Random.Range(0, colors.Length));
                }
                else
                {
                    PickColorRPC(Random.Range(0, colors.Length));
                }
            }
        }
        [PunRPC]
        public void PickColorRPC(int index)
        {
            meshRenderer.material.SetColor("_BaseColor", colors[index]);
            GradientColorKey[] colorKeys = valuableObject.particleColors.colorKeys;
            for (int i = 0; i < colorKeys.Length; i++)
            {
                colorKeys[i] = new GradientColorKey(colors[index], colorKeys[i].time);
            }
            valuableObject.particleColors.colorKeys = colorKeys;
            if (index == 6)
            {
                meshRenderer.material.SetColor("_EmissionColor", new Color(0.4f, 0.4f, 0.4f, 1f));
            }
            else
            {
                meshRenderer.material.SetColor("_EmissionColor", new Color(colors[index].r / 2f, colors[index].g / 2f, colors[index].b / 2f, 1f));
            }
        }
        public void Update()
        {
            if (physGrabObject.grabbed)
            {
                for (int i = 0; i < physGrabObject.playerGrabbing.Count; i++)
                {
                    if (physGrabObject.playerGrabbing[i].isLocal)
                    {
                        PlayerController.instance.OverrideSpeed(3f);
                    }
                }
            }
        }
        public void AddPlayerEmerald()
        {
            log.LogDebug("Adding a Chaos Emerald point");
            if (SemiFunc.IsMasterClientOrSingleplayer())
            {
                string steamID = SemiFunc.PlayerGetSteamID(SemiFunc.PlayerAvatarLocal());
                StatsManager.instance.dictionaryOfDictionaries["playerUpgradeChaosEmeralds"][steamID]++;
                if (StatsManager.instance.dictionaryOfDictionaries["playerUpgradeChaosEmeralds"][steamID] >= 7)
                {
                    StatsManager.instance.dictionaryOfDictionaries["playerUpgradeChaosEmeralds"][steamID] = 0;
                    SuperSonic();
                }
                if (SemiFunc.IsMultiplayer())
                {
                    photonView.RPC("PropogateEmeraldsRPC", RpcTarget.Others, StatsManager.instance.dictionaryOfDictionaries["playerUpgradeChaosEmeralds"][steamID], steamID);
                }
            }
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
        }
        public void SuperSonic()
        {
            GameObject newObject = Instantiate(CameraGlitch.Instance.gameObject);
            Component[] components = newObject.GetComponents<Component>();
            for (int i = 0; i < components.Length; i++)
            {
                if (components[i].GetType() != typeof(Animator) || components[i].GetType() != typeof(CameraGlitch))
                {
                    Destroy(components[i]);
                }
                else if (components[i].GetType() == typeof(AudioSource))
                {
                    sonicLoop.Source = components[i] as AudioSource;
                }
            }
            newObject.transform.parent = PlayerController.instance.transform;
            newObject.transform.localPosition = Vector3.zero;
            SuperSonic superSonic = newObject.AddComponent<SuperSonic>();
            superSonic.sonicLoop = sonicLoop;
        }
        [PunRPC]
        public void PropogateEmeraldsRPC(int emeralds, string masterID)
        {
            StatsManager.instance.dictionaryOfDictionaries["playerUpgradeChaosEmeralds"][masterID] = emeralds;
            string localSteamID = SemiFunc.PlayerGetSteamID(SemiFunc.PlayerAvatarLocal());
            if (emeralds != StatsManager.instance.dictionaryOfDictionaries["playerUpgradeChaosEmeralds"][localSteamID])
            {
                photonView.RPC("SetEmeraldsRPC", RpcTarget.All, localSteamID, emeralds);
            }
        }
        [PunRPC]
        public void SetEmeraldsRPC(string steamID, int emeralds)
        {
            StatsManager.instance.dictionaryOfDictionaries["playerUpgradeDragonBalls"][steamID] = emeralds;
        }
    }
}