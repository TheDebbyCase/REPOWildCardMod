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
namespace REPOWildCardMod
{
    [BepInPlugin(modGUID, modName, modVersion)]
    [BepInDependency(REPOLib.MyPluginInfo.PLUGIN_GUID, BepInDependency.DependencyFlags.HardDependency)]
    [BepInDependency("bulletbot.moreupgrades", BepInDependency.DependencyFlags.SoftDependency)]
    public class WildCardMod : BaseUnityPlugin
    {
        internal const string modGUID = "deB.WildCard";
        internal const string modName = "WILDCARD REPO";
        internal const string modVersion = "0.11.6";
        private readonly Harmony harmony = new Harmony(modGUID);
        internal ManualLogSource log = null!;
        public WildCardUtils utils;
        public static WildCardMod instance;
        public bool moreUpgradesPresent = false;
        internal WildCardConfig ModConfig { get; private set; } = null!;
        public List<GameObject> valList = new List<GameObject>();
        public List<Item> itemList = new List<Item>();
        public List<Reskin> reskinList = new List<Reskin>();
        [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "<Pending>")]
        private void Awake()
        {
            log = Logger;
            utils = new WildCardUtils();
            if (instance == null)
            {
                instance = this;
            }
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
                    default:
                        {
                            log.LogWarning($"\"{assetPath}\" is not a known asset path, skipping.");
                            break;
                        }
                }
            }
            ModConfig = new WildCardConfig(base.Config, valList, itemList, reskinList);
            for (int i = 0; i < valList.Count; i++)
            {
                if (ModConfig.isValEnabled[i].Value)
                {
                    bool register = true;
                    if (valList[i].TryGetComponent(out DummyValuable dummy))
                    {
                        switch (dummy.script.GetType().ToString())
                        {
                            case "Item":
                                {
                                    itemList.Add((Item)dummy.script);
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
            for (int i = 0; i < reskinList.Count; i++)
            {
                if (!ModConfig.isReskinEnabled[i].Value || ModConfig.reskinChance[i].Value <= 0f)
                {
                    log.LogInfo($"{reskinList[i].identifier} reskin was disabled!");
                }
            }
            log.LogDebug("Patching Game");
            harmony.PatchAll(typeof(CameraGlitchPatches));
            harmony.PatchAll(typeof(EnemyParentPatches));
            harmony.PatchAll(typeof(HurtColliderPatch));
            harmony.PatchAll(typeof(PhysGrabObjectImpactDetectorPatches));
            harmony.PatchAll(typeof(PhysGrabObjectPatches));
            harmony.PatchAll(typeof(PlayerAvatarPatch));
            harmony.PatchAll(typeof(StatsManagerPatches));
            if (ModConfig.harmonyPatches.Value)
            {
                harmony.PatchAll(typeof(PhysGrabberRayCheckPatch));
                harmony.PatchAll(typeof(PhysGrabberPhysGrabPointActivatePatch));
            }
            else
            {
                log.LogInfo("Disabled Giwi's Transpilers");
            }
            log.LogInfo("WILDCARD REPO Successfully Loaded");
        }
    }
}