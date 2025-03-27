using HarmonyLib;
using REPOWildCardMod.Items;
using UnityEngine;
namespace REPOWildCardMod.Patches
{
    [HarmonyPatch(typeof(EnemyParent))]
    public static class EnemyParentPatch
    {
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
    }
}