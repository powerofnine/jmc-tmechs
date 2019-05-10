using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using JetBrains.Annotations;
using Newtonsoft.Json;
using TMechs.UI.FX;
using UnityEngine;
using Object = UnityEngine.Object;

namespace TMechs.Data
{
    public class SaveSystem
    {
        private static SaveSystem instance;

        private readonly List<LexiconEntry> lexicon = new List<LexiconEntry>();
        private readonly string dataPath;
        private readonly string lexiconPath;

        private SaveSystem()
        {
            dataPath = Path.Combine(Application.persistentDataPath, "Save/");
            Directory.CreateDirectory(dataPath);

            lexiconPath = Path.Combine(dataPath, "lexicon.json");
            if (File.Exists(lexiconPath))
                try
                {
                    lexicon = JsonConvert.DeserializeObject<List<LexiconEntry>>(File.ReadAllText(lexiconPath));
                }
                catch (JsonException e)
                {
                    Debug.LogError("Failed to read the lexicon, the JSON is invalid");
                    Debug.LogError(e.StackTrace);
                }

            int lexiconSize = lexicon.Count;
            lexicon = lexicon.Where(x => File.Exists(Path.Combine(dataPath, x.id + ".json"))).OrderByDescending(x => x.creationTime).ToList();

            if (lexiconSize - lexicon.Count != 0)
                Debug.LogWarningFormat("Removing {0} missing saves from the lexicon", lexiconSize - lexicon.Count);
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Init() => instance = new SaveSystem();

        public static void CreateSave(SaveData data, string meta)
        {
            GameObject display = Object.Instantiate(Resources.Load<GameObject>("UI/SavingWheel"));

            BackgroundWorker worker = new BackgroundWorker();
            worker.DoWork += (sender, args) => instance._CreateSave(data, meta);
            worker.RunWorkerCompleted += (sender, args) => display.GetComponent<UiFade>().Kill();

            worker.RunWorkerAsync();
        }

        private void _CreateSave(SaveData data, string meta)
        {
            LexiconEntry entry = new LexiconEntry {meta = meta};

            // Prevent clashing ids in the lexicon
            while (lexicon.FirstOrDefault(x => x.id == entry.id) != null)
                entry.id = Guid.NewGuid();

            string saveFile = GetSaveFile(entry);

            lexicon.Add(entry);

            File.WriteAllText(saveFile, JsonConvert.SerializeObject(data));
            FlushLexicon();
        }

        public static SaveData LoadSave(LexiconEntry entry)
        {
            return instance._LoadSave(entry);
        }

        private SaveData _LoadSave(LexiconEntry entry)
        {
            string saveFile = GetSaveFile(entry);

            SaveData data = null;

            if (File.Exists(saveFile))
            {
                try
                {
                    data = JsonConvert.DeserializeObject<SaveData>(File.ReadAllText(saveFile));
                }
                catch (JsonException e)
                {
                    Debug.LogErrorFormat("Failed to read save file {0} as the JSON is invalid", saveFile);
                    Debug.LogError(e.StackTrace);
                }
            }
            else
            {
                Debug.LogErrorFormat("Save file {0} does not exist", saveFile);
            }

            return data;
        }

        public static void DeleteSave(LexiconEntry entry)
        {
            instance._DeleteSave(entry);
        }

        private void _DeleteSave(LexiconEntry entry)
        {
            if (lexicon.Contains(entry))
            {
                lexicon.Remove(entry);

                string saveFile = Path.Combine(dataPath, entry.id + ".json");

                if (File.Exists(saveFile))
                    File.Delete(saveFile);

                FlushLexicon();
            }
        }

        private string GetSaveFile(LexiconEntry entry)
            => Path.Combine(dataPath, entry.id + ".json");

        private void FlushLexicon()
            => File.WriteAllText(lexiconPath, JsonConvert.SerializeObject(lexicon));

        public static LexiconEntry[] GetLexicon() => instance.lexicon.ToArray();

        [UsedImplicitly(ImplicitUseKindFlags.Assign, ImplicitUseTargetFlags.WithMembers)]
        public class LexiconEntry
        {
            public Guid id = Guid.NewGuid();
            public int formatVer = 1;
            public DateTime creationTime = DateTime.Now;
            public string meta;
        }

        [UsedImplicitly(ImplicitUseKindFlags.Assign, ImplicitUseTargetFlags.WithMembers)]
        public class SaveData
        {
            public string sceneId;
            public string checkpointId;
            public float health = 1F;
        }
    }
}