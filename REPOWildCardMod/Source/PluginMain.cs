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
namespace REPOWildCardMod
{
    [BepInPlugin(modGUID, modName, modVersion)]
    [BepInDependency(REPOLib.MyPluginInfo.PLUGIN_GUID, BepInDependency.DependencyFlags.HardDependency)]
    public class WildCardMod : BaseUnityPlugin
    {
        internal const string modGUID = "deB.WildCard";
        internal const string modName = "WILDCARD REPO";
        internal const string modVersion = "0.9.0";
        private readonly Harmony harmony = new Harmony(modGUID);
        internal ManualLogSource log = null!;
        public WildCardUtils utils;
        public static WildCardMod instance;
        internal WildCardConfig ModConfig { get; private set; } = null!;
        public List<GameObject> valList = new List<GameObject>();
        public List<Item> itemList = new List<Item>();
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
                    object[] methodAttributes = typeMethods[j].GetCustomAttributes(typeof(RuntimeInitializeOnLoadMethodAttribute), false);
                    if (methodAttributes.Length > 0)
                    {
                        typeMethods[j].Invoke(null, null);
                    }
                }
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
                    default:
                        {
                            log.LogWarning($"\"{assetPath}\" is not a known asset path, skipping.");
                            break;
                        }
                }
            }
            ModConfig = new WildCardConfig(base.Config, valList, itemList);
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
            if (ModConfig.harmonyPatches.Value)
            {
                log.LogDebug("Patching Game");
                harmony.PatchAll();
            }
            else
            {
                log.LogInfo("Disabled Harmony Patches");
            }
            log.LogInfo("WILDCARD REPO Successfully Loaded");
        }
    }
}