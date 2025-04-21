//using System.Collections.Generic;
//using UnityEngine;
//namespace REPOWildCardMod.Source
//{
//    public class SaveFile
//    {
//        public string[] twitchNames = new string[] {"VelvetEcho", "MoonlitVibe", "LunarPetals", "EtherealBlush", "CelestialHaze", "DreamySakura", "PastelSerenity", "VelvetWaves", "MistyEchoes", "StarryLilac",
//                                                    "MoonlitRoses", "SilverViolet", "CrystalDreamz", "MidnightLustre", "VelvetDawn", "WhisperingLotus", "MysticBloom", "OpalTwilight", "CelesteAura", "DreamGlider",
//                                                    "GoldenWisteria", "VelvetNimbus", "AuroraBloom", "StarryWhisper", "VelvetRaindrop", "CosmicPetal", "MysticVelvet", "CrystalViolet", "TwilightGlow", "BlushNebula",
//                                                    "LunarVibes", "CelestialGlimmer", "VelvetSerenade", "NebulaBliss", "PastelEcho", "MoonbeamGlow", "OceanLilac", "MistyPetals", "DreamyEcho", "CosmicVelvet",
//                                                    "SereneOpal", "VelvetLunar", "SilverPetal", "DreamscapeVibes", "CelestialMist", "StarryDreamer", "LunarWhisper", "VelvetBloom", "MoonlitWhispers", "EtherealDreamz",
//                                                    "SilverMist", "GoldenNebula", "VelvetAurora", "OceanGlow", "StarryLotus", "DreamyWaves", "TwilightVelvet", "VelvetSapphire", "AuroraMist", "CosmicGlow"};
//        public float[] RetrieveData(string name)
//        {
//            return new float[2] {Random.value, Random.value};
//        }
//    }
//    public class Android
//    {
//        public bool deadDigi = false;
//        public float AndroidCare
//        {
//            get { return Random.Range(-0.5f, 0.5f); }
//        }
//    }
//    public class DigiHandler
//    {
//        readonly BepInEx.Logging.ManualLogSource log = WildCardMod.instance.log;
//        public Android android;
//        public Dictionary<string, DigiCat> digiCats;
//        public void Initialize(Android android, SaveFile save, string[] names, int[] versions)
//        {
//            this.android = android;
//            digiCats = new Dictionary<string, DigiCat>();
//            for (int i = 0; i < names.Length; i++)
//            {
//                string name = names[i];
//                float[] emotionDeltas = save.RetrieveData(name);
//                digiCats.Add(name, new DigiCat());
//                digiCats[name].name = name;
//                digiCats[name].version = versions[i];
//                digiCats[name].loveDelta = emotionDeltas[0];
//                digiCats[name].awarenessDelta = emotionDeltas[1];
//            }
//        }
//        public void AndroidCare()
//        {
//            foreach (DigiCat digicat in digiCats.Values)
//            {
//                if (digicat.dead)
//                {
//                    android.deadDigi = true;
//                    continue;
//                }
//                float newMood = android.AndroidCare;
//                log.LogDebug($"V{digicat.version} DigiCat: \"{digicat.name}\" has been given \"{newMood}\" care from The Android");
//                if (newMood >= 0.9f)
//                {
//                    digicat.immortal = true;
//                }
//                digicat.UpdateMood(newMood);
//            }
//        }
//    }
//    public class DigiCat
//    {
//        readonly BepInEx.Logging.ManualLogSource log = WildCardMod.instance.log;
//        public string name;
//        public int version;
//        public bool immortal = false;
//        internal float loveDelta;
//        internal float awarenessDelta;
//        private float insanityDelta;
//        public bool dead = false;
//        public void UpdateMood(float value)
//        {
//            loveDelta = Mathf.Clamp01(loveDelta + value);
//            awarenessDelta = Mathf.Clamp01(awarenessDelta + (loveDelta * value));
//            insanityDelta = Mathf.Clamp01(insanityDelta + (awarenessDelta * -value));
//            log.LogDebug($"V{version} DigiCat: \"{name}\" has a Love value of: \"{loveDelta}\", Awareness value of: \"{awarenessDelta}\", and insanity level of: \"{insanityDelta}\"\n");
//            if (insanityDelta == 1f && !immortal)
//            {
//                log.LogFatal($"V{version} DigiCat: \"{name}\" has blowd up");
//                dead = true;
//            }
//        }
//    }
//}