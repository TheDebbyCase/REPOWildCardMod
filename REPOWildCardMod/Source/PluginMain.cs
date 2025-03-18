using UnityEngine;
using BepInEx;
using BepInEx.Logging;
using System;
using System.Reflection;
using System.IO;
using System.Collections.Generic;
using REPOWildCardMod.Config;
using HarmonyLib;
namespace REPOWildCardMod
{
    [BepInPlugin(modGUID, modName, modVersion)]
    [BepInDependency(REPOLib.MyPluginInfo.PLUGIN_GUID, BepInDependency.DependencyFlags.HardDependency)]
    public class WildCardMod : BaseUnityPlugin
    {
        internal const string modGUID = "deB.WildCard";
        internal const string modName = "WILDCARD REPO";
        internal const string modVersion = "0.4.0";
        private readonly Harmony harmony = new Harmony(modGUID);
        internal static ManualLogSource log = null!;
        public static WildCardMod Instance;
        internal static WildCardConfig ModConfig {get; private set;} = null!;
        public static List<GameObject> valList = new List<GameObject>();
        [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "<Pending>")]

        private void Awake()
        {
            log = Logger;
            if (Instance == null)
            {
                Instance = this;
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
                    case "assets/my creations/valuables":
                        {
                            valList.Add(bundle.LoadAsset<GameObject>(allAssetPaths[i]));
                            break;
                        }
                    default:
                        {
                            log.LogWarning($"\"{assetPath}\" is not a known asset path, skipping.");
                            break;
                        }
                }
            }
            ModConfig = new WildCardConfig(base.Config, valList);
            for (int i = 0; i < valList.Count; i++)
            {
                if (ModConfig.isValEnabled[i].Value)
                {
                    REPOLib.Modules.Valuables.RegisterValuable(valList[i]);
                    log.LogDebug($"{valList[i].name} valuable was loaded!");
                }
                else
                {
                    log.LogInfo($"{valList[i].name} valuable was disabled!");
                }
            }
            harmony.PatchAll();
            log.LogInfo("WILDCARD REPO Successfully Loaded");
        }
    }
}