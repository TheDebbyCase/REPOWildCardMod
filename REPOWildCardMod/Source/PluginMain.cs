using UnityEngine;
using BepInEx;
using BepInEx.Logging;
using System;
using System.Reflection;
using System.IO;
using System.Collections.Generic;
using REPOWildCardMod.Config;
using HarmonyLib;
using REPOWildCardMod.Utils;
using REPOWildCardMod.Valuables;
using REPOWildCardMod.Patches;
using REPOLib.Modules;
namespace REPOWildCardMod
{
    [BepInPlugin(modGUID, modName, modVersion)]
    [BepInDependency(REPOLib.MyPluginInfo.PLUGIN_GUID, BepInDependency.DependencyFlags.HardDependency)]
    [BepInDependency("bulletbot.moreupgrades", BepInDependency.DependencyFlags.SoftDependency)]
    public class WildCardMod : BaseUnityPlugin
    {
        internal const string modGUID = "deB.WildCard";
        internal const string modName = "WILDCARD REPO";
        internal const string modVersion = "0.16.9";
        readonly Harmony harmony = new Harmony(modGUID);
        internal ManualLogSource log = null!;
        public WildCardUtils utils;
        public static WildCardMod instance;
        public bool moreUpgradesPresent = false;
        internal WildCardConfig ModConfig { get; private set; } = null!;
        public List<GameObject> valList = new List<GameObject>();
        public List<Item> itemList = new List<Item>();
        public List<Reskin> reskinList = new List<Reskin>();
        public List<GameObject> miscPrefabsList = new List<GameObject>();
        public static List<NetworkedEvent> networkedEvents = new List<NetworkedEvent>();
        [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "<Pending>")]
        void Awake()
        {
            if (instance == null)
            {
                instance = this;
            }
            log = Logger;
            utils = new WildCardUtils();
            Type[] assemblyTypes = Assembly.GetExecutingAssembly().GetTypes();
            for (int i = 0; i < assemblyTypes.Length; i++)
            {
                MethodInfo[] typeMethods = assemblyTypes[i].GetMethods(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
                for (int j = 0; j < typeMethods.Length; j++)
                {
                    object[] methodAttributes;
                    try
                    {
                        methodAttributes = typeMethods[j].GetCustomAttributes(typeof(RuntimeInitializeOnLoadMethodAttribute), false);
                    }
                    catch
                    {
                        continue;
                    }
                    if (methodAttributes.Length > 0)
                    {
                        typeMethods[j].Invoke(null, null);
                    }
                }
            }
            if (BepInEx.Bootstrap.Chainloader.PluginInfos.ContainsKey("bulletbot.moreupgrades"))
            {
                moreUpgradesPresent = true;
            }
            PropogateLists();
            HandleContent();
            DoPatches();
            log.LogInfo("WILDCARD REPO Successfully Loaded");
        }
        public void PropogateLists()
        {
            AssetBundle bundle = AssetBundle.LoadFromFile(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "wildcardmod"));
            string[] allAssetPaths = bundle.GetAllAssetNames();
            for (int i = 0; i < allAssetPaths.Length; i++)
            {
                string assetPath = allAssetPaths[i][..allAssetPaths[i].LastIndexOf("/")];
                switch (assetPath)
                {
                    case "assets/my creations/resources/valuables":
                        {
                            valList.Add(bundle.LoadAsset<GameObject>(allAssetPaths[i]));
                            break;
                        }
                    case "assets/my creations/resources/items":
                        {
                            itemList.Add(bundle.LoadAsset<Item>(allAssetPaths[i]));
                            break;
                        }
                    case "assets/my creations/resources/reskins":
                        {
                            reskinList.Add(bundle.LoadAsset<Reskin>(allAssetPaths[i]));
                            break;
                        }
                    case "assets/my creations/resources/misc":
                        {
                            UnityEngine.Object obj = bundle.LoadAsset(allAssetPaths[i]);
                            switch (obj)
                            {
                                case GameObject go:
                                    {
                                        miscPrefabsList.Add(go);
                                        break;
                                    }
                                default:
                                    {
                                        log.LogWarning($"\"{allAssetPaths[i]}\" was not a valid type, skipping.");
                                        break;
                                    }
                            }
                            break;
                        }
                    default:
                        {
                            log.LogWarning($"\"{assetPath}\" is not a known asset path, skipping.");
                            break;
                        }
                }
            }
        }
        public void HandleContent()
        {
            ModConfig = new WildCardConfig(base.Config, valList, itemList, reskinList);
            HandleValuables();
            HandleItems();
            HandleReskins();
            HandleMisc();
            HandleEvents();
        }
        
        public void HandleValuables()
        {
            for (int i = 0; i < valList.Count; i++)
            {
                if (ModConfig.isValEnabled[i].Value)
                {
                    bool register = true;
                    if (valList[i].TryGetComponent(out DummyValuable dummy))
                    {
                        switch (dummy.script)
                        {
                            case Item item:
                                {
                                    itemList.Add(item);
                                    break;
                                }
                            default:
                                {
                                    register = false;
                                    break;
                                }
                        }
                    }
                    if (register)
                    {
                        REPOLib.Modules.Valuables.RegisterValuable(valList[i]);
                        log.LogDebug($"{valList[i].name} valuable was loaded!");
                    }
                    else
                    {
                        log.LogInfo($"{valList[i].name} dummy did not have a valid setup");
                    }
                }
                else
                {
                    log.LogInfo($"{valList[i].name} valuable was disabled!");
                }
            }
        }
        public void HandleItems()
        {
            for (int i = 0; i < itemList.Count; i++)
            {
                if (i >= ModConfig.isItemEnabled.Count || ModConfig.isItemEnabled[i].Value)
                {
                    REPOLib.Modules.Items.RegisterItem(itemList[i]);
                    log.LogDebug($"{itemList[i].name} item was loaded!");
                }
                else
                {
                    log.LogInfo($"{itemList[i].name} item was disabled!");
                }
            }
        }
        public void HandleReskins()
        {
            for (int i = 0; i < reskinList.Count; i++)
            {
                if (!ModConfig.isReskinEnabled[i].Value || ModConfig.reskinChance[i].Value <= 0f)
                {
                    log.LogInfo($"{reskinList[i].identifier} reskin was disabled!");
                }
            }
        }
        public void HandleMisc()
        {
            for (int i = 0; i < miscPrefabsList.Count; i++)
            {
                NetworkPrefabs.RegisterNetworkPrefab($"Misc/{miscPrefabsList[i].name}", miscPrefabsList[i]);
                Utilities.FixAudioMixerGroups(miscPrefabsList[i]);
                log.LogDebug($"{miscPrefabsList[i].name} prefab was loaded!");
            }
        }
        public void HandleEvents()
        {
            networkedEvents.Add(new NetworkedEvent("Set Enemy Skin", WildCardUtils.SetEnemySkin));
        }
        public void DoPatches()
        {
            log.LogDebug("Patching Game");
            harmony.PatchAll(typeof(CameraGlitchPatches));
            harmony.PatchAll(typeof(EnemyParentPatches));
            harmony.PatchAll(typeof(HurtColliderPatches));
            harmony.PatchAll(typeof(PhysGrabObjectPatches));
            harmony.PatchAll(typeof(PlayerAvatarPatches));
            harmony.PatchAll(typeof(StatsManagerPatches));
            harmony.PatchAll(typeof(WorldSpaceUIValueLostPatches));
            harmony.PatchAll(typeof(EnemyPatches));
            if (ModConfig.harmonyPatches.Value)
            {
                harmony.PatchAll(typeof(PhysGrabberRayCheckPatch));
            }
            else
            {
                log.LogInfo("Disabled Giwi's Harmony Patches");
            }
        }
    }
}