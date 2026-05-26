using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ChunkWorld.Editor
{
    /// <summary>
    /// Меню ChunkWorld: настройки, prefab системы, сцена.
    /// </summary>
    public static class ChunkWorldEditorMenus
    {
        private const string SettingsFolder = "Assets/ChunkWorld/Content/Settings";
        private const string PrefabFolder = "Assets/ChunkWorld/Content/Prefabs";
        private const string TexturesRoot = "Assets/ChunkWorld/Content/Textures";
        private const string DefaultCatalogPath = SettingsFolder + "/DefaultBiomeTextures.asset";
        private const string DefaultConfigPath = SettingsFolder + "/DefaultChunkWorldConfig.asset";
        private const string SystemPrefabPath = PrefabFolder + "/ChunkWorldSystem.prefab";

        [MenuItem("Tools/ChunkWorld/Create Default Settings Assets", false, 100)]
        public static void CreateDefaultSettingsAssets()
        {
            EnsureFolder(SettingsFolder);

            ChunkWorldBiomeTextureCatalog catalog = LoadOrCreateCatalog();
            AutoFillCatalogFromFolders(catalog);
            EditorUtility.SetDirty(catalog);

            ChunkWorldConfig config = AssetDatabase.LoadAssetAtPath<ChunkWorldConfig>(DefaultConfigPath);
            if (config == null)
            {
                config = ScriptableObject.CreateInstance<ChunkWorldConfig>();
                config.biomeTextures = catalog;
                AssetDatabase.CreateAsset(config, DefaultConfigPath);
            }
            else if (config.biomeTextures == null)
            {
                config.biomeTextures = catalog;
                EditorUtility.SetDirty(config);
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            float effective = config != null ? config.GetEffectiveChunkSize() : 0f;
            Sprite grass = catalog.GetGroundSprite(ChunkWorldBiome.Grass);
            string sizeHint = grass != null
                ? $"\nСпрайт Grass в мире: {grass.bounds.size.x:F2}×{grass.bounds.size.y:F2} u\nChunk Size (effective): {effective:F2}"
                : "";

            EditorUtility.DisplayDialog(
                "ChunkWorld",
                "Создано или обновлено:\n" +
                "• DefaultBiomeTextures.asset\n" +
                "• DefaultChunkWorldConfig.asset" + sizeHint,
                "OK");

            Selection.activeObject = catalog;
        }

        [MenuItem("Tools/ChunkWorld/Refresh Biome Textures From Folders", false, 101)]
        public static void RefreshBiomeTexturesFromFolders()
        {
            ChunkWorldBiomeTextureCatalog catalog = LoadOrCreateCatalog();
            AutoFillCatalogFromFolders(catalog);
            EditorUtility.SetDirty(catalog);
            AssetDatabase.SaveAssets();
            Debug.Log("[ChunkWorld] Спрайты в каталоге обновлены.");
            Selection.activeObject = catalog;
        }

        [MenuItem("Tools/ChunkWorld/Create ChunkWorld System Prefab", false, 110)]
        public static void CreateChunkWorldSystemPrefab()
        {
            EnsureFolder(PrefabFolder);
            CreateDefaultSettingsAssets();

            ChunkWorldConfig config = AssetDatabase.LoadAssetAtPath<ChunkWorldConfig>(DefaultConfigPath);
            if (config == null)
            {
                Debug.LogError("[ChunkWorld] Нет DefaultChunkWorldConfig.");
                return;
            }

            var systemGo = new GameObject("ChunkWorldSystem");
            var chunkRootGo = new GameObject("ChunkRoot");
            chunkRootGo.transform.SetParent(systemGo.transform, false);

            ChunkWorldStreamer streamer = systemGo.AddComponent<ChunkWorldStreamer>();
            SerializedObject so = new SerializedObject(streamer);
            SetRef(so, "_config", config);
            SetRef(so, "_chunkRoot", chunkRootGo.transform);
            SetBool(so, "_autoFindPlayerByName", true);
            so.ApplyModifiedProperties();

            PrefabUtility.SaveAsPrefabAsset(systemGo, SystemPrefabPath);
            Object.DestroyImmediate(systemGo);

            AssetDatabase.SaveAssets();
            Debug.Log($"[ChunkWorld] Prefab сохранён: {SystemPrefabPath}");
            Selection.activeObject = AssetDatabase.LoadAssetAtPath<GameObject>(SystemPrefabPath);
        }

        [MenuItem("Tools/ChunkWorld/Setup Open Scene (Replace Legacy WorldChunks)", false, 120)]
        public static void SetupOpenScene()
        {
            CreateChunkWorldSystemPrefab();

            GameObject legacy = GameObject.Find("WorldChunkSystem");
            if (legacy != null)
            {
                Object.DestroyImmediate(legacy);
                Debug.Log("[ChunkWorld] Удалён старый WorldChunkSystem.");
            }

            if (Object.FindObjectOfType<ChunkWorldStreamer>() != null)
            {
                Debug.LogWarning("[ChunkWorld] ChunkWorldStreamer уже на сцене.");
                return;
            }

            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(SystemPrefabPath);
            if (prefab == null)
            {
                Debug.LogError("[ChunkWorld] Prefab не найден. Запустите Create ChunkWorld System Prefab.");
                return;
            }

            PrefabUtility.InstantiatePrefab(prefab, SceneManager.GetActiveScene());

            EnsureSceneBootstrap();

            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
            Debug.Log("[ChunkWorld] ChunkWorldSystem добавлен на сцену. Сохраните сцену (Ctrl+S).");
        }

        private static void EnsureSceneBootstrap()
        {
            const string bootstrapTypeName = "ChunkWorldSceneBootstrap";
            foreach (MonoBehaviour mb in Object.FindObjectsOfType<MonoBehaviour>())
            {
                if (mb != null && mb.GetType().Name == bootstrapTypeName)
                    return;
            }

            var bootstrapGo = new GameObject("ChunkWorldBootstrap");
            var bootstrapType = System.Type.GetType($"{bootstrapTypeName}, Assembly-CSharp");
            if (bootstrapType != null)
                bootstrapGo.AddComponent(bootstrapType);
            else
                Debug.LogWarning("[ChunkWorld] Добавьте ChunkWorldSceneBootstrap на сцену вручную (скрипт в Assets/Scripts).");
        }

        private static ChunkWorldBiomeTextureCatalog LoadOrCreateCatalog()
        {
            var catalog = AssetDatabase.LoadAssetAtPath<ChunkWorldBiomeTextureCatalog>(DefaultCatalogPath);
            if (catalog != null)
                return catalog;

            catalog = ScriptableObject.CreateInstance<ChunkWorldBiomeTextureCatalog>();
            AssetDatabase.CreateAsset(catalog, DefaultCatalogPath);
            return catalog;
        }

        private static void AutoFillCatalogFromFolders(ChunkWorldBiomeTextureCatalog catalog)
        {
            if (catalog.entries == null || catalog.entries.Length < 4)
            {
                catalog.entries = new ChunkWorldBiomeTextureEntry[]
                {
                    new() { biome = ChunkWorldBiome.Grass },
                    new() { biome = ChunkWorldBiome.Sand },
                    new() { biome = ChunkWorldBiome.Stone },
                    new() { biome = ChunkWorldBiome.Taiga }
                };
            }

            for (int i = 0; i < catalog.entries.Length; i++)
            {
                string folder = BiomeToFolderName(catalog.entries[i].biome);
                Sprite sprite = FindMainSpriteInFolder($"{TexturesRoot}/{folder}");
                if (sprite == null && catalog.entries[i].biome == ChunkWorldBiome.Grass)
                    sprite = FindMainSpriteInFolder($"{TexturesRoot}/grass");

                if (sprite != null)
                    catalog.entries[i].groundSprite = sprite;
            }
        }

        private static string BiomeToFolderName(ChunkWorldBiome biome)
        {
            return biome switch
            {
                ChunkWorldBiome.Grass => "Grass",
                ChunkWorldBiome.Sand => "Send",
                ChunkWorldBiome.Stone => "Stone",
                ChunkWorldBiome.Taiga => "Taiga",
                _ => biome.ToString()
            };
        }

        private static Sprite FindMainSpriteInFolder(string folderPath)
        {
            if (!AssetDatabase.IsValidFolder(folderPath))
                return null;

            string[] guids = AssetDatabase.FindAssets("t:Sprite", new[] { folderPath });
            Sprite preferred = null;
            Sprite fallback = null;
            string folderName = Path.GetFileName(folderPath);

            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
                if (sprite == null)
                    continue;

                fallback ??= sprite;

                string fileName = Path.GetFileNameWithoutExtension(path);
                if (string.Equals(fileName, folderName, System.StringComparison.OrdinalIgnoreCase))
                    preferred = sprite;
            }

            return preferred != null ? preferred : fallback;
        }

        private static void SetRef(SerializedObject so, string name, Object value)
        {
            SerializedProperty prop = so.FindProperty(name);
            if (prop != null)
                prop.objectReferenceValue = value;
        }

        private static void SetBool(SerializedObject so, string name, bool value)
        {
            SerializedProperty prop = so.FindProperty(name);
            if (prop != null)
                prop.boolValue = value;
        }

        private static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path))
                return;

            string parent = Path.GetDirectoryName(path)?.Replace('\\', '/');
            string name = Path.GetFileName(path);
            if (!string.IsNullOrEmpty(parent) && !string.IsNullOrEmpty(name))
                AssetDatabase.CreateFolder(parent, name);
        }
    }
}
