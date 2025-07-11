using Photon.Pun;
using System.Collections;
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
        public GameObject hudAudio;
        public int extractFinishedPlayers = 0;
        public int playersEmeraldsSet = 0;
        public Coroutine extractCoroutine;
        public void Awake()
        {
            hudAudio = WildCardMod.instance.miscPrefabsList.Find((x) => x.name == "Wildcard HUD Audio");
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
                    photonView.RPC("PickColorRPC", RpcTarget.All, randomColour, colourMap[randomColour].r, colourMap[randomColour].g, colourMap[randomColour].b, colourMap[randomColour].a);
                }
                else
                {
                    PickColorRPC(randomColour, colourMap[randomColour].r, colourMap[randomColour].g, colourMap[randomColour].b, colourMap[randomColour].a);
                }
            }
        }
        [PunRPC]
        public void PickColorRPC(string colourName, float r, float g, float b, float a)
        {
            colour = colourName;
            Color chosenColour = new Color(r, g, b, a);
            meshRenderer.material.SetColor("_BaseColor", chosenColour);
            GradientColorKey[] colorKeys = valuableObject.particleColors.colorKeys;
            for (int i = 0; i < colorKeys.Length; i++)
            {
                colorKeys[i] = new GradientColorKey(chosenColour, colorKeys[i].time);
            }
            valuableObject.particleColors.colorKeys = colorKeys;
            if (colour == "White")
            {
                meshRenderer.material.SetColor("_EmissionColor", new Color(0.4f, 0.4f, 0.4f, 1f));
            }
            else
            {
                meshRenderer.material.SetColor("_EmissionColor", new Color(r / 2f, g / 2f, b / 2f, 1f));
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
        [PunRPC]
        public void AllAddPlayerEmeraldRPC()
        {
            log.LogDebug("Adding a Chaos Emerald point");
            playersEmeraldsSet = 0;
            StatsUI.instance.Fetch();
            StatsUI.instance.ShowStats();
            CameraGlitch.Instance.PlayUpgrade();
            GameDirector.instance.CameraImpact.ShakeDistance(5f, 1f, 6f, SemiFunc.PlayerAvatarLocal().transform.position, 0.2f);
        }
        [PunRPC]
        public void ExtractCompleteRPC()
        {
            extractFinishedPlayers += 1;
            log.LogDebug($"extractFinishedPlayers == {extractFinishedPlayers}");
            if (extractCoroutine == null)
            {
                extractCoroutine = StartCoroutine(ExtractCompleteCoroutine());
            }
        }
        public IEnumerator ExtractCompleteCoroutine()
        {
            yield return new WaitUntil(() => extractFinishedPlayers >= SemiFunc.PlayerGetList().Count);
            log.LogDebug("Calling DestroyEmerald()");
            DestroyEmerald();
        }
        public void MasterAddPlayerEmerald()
        {
            if (SemiFunc.IsMultiplayer())
            {
                photonView.RPC("AllAddPlayerEmeraldRPC", RpcTarget.All);
            }
            else
            {
                AllAddPlayerEmeraldRPC();
            }
            string steamID = SemiFunc.PlayerGetSteamID(SemiFunc.PlayerAvatarLocal());
            StatsManager.instance.dictionaryOfDictionaries["playerUpgradeChaosEmeralds"][steamID]++;
            StatsManager.instance.dictionaryOfDictionaries["chaosEmeraldsUnique"][colour]++;
            if (StatsManager.instance.dictionaryOfDictionaries["playerUpgradeChaosEmeralds"][steamID] >= 7)
            {
                StatsManager.instance.dictionaryOfDictionaries["playerUpgradeChaosEmeralds"][steamID] = 0;
                StatsManager.instance.DictionaryFill("chaosEmeraldsUnique", 0);
                SuperSonic();
            }
            List<PlayerAvatar> players = SemiFunc.PlayerGetAll();
            for (int i = 0; i < players.Count; i++)
            {
                players[i].playerHealth.MaterialEffectOverride(PlayerHealth.Effect.Upgrade);
            }
            if (SemiFunc.IsMultiplayer())
            {
                photonView.RPC("PropogateEmeraldsRPC", RpcTarget.Others, StatsManager.instance.dictionaryOfDictionaries["playerUpgradeChaosEmeralds"][steamID]);
            }
            ExtractCompleteRPC();
        }
        public void SuperSonic()
        {
            SuperSonic existingSonic = PlayerController.instance.transform.GetComponentInChildren<SuperSonic>();
            if (existingSonic != null)
            {
                if (SemiFunc.IsMultiplayer())
                {
                    existingSonic.photonView.RPC("DoubleTimerRPC", RpcTarget.All);
                }
                else
                {
                    existingSonic.DoubleTimerRPC();
                }
                return;
            }
            if (SemiFunc.IsMultiplayer())
            {
                GameObject newObject = PhotonNetwork.InstantiateRoomObject("Misc/Wildcard HUD Audio", Vector3.zero, Quaternion.identity);
                photonView.RPC("SuperSonicRPC", RpcTarget.All, newObject.GetComponent<PhotonView>().ViewID);
            }
            else
            {
                SuperSonicLogic(Instantiate(hudAudio));
            }
        }
        [PunRPC]
        public void SuperSonicRPC(int id = -1)
        {
            GameObject newObject = PhotonView.Find(id).gameObject;
            if (SemiFunc.PlayerAvatarLocal().deadSet)
            {
                Destroy(newObject);
                return;
            }
            SuperSonicLogic(newObject);
        }
        public void SuperSonicLogic(GameObject newObject)
        {
            sonicLoop.Source = newObject.GetComponent<AudioSource>();
            newObject.transform.parent = PlayerController.instance.transform;
            newObject.transform.localPosition = Vector3.zero;
            SuperSonic superSonic = newObject.AddComponent<SuperSonic>();
            superSonic.sonicLoop = new Sound { Source = sonicLoop.Source, Sounds = sonicLoop.Sounds, Type = sonicLoop.Type, Volume = sonicLoop.Volume, VolumeRandom = sonicLoop.VolumeRandom, Pitch = sonicLoop.Pitch, PitchRandom = sonicLoop.PitchRandom, SpatialBlend = sonicLoop.SpatialBlend, ReverbMix = sonicLoop.ReverbMix, Doppler = sonicLoop.Doppler };
            superSonic.photonView = newObject.GetComponent<PhotonView>();
        }
        [PunRPC]
        public void PropogateEmeraldsRPC(int emeralds)
        {
            List<PlayerAvatar> players = SemiFunc.PlayerGetAll();
            for (int i = 0; i < players.Count; i++)
            {
                StatsManager.instance.dictionaryOfDictionaries["playerUpgradeChaosEmeralds"][SemiFunc.PlayerGetSteamID(players[i])] = emeralds;
            }
            photonView.RPC("ExtractCompleteRPC", RpcTarget.MasterClient);
        }
        public void DestroyEmerald()
        {
            if (SemiFunc.IsMultiplayer())
            {
                physGrabObject.photonView.RPC("DestroyPhysGrabObjectRPC", RpcTarget.All);
            }
            else
            {
                physGrabObject.DestroyPhysGrabObjectRPC();
            }
        }
    }
    public class SuperSonic : MonoBehaviour
    {
        public PhotonView photonView;
        public float overrideTimer;
        public Sound sonicLoop;
        public PlayerAvatar[] playersList;
        public void Start()
        {
            overrideTimer = 120f;
            playersList = SemiFunc.PlayerGetAll().ToArray();
            sonicLoop.LowPassIgnoreColliders.Add(PlayerController.instance.col);
        }
        [PunRPC]
        public void DoubleTimerRPC()
        {
            overrideTimer *= 2f;
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