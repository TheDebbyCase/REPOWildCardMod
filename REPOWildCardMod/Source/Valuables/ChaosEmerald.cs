using Photon.Pun;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
namespace REPOWildCardMod.Valuables
{
    public class ChaosEmerald : MonoBehaviour
    {
        static readonly BepInEx.Logging.ManualLogSource log = WildCardMod.instance.log;
        public PhotonView photonView;
        public ValuableObject valuableObject;
        public PhysGrabObject physGrabObject;
        public Color[] colours;
        public Dictionary<string, Color> colourMap;
        public string colour;
        public MeshRenderer meshRenderer;
        public Sound sonicLoop;
        public void Awake()
        {
            if (SemiFunc.IsMasterClientOrSingleplayer())
            {
                string[] colourNames = StatsManager.instance.dictionaryOfDictionaries["chaosEmeraldsUnique"].Keys.ToArray();
                colourMap = new Dictionary<string, Color>();
                for (int i = 0; i < StatsManager.instance.dictionaryOfDictionaries["chaosEmeraldsUnique"].Keys.Count; i++)
                {
                    if (StatsManager.instance.dictionaryOfDictionaries["chaosEmeraldsUnique"][colourNames[i]] == 0)
                    {
                        colourMap.Add(colourNames[i], colours[i]);
                    }
                }
            }
        }
        public void Start()
        {
            if (SemiFunc.IsMasterClientOrSingleplayer())
            {
                string[] colourNames = colourMap.Keys.ToArray();
                string randomColour = colourNames[Random.Range(0, colourNames.Length)];
                if (SemiFunc.IsMultiplayer())
                {
                    photonView.RPC("PickColorRPC", RpcTarget.All, randomColour);
                }
                else
                {
                    PickColorRPC(randomColour);
                }
            }
        }
        [PunRPC]
        public void PickColorRPC(string colourName)
        {
            colour = colourName;
            meshRenderer.material.SetColor("_BaseColor", colourMap[colour]);
            GradientColorKey[] colorKeys = valuableObject.particleColors.colorKeys;
            for (int i = 0; i < colorKeys.Length; i++)
            {
                colorKeys[i] = new GradientColorKey(colourMap[colour], colorKeys[i].time);
            }
            valuableObject.particleColors.colorKeys = colorKeys;
            if (colour == "White")
            {
                meshRenderer.material.SetColor("_EmissionColor", new Color(0.4f, 0.4f, 0.4f, 1f));
            }
            else
            {
                meshRenderer.material.SetColor("_EmissionColor", new Color(colourMap[colour].r / 2f, colourMap[colour].g / 2f, colourMap[colour].b / 2f, 1f));
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
                        PlayerController.instance.OverrideSpeed(2f);
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
                StatsManager.instance.dictionaryOfDictionaries["chaosEmeraldsUnique"][colour]++;
                if (StatsManager.instance.dictionaryOfDictionaries["playerUpgradeChaosEmeralds"][steamID] >= 7)
                {
                    StatsManager.instance.dictionaryOfDictionaries["playerUpgradeChaosEmeralds"][steamID] = 0;
                    StatsManager.instance.DictionaryFill("chaosEmeraldsUnique", 0);
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
                if (components[i].GetType() == typeof(Animator))
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
            superSonic.sonicLoop = new Sound { Source = sonicLoop.Source, Sounds = sonicLoop.Sounds, Type = sonicLoop.Type, Volume = sonicLoop.Volume, VolumeRandom = sonicLoop.VolumeRandom, Pitch = sonicLoop.Pitch, PitchRandom = sonicLoop.PitchRandom, SpatialBlend = sonicLoop.SpatialBlend, ReverbMix = sonicLoop.ReverbMix, Doppler = sonicLoop.Doppler };
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
    public class SuperSonic : MonoBehaviour
    {
        public float overrideTimer;
        public Sound sonicLoop;
        public PlayerAvatar[] playersList;
        public void Start()
        {
            overrideTimer = 120f;
            playersList = SemiFunc.PlayerGetAll().ToArray();
            sonicLoop.LowPassIgnoreColliders.Add(PlayerController.instance.col);
        }
        public void Update()
        {
            for (int i = 0; i < playersList.Length; i++)
            {
                if (playersList[i] != null)
                {
                    if (playersList[i].isLocal)
                    {
                        sonicLoop.PlayLoop(overrideTimer > 10f, 2f, 0.5f);
                        if (SemiFunc.IsMultiplayer())
                        {
                            PlayerAvatar.instance.voiceChat.OverridePitch(1.5f, 1f, 0.25f);
                        }
                        PlayerAvatar.instance.OverridePupilSize(0.3f, 4, 0.25f, 1f, 5f, 0.5f);
                        PlayerController.instance.OverrideSpeed(3f);
                        PlayerController.instance.OverrideAnimationSpeed(2.5f, 1f, 0.5f);
                        if (PlayerController.instance.rb.isKinematic)
                        {
                            PlayerController.instance.OverrideTimeScale(1f, -1f);
                        }
                        else
                        {
                            PlayerController.instance.OverrideTimeScale(2.5f);
                        }
                        if (PhysGrabber.instance.grabbedPhysGrabObject != null)
                        {
                            PhysGrabber.instance.grabbedPhysGrabObject.OverrideTorqueStrength(1.5f);
                        }
                        CameraZoom.Instance.OverrideZoomSet(90f, 0.1f, 1f, 0.5f, null, 0);
                        PostProcessing.Instance.SaturationOverride(50f, 0.5f, 0.1f, 0.1f, null);
                    }
                    else
                    {
                        playersList[i].voiceChat.OverridePitch(1.5f, 1f, 0.25f);
                    }
                }
            }
            overrideTimer -= Time.deltaTime;
            if (!SemiFunc.RunIsLevel() && overrideTimer > 5f)
            {
                overrideTimer = 5f;
            }
            if (overrideTimer <= 0f)
            {
                Destroy(this.gameObject);
            }
        }
    }
}