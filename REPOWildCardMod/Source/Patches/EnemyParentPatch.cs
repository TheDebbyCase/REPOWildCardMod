using ExitGames.Client.Photon;
using HarmonyLib;
using Photon.Pun;
using REPOWildCardMod.Config;
using REPOWildCardMod.Items;
using REPOWildCardMod.Utils;
using System.Reflection;
using System.Collections.Generic;
using UnityEngine;
using MonoMod.Utils;
using REPOWildCardMod.Extensions;
namespace REPOWildCardMod.Patches
{
    [HarmonyPatch(typeof(EnemyParent))]
    public static class EnemyParentPatches
    {
        public static FieldInfo setupPatchInstance;
        static readonly BepInEx.Logging.ManualLogSource log = WildCardMod.instance.log;
        [HarmonyPatch(nameof(EnemyParent.SpawnRPC))]
        [HarmonyPostfix]
        public static void AddEnemyToList(EnemyParent __instance)
        {
            SmithNote[] smithNotes = Object.FindObjectsOfType<SmithNote>();
            if (smithNotes != null && smithNotes.Length > 0)
            {
                for (int i = 0; i < smithNotes.Length; i++)
                {
                    smithNotes[i].StartCoroutine(smithNotes[i].FirstSpawnCoroutine(__instance));
                }
            }
        }
        [HarmonyPatch(nameof(EnemyParent.DespawnRPC))]
        [HarmonyPostfix]
        public static void RemoveEnemyFromList(EnemyParent __instance)
        {
            SmithNote[] smithNotes = Object.FindObjectsOfType<SmithNote>();
            if (smithNotes != null && smithNotes.Length > 0)
            {
                for (int i = 0; i < smithNotes.Length; i++)
                {
                    smithNotes[i].RemoveEnemy(__instance);
                }
            }
        }
        [HarmonyPatch(nameof(EnemyParent.Awake))]
        [HarmonyPrefix]
        public static void GetInstance(EnemyParent __instance)
        {
            setupPatchInstance = typeof(EnemyParent).GetMethod("Setup", BindingFlags.Instance | BindingFlags.NonPublic).GetStateMachineTarget().DeclaringType!.GetField("<>4__this", BindingFlags.Instance | BindingFlags.Public)!;
        }
        [HarmonyPatch(nameof(EnemyParent.Setup), MethodType.Enumerator)]
        [HarmonyPostfix]
        public static void WildCardEnemySetup(object __instance, bool __result)
        {
            if (__result)
            {
                return;
            }
            EnemyParent enemyParent = setupPatchInstance.GetValue(__instance) as EnemyParent;
            if (SemiFunc.IsMasterClientOrSingleplayer())
            {
                WildCardConfig config = WildCardMod.instance.ModConfig;
                List<Reskin> reskins = WildCardMod.instance.reskinList;
                int skinIndex = -1;
                int variantIndex = 0;
                List<Reskin> potentialSkins = new List<Reskin>();
                Reskin newSkin = null;
                for (int i = 0; i < reskins.Count; i++)
                {
                    if (reskins[i].identifier == enemyParent.enemyName && config.isReskinEnabled[i].Value && config.reskinChance[i].Value > 0f)
                    {
                        potentialSkins.Add(reskins[i]);
                    }
                }
                if (potentialSkins.Count > 0)
                {
                    newSkin = potentialSkins[Random.Range(0, potentialSkins.Count)];
                    skinIndex = reskins.IndexOf(newSkin);
                }
                if (newSkin != null)
                {
                    log.LogDebug($"Reskin for {enemyParent.enemyName} found!");
                    if (Random.value <= config.reskinChance[skinIndex].Value)
                    {
                        log.LogDebug($"New skin for {newSkin.identifier} is being applied!");
                        if (newSkin.variantChances.Length > 1)
                        {
                            AnimationCurve curve = newSkin.variantsCurve;
                            float cumulative = 0f;
                            for (int i = 0; i < config.reskinVariantChance[skinIndex].Count; i++)
                            {
                                curve.keys[i + 1].time = cumulative;
                                cumulative += config.reskinVariantChance[skinIndex][i].Value;
                            }
                            variantIndex = Mathf.FloorToInt(curve.Evaluate(Random.value));
                            if (variantIndex == newSkin.variantChances.Length)
                            {
                                variantIndex--;
                            }
                            log.LogDebug($"{enemyParent.enemyName} reskin selected variant {variantIndex + 1}");
                        }
                        WildCardMod.networkedEvents.Find((x) => x.Name == "Set Enemy Skin").RaiseEvent(new object[] { SemiFunc.EnemyGetIndex(enemyParent.Enemy), skinIndex, variantIndex }, REPOLib.Modules.NetworkingEvents.RaiseAll, SendOptions.SendReliable);
                    }
                }
                if (StatsManager.instance.itemsPurchased.ContainsKey("Item Worm Jar") && StatsManager.instance.itemsPurchased["Item Worm Jar"] > 0 && !enemyParent.WormData().hasWorm)
                {
                    log.LogDebug($"Adding dormant worm to: \"{enemyParent.enemyName}\"");
                    GameObject newWorm;
                    if (SemiFunc.IsMultiplayer())
                    {
                        newWorm = PhotonNetwork.InstantiateRoomObject($"Misc/Worm Attach", new Vector3(0f, -100f, 0f), Quaternion.identity);
                    }
                    else
                    {
                        newWorm = Object.Instantiate(WildCardMod.instance.miscPrefabsList.Find((x) => x.name == "Worm Attach"), new Vector3(0f, -100f, 0f), Quaternion.identity);
                    }
                    enemyParent.InitializeWormData(newWorm.GetComponent<WormAttach>());
                    enemyParent.WormData().worm.Initialize(SemiFunc.EnemyGetIndex(enemyParent.Enemy));
                }
            }
        }
    }
}