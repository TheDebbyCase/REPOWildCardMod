using HarmonyLib;
using REPOWildCardMod.Config;
using REPOWildCardMod.Items;
using REPOWildCardMod.Utils;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
namespace REPOWildCardMod.Patches
{
    [HarmonyPatch(typeof(EnemyParent))]
    public static class EnemyParentPatches
    {
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
        public static bool ReskinEnemy(EnemyParent __instance)
        {
            WildCardConfig config = WildCardMod.instance.ModConfig;
            List<Reskin> reskins = WildCardMod.instance.reskinList;
            int skinIndex = -1;
            Reskin newSkin = null;
            for (int i = 0; i < reskins.Count; i++)
            {
                if (reskins[i].identifier == __instance.enemyName && config.isReskinEnabled[i].Value && config.reskinChance[i].Value > 0f)
                {
                    newSkin = reskins[i];
                    skinIndex = i;
                    break;
                }
            }
            if (newSkin != null)
            {
                log.LogDebug($"Reskin for {__instance.enemyName} found!");
                if (Random.value <= config.reskinChance[skinIndex].Value)
                {
                    log.LogDebug($"New skin for {newSkin.identifier} is being applied!");
                    int variantIndex = 0;
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
                        log.LogDebug($"{__instance.enemyName} reskin selected variant {variantIndex + 1}");
                    }
                    bool success = true;
                    switch (__instance.enemyName)
                    {
                        case "Rugrat":
                            {
                                Transform meshesParent = __instance.EnableObject.transform.Find("Visuals").Find("Mesh").Find("________________________________").Find("ANIM BOT").Find("________________________________").Find("ANIM MID").Find("________________________________");
                                MeshFilter[] filters = meshesParent.GetComponentsInChildren<MeshFilter>(true);
                                List<Transform> transforms = new List<Transform>();
                                for (int i = 0; i < filters.Length; i++)
                                {
                                    transforms.Add(filters[i].transform);
                                }
                                for (int i = 0; i < newSkin.replacers[variantIndex].bodyParts.Count; i++)
                                {
                                    List<Transform> replacedTransforms = new List<Transform>();
                                    for (int j = 0; j < transforms.Count; j++)
                                    {
                                        if (replacedTransforms.Contains(transforms[j]))
                                        {
                                            continue;
                                        }
                                        if (transforms[j].name == newSkin.replacers[variantIndex].bodyParts[i].transformName)
                                        {
                                            log.LogDebug($"{__instance.enemyName}: {transforms[j].name} successfully replaced!");
                                            transforms[j].GetComponent<MeshFilter>().mesh = newSkin.replacers[variantIndex].bodyParts[i].newMesh;
                                            transforms[j].GetComponent<MeshRenderer>().materials = newSkin.replacers[variantIndex].bodyParts[i].newMaterials.ToArray();
                                            replacedTransforms.Add(transforms[j]);
                                            break;
                                        }
                                    }
                                }
                                break;
                            }
                        default:
                            {
                                success = false;
                                break;
                            }
                    }
                    if (success)
                    {
                        log.LogInfo($"Successfully reskinned {__instance.enemyName}!");
                    }
                    else
                    {
                        log.LogWarning($"Attempted to reskin {__instance.enemyName} but something went wrong!");

                    }
                }
            }
            return true;
        }
    }
}