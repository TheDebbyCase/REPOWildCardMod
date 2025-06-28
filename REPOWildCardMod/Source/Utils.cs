using UnityEditor;
using System;
using System.Reflection;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using UnityEngine;
using System.Linq;
using ExitGames.Client.Photon;
using Photon.Pun;
namespace REPOWildCardMod.Utils
{
    public class WildCardUtils
    {
        readonly BepInEx.Logging.ManualLogSource log = WildCardMod.instance.log;
        public static void SetEnemyAudio(EventData eventData)
        {
            object[] data = (object[])eventData.CustomData;
            EnemyAudioLogic(PhotonView.Find((int)data[0]).GetComponent<Enemy>().EnemyParent, WildCardMod.instance.audioReplacerList[(int)data[1]], (int)data[2]);
        }
        public static void EnemyAudioLogic(EnemyParent enemyParent, AudioReplacer newAudio, int variantIndex)
        {
            Component animComponent = enemyParent.GetComponentInChildren(newAudio.animClass, true);
            List<FieldInfo> fieldInfos = newAudio.animClass.GetFields().ToList();
            List<Sound> toReplace = new List<Sound>();
            Dictionary<Sound, FieldInfo> soundInfoDict = new Dictionary<Sound, FieldInfo>();
            for (int i = 0; i < fieldInfos.Count; i++)
            {
                if (fieldInfos[i].FieldType == typeof(Sound))
                {
                    soundInfoDict.Add(fieldInfos[i].GetValue(animComponent) as Sound, fieldInfos[i]);
                }
            }
            toReplace = soundInfoDict.Keys.ToList();
            for (int i = 0; i < toReplace.Count; i++)
            {
                NewSounds newSound = newAudio.replacers[variantIndex].sounds.Find((x) => x.fieldName == soundInfoDict[toReplace[i]].Name);
                if (newSound == null || (newSound != null && newSound.newClips.Length == 0))
                {
                    continue;
                }
                List<AudioClip> newClips = newSound.newClips.ToList();
                if (newSound.addon)
                {
                    newClips.AddRange(toReplace[i].Sounds);
                }
                toReplace[i].Sounds = newClips.ToArray();
                toReplace[i].Volume = newSound.volumeOverride;
                WildCardMod.instance.log.LogDebug($"{enemyParent.enemyName}: \"{newSound.fieldName}\" successfully replaced!");
            }
        }
        public static void SetEnemySkin(EventData eventData)
        {
            object[] data = (object[])eventData.CustomData;
            EnemySkinLogic(PhotonView.Find((int)data[0]).GetComponent<Enemy>().EnemyParent, WildCardMod.instance.reskinList[(int)data[1]], (int)data[2]);
        }
        public static void EnemySkinLogic(EnemyParent enemyParent, Reskin newSkin, int variantIndex)
        {
            string enemyName = enemyParent.enemyName;
            MeshFilter[] filters = enemyParent.transform.GetComponentsInChildren<MeshFilter>(true);
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
                        transforms[j].GetComponent<MeshFilter>().mesh = newSkin.replacers[variantIndex].bodyParts[i].newMesh;
                        transforms[j].GetComponent<MeshRenderer>().materials = newSkin.replacers[variantIndex].bodyParts[i].newMaterials.ToArray();
                        replacedTransforms.Add(transforms[j]);
                        WildCardMod.instance.log.LogDebug($"{enemyName}: \"{transforms[j].name}\" successfully replaced!");
                        break;
                    }
                }
            }
        }
        public Transform FindEnemyTransform(EnemyParent targetEnemy, string type)
        {
            Transform finalTransform = null;
            if (targetEnemy == null)
            {
                log.LogWarning($"Enemy \"{targetEnemy.enemyName}\" is not spawned!");
                return null;
            }
            List<Transform> transforms = targetEnemy.GetComponentsInChildren<Transform>(true).ToList();
            bool valid = true;
            switch (targetEnemy.enemyName)
            {
                case "Animal":
                    {
                        switch (type)
                        {
                            case "Head":
                                {
                                    finalTransform = transforms.Find((x) => x.name == "Eyes");
                                    break;
                                }
                            default:
                                {
                                    valid = false;
                                    break;
                                }
                        }
                        break;
                    }
                case "Apex Predator":
                    {
                        switch (type)
                        {
                            case "Head":
                                {
                                    finalTransform = transforms.Find((x) => x.name == "ANIM Head");
                                    break;
                                }
                            default:
                                {
                                    valid = false;
                                    break;
                                }
                        }
                        break;
                    }
                case "Banger":
                    {
                        switch (type)
                        {
                            case "Head":
                                {
                                    finalTransform = transforms.Find((x) => x.name == "[ANIM BODY TOP]");
                                    break;
                                }
                            default:
                                {
                                    valid = false;
                                    break;
                                }
                        }
                        break;
                    }
                case "Bowtie":
                    {
                        switch (type)
                        {
                            case "Head":
                                {
                                    finalTransform = transforms.Find((x) => x.name == "ANIM EYES");
                                    break;
                                }
                            default:
                                {
                                    valid = false;
                                    break;
                                }
                        }
                        break;
                    }
                case "Chef":
                    {
                        switch (type)
                        {
                            case "Head":
                                {
                                    finalTransform = transforms.Find((x) => x.name == "hat");
                                    break;
                                }
                            default:
                                {
                                    valid = false;
                                    break;
                                }
                        }
                        break;
                    }
                case "Clown":
                    {
                        switch (type)
                        {
                            case "Head":
                                {
                                    finalTransform = transforms.Find((x) => x.name == "[ANIM HAT]");
                                    break;
                                }
                            default:
                                {
                                    valid = false;
                                    break;
                                }
                        }
                        break;
                    }
                case "Gnome":
                    {
                        switch (type)
                        {
                            case "Head":
                                {
                                    finalTransform = transforms.Find((x) => x.name == "ANIM HEAD");
                                    break;
                                }
                            default:
                                {
                                    valid = false;
                                    break;
                                }
                        }
                        break;
                    }
                case "Headman":
                    {
                        switch (type)
                        {
                            case "Head":
                                {
                                    finalTransform = transforms.Find((x) => x.name == "Top Mesh");
                                    break;
                                }
                            default:
                                {
                                    valid = false;
                                    break;
                                }
                        }
                        break;
                    }
                case "Hidden":
                    {
                        switch (type)
                        {
                            case "Head":
                                {
                                    finalTransform = transforms.Find((x) => x.name == "Breath Source");
                                    break;
                                }
                            default:
                                {
                                    valid = false;
                                    break;
                                }
                        }
                        break;
                    }
                case "Huntsman":
                    {
                        switch (type)
                        {
                            case "Head":
                                {
                                    finalTransform = transforms.Find((x) => x.name == "ANIM HEAD");
                                    break;
                                }
                            default:
                                {
                                    valid = false;
                                    break;
                                }
                        }
                        break;
                    }
                case "Mentalist":
                    {
                        switch (type)
                        {
                            case "Head":
                                {
                                    finalTransform = transforms.Find((x) => x.name == "ANIM head_raw");
                                    break;
                                }
                            default:
                                {
                                    valid = false;
                                    break;
                                }
                        }
                        break;
                    }
                case "Peeper":
                    {
                        switch (type)
                        {
                            case "Head":
                                {
                                    finalTransform = transforms.Find((x) => x.name == "Pupil");
                                    break;
                                }
                            default:
                                {
                                    valid = false;
                                    break;
                                }
                        }
                        break;
                    }
                case "Reaper":
                    {
                        switch (type)
                        {
                            case "Head":
                                {
                                    finalTransform = transforms.Find((x) => x.name == "ANIM head");
                                    break;
                                }
                            default:
                                {
                                    valid = false;
                                    break;
                                }
                        }
                        break;
                    }
                case "Robe":
                    {
                        switch (type)
                        {
                            case "Head":
                                {
                                    finalTransform = transforms.Find((x) => x.name == "Head");
                                    break;
                                }
                            default:
                                {
                                    valid = false;
                                    break;
                                }
                        }
                        break;
                    }
                case "Rugrat":
                    {
                        switch (type)
                        {
                            case "Head":
                                {
                                    finalTransform = transforms.Find((x) => x.name == "ANIM HEAD");
                                    break;
                                }
                            default:
                                {
                                    valid = false;
                                    break;
                                }
                        }
                        break;
                    }
                case "Shadow Child":
                    {
                        switch (type)
                        {
                            case "Head":
                                {
                                    finalTransform = transforms.Find((x) => x.name == "Thin man head");
                                    break;
                                }
                            default:
                                {
                                    valid = false;
                                    break;
                                }
                        }
                        break;
                    }
                case "Spewer":
                    {
                        switch (type)
                        {
                            case "Head":
                                {
                                    finalTransform = transforms.Find((x) => x.name == "enemy slow mouth flying top");
                                    break;
                                }
                            default:
                                {
                                    valid = false;
                                    break;
                                }
                        }
                        break;
                    }
                case "Trudge":
                    {
                        switch (type)
                        {
                            case "Head":
                                {
                                    finalTransform = transforms.Find((x) => x.name == "ANIM head_top");
                                    break;
                                }
                            default:
                                {
                                    valid = false;
                                    break;
                                }
                        }
                        break;
                    }
                case "Upscream":
                    {
                        switch (type)
                        {
                            case "Head":
                                {
                                    finalTransform = transforms.Find((x) => x.name == "head");
                                    break;
                                }
                            default:
                                {
                                    valid = false;
                                    break;
                                }
                        }
                        break;
                    }
                default:
                    {
                        finalTransform = transforms[0];
                        log.LogWarning($"Enemy \"{targetEnemy.enemyName}\" has not been set up");
                        break;
                    }
            }
            if (!valid || finalTransform == null)
            {
                log.LogWarning($"Transform of type \"{type}\" for enemy \"{targetEnemy.enemyName}\" has not been set up");
            }
            return finalTransform;
        }
        public bool TextIsSimilar(string first, string second)
        {
            string firstReplaced = first.Replace(" ", string.Empty);
            string secondReplaced = second.Replace(" ", string.Empty);
            if (first == second)
            {
                return true;
            }
            else if (first == second.ToLower() || first == second.ToUpper())
            {
                return true;
            }
            else if (first == second.TrimStart() || first == second.TrimEnd())
            {
                return true;
            }
            else if (first == second[1..] || first == second[..(second.Length - 2)])
            {
                return true;
            }
            else if (firstReplaced == secondReplaced)
            {
                return true;
            }
            else if (firstReplaced == secondReplaced.ToLower() || firstReplaced == secondReplaced.ToUpper())
            {
                return true;
            }
            else if (firstReplaced == secondReplaced.TrimStart() || firstReplaced == secondReplaced.TrimEnd())
            {
                return true;
            }
            else if (firstReplaced == secondReplaced[1..] || firstReplaced == secondReplaced[..(secondReplaced.Length - 2)])
            {
                return true;
            }
            return false;
        }
        public string CleanText(string text)
        {
            string newText = Regex.Replace(text, @"[^\u0000-\u007F]+", string.Empty);
            if (newText == string.Empty)
            {
                newText = $"this is nicer {text.Length}";
            }
            return newText;
        }
        public bool IntToBool(int binary)
        {
            if (binary > 1)
            {
                binary = 1;
            }
            else if (binary < 0)
            {
                binary = 0;
            }
            if (binary == 1)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        public int BoolToInt(bool binary)
        {
            if (binary)
            {
                return 1;
            }
            else
            {
                return 0;
            }
        }
    }
    [Serializable]
    public class Chance
    {
        [Range(0f, 1f)]
        [Delayed] public float value;
    }
    [Serializable]
    public class Replacer
    {
        public List<BodyParts> bodyParts = new List<BodyParts>();
    }
    [Serializable]
    public class BodyParts
    {
        public string transformName = "";
        public Mesh newMesh;
        public List<Material> newMaterials = new List<Material>();
    }
    [CreateAssetMenu(menuName = "WCScriptableObjects/Reskin", order = 1)]
    public class Reskin : ScriptableObject
    {
        public string identifier = "";
        [Space]
        public Chance replaceChance = new Chance { value = 1f };
        public Chance[] variantChances = new Chance[0];
        public AnimationCurve variantsCurve;
        [Space]
        public List<Replacer> replacers = new List<Replacer>();
        [MethodImpl(MethodImplOptions.NoInlining)]
        public void OnValidate()
        {
            for (int i = 0; i < variantChances.Length; i++)
            {
                if (replacers.Count < i + 1)
                {
                    replacers.Add(new Replacer());
                }
                else if (replacers.Count > variantChances.Length)
                {
                    replacers.RemoveAt(replacers.Count - 1);
                }
            }
            float variantChancesTotal = 0f;
            for (int i = 0; i < variantChances.Length; i++)
            {
                variantChancesTotal += variantChances[i].value;
            }
            if (variantChancesTotal > 1f)
            {
                for (int i = 0; i < variantChances.Length; i++)
                {
                    variantChances[i].value -= (float)Math.Round((decimal)((variantChancesTotal - 1f) / variantChances.Length), 2);
                    if (variantChances[i].value < 0.001f)
                    {
                        variantChances[i].value = 0f;
                    }
                }
            }
            else if (variantChancesTotal < 1f)
            {
                for (int i = 0; i < variantChances.Length; i++)
                {

                    variantChances[i].value += (float)Math.Round((decimal)((1f - variantChancesTotal) / variantChances.Length), 2);
                    if (variantChances[i].value > 0.999)
                    {
                        variantChances[i].value = 1f;
                    }
                }
            }
            variantsCurve = new AnimationCurve();
            variantsCurve.AddKey(0f, 0f);
            float cumulative = 0f;
            for (int i = 0; i < variantChances.Length; i++)
            {
                variantsCurve.AddKey(cumulative + variantChances[i].value, (float)i + 1f);
                cumulative += variantChances[i].value;
            }
            variantsCurve.keys[^1].value -= 1f;
            for (int i = 0; i < variantsCurve.length; i++)
            {
                AnimationUtility.SetKeyLeftTangentMode(variantsCurve, i, AnimationUtility.TangentMode.Linear);
                AnimationUtility.SetKeyRightTangentMode(variantsCurve, i, AnimationUtility.TangentMode.Linear);
            }
        }
    }
    [Serializable]
    public class ReplacerAudio
    {
        public List<NewSounds> sounds = new List<NewSounds>();
    }
    [Serializable]
    public class NewSounds
    {
        public string fieldName = "";
        public bool addon = false;
        public float volumeOverride = 0.5f;
        public AudioClip[] newClips;
    }
    [CreateAssetMenu(menuName = "WCScriptableObjects/AudioReplacer", order = 1)]
    public class AudioReplacer : ScriptableObject
    {
        public string identifier = "";
        public string enemyName = "";
        public Type animClass;
        [Space]
        public Chance replaceChance = new Chance { value = 1f };
        public Chance[] variantChances = new Chance[0];
        public AnimationCurve variantsCurve;
        [Space]
        public List<ReplacerAudio> replacers = new List<ReplacerAudio>();
        [MethodImpl(MethodImplOptions.NoInlining)]
        public void OnValidate()
        {
            animClass = Type.GetType(identifier);
            for (int i = 0; i < variantChances.Length; i++)
            {
                if (replacers.Count < i + 1)
                {
                    replacers.Add(new ReplacerAudio());
                }
                else if (replacers.Count > variantChances.Length)
                {
                    replacers.RemoveAt(replacers.Count - 1);
                }
            }
            float variantChancesTotal = 0f;
            for (int i = 0; i < variantChances.Length; i++)
            {
                variantChancesTotal += variantChances[i].value;
            }
            if (variantChancesTotal > 1f)
            {
                for (int i = 0; i < variantChances.Length; i++)
                {
                    variantChances[i].value -= (float)Math.Round((decimal)((variantChancesTotal - 1f) / variantChances.Length), 2);
                    if (variantChances[i].value < 0.001f)
                    {
                        variantChances[i].value = 0f;
                    }
                }
            }
            else if (variantChancesTotal < 1f)
            {
                for (int i = 0; i < variantChances.Length; i++)
                {

                    variantChances[i].value += (float)Math.Round((decimal)((1f - variantChancesTotal) / variantChances.Length), 2);
                    if (variantChances[i].value > 0.999)
                    {
                        variantChances[i].value = 1f;
                    }
                }
            }
            variantsCurve = new AnimationCurve();
            variantsCurve.AddKey(0f, 0f);
            float cumulative = 0f;
            for (int i = 0; i < variantChances.Length; i++)
            {
                variantsCurve.AddKey(cumulative + variantChances[i].value, (float)i + 1f);
                cumulative += variantChances[i].value;
            }
            variantsCurve.keys[^1].value -= 1f;
            for (int i = 0; i < variantsCurve.length; i++)
            {
                AnimationUtility.SetKeyLeftTangentMode(variantsCurve, i, AnimationUtility.TangentMode.Linear);
                AnimationUtility.SetKeyRightTangentMode(variantsCurve, i, AnimationUtility.TangentMode.Linear);
            }
        }
    }
}