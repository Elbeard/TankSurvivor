using System.IO;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Пункты меню <b>Tools</b> для prefab чанков.
/// </summary>
public static class WorldChunkPrefabGenerator
{
    private const string PrefabFolder = "Assets/WorldChunks/Prefabs";
    private const string SettingsPath = "Assets/WorldChunks/Settings/DefaultChunkSettings.asset";
    private const string ResourcesFolder = "Assets/WorldChunks/Resources";
    private const string ChunkQuadPath = "Assets/WorldChunks/Resources/ChunkQuad.png";

    private const string ToolsRoot = "Tools/";

    // --- Главный пункт: сразу в меню Tools (без подменю) ---

    [MenuItem(ToolsRoot + "Repair World Chunk Prefabs", false, 200)]
    public static void RepairAllWorldChunkPrefabs()
    {
        EnsureFolder(PrefabFolder);
        EnsureFolder("Assets/WorldChunks/Settings");
        EnsureChunkQuadSpriteAsset();
        WorldChunkRuntimeFactory.InvalidateCaches();

        ChunkSettings settings = AssetDatabase.LoadAssetAtPath<ChunkSettings>(SettingsPath);
        if (settings == null)
        {
            settings = ScriptableObject.CreateInstance<ChunkSettings>();
            AssetDatabase.CreateAsset(settings, SettingsPath);
        }

        float chunkSize = settings.chunkSize;
        GameObject[] templates = WorldChunkRuntimeFactory.CreateDefaultVariantPrefabs(chunkSize);
        string[] names = { "Chunk_Grass_A", "Chunk_Grass_B", "Chunk_Rock_C" };
        var chunkPrefabAssets = new GameObject[names.Length];

        for (int i = 0; i < templates.Length; i++)
        {
            templates[i].name = names[i];
            string path = $"{PrefabFolder}/{names[i]}.prefab";
            chunkPrefabAssets[i] = PrefabUtility.SaveAsPrefabAsset(templates[i], path);
            Object.DestroyImmediate(templates[i]);
        }

        var systemGo = new GameObject("WorldChunkSystem");
        var chunkRootGo = new GameObject("ChunkRoot");
        chunkRootGo.transform.SetParent(systemGo.transform, false);

        ChunkStreamer streamer = systemGo.AddComponent<ChunkStreamer>();
        SerializedObject so = new SerializedObject(streamer);

        SetProperty(so, "_settings", settings);
        SetProperty(so, "_chunkRoot", chunkRootGo.transform);
        SetProperty(so, "_player", null);
        SetProperty(so, "_autoFindPlayer", true);

        SerializedProperty prefabsProp = so.FindProperty("_chunkPrefabs");
        if (prefabsProp != null)
        {
            prefabsProp.arraySize = chunkPrefabAssets.Length;
            for (int i = 0; i < chunkPrefabAssets.Length; i++)
                prefabsProp.GetArrayElementAtIndex(i).objectReferenceValue = chunkPrefabAssets[i];
        }

        so.ApplyModifiedProperties();

        string systemPath = $"{PrefabFolder}/WorldChunkSystem.prefab";
        PrefabUtility.SaveAsPrefabAsset(systemGo, systemPath);
        Object.DestroyImmediate(systemGo);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        EditorUtility.DisplayDialog(
            "World Chunks",
            "Prefab пересозданы:\n• Chunk_Grass_A, B, Rock_C\n• WorldChunkSystem\n\n" +
            "Если на сцене был старый WorldChunkSystem — удалите и перетащите prefab заново.",
            "OK");

        Debug.Log("[WorldChunks] Repair завершён. Prefab в Assets/WorldChunks/Prefabs/");
    }

    // --- Подменю Tools → World Chunks ---

    [MenuItem(ToolsRoot + "World Chunks/Repair All World Chunk Prefabs", false, 0)]
    public static void RepairFromSubmenu()
    {
        RepairAllWorldChunkPrefabs();
    }

    [MenuItem(ToolsRoot + "World Chunks/Generate Example Prefabs", false, 1)]
    public static void GenerateExamplePrefabs()
    {
        RepairAllWorldChunkPrefabs();
    }

    [MenuItem(ToolsRoot + "World Chunks/Add World Chunk System To Open Scene", false, 2)]
    public static void AddWorldChunkSystemToScene()
    {
        string path = $"{PrefabFolder}/WorldChunkSystem.prefab";
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
        if (prefab == null)
        {
            EditorUtility.DisplayDialog(
                "World Chunks",
                "Prefab WorldChunkSystem не найден.\nСначала: Tools → Repair World Chunk Prefabs",
                "OK");
            return;
        }

        if (Object.FindObjectOfType<ChunkStreamer>() != null)
        {
            Debug.LogWarning("[WorldChunks] ChunkStreamer уже есть на сцене.");
            return;
        }

        PrefabUtility.InstantiatePrefab(prefab);
        Debug.Log("[WorldChunks] WorldChunkSystem добавлен на сцену.");
    }

    [MenuItem(ToolsRoot + "World Chunks/Create Default Settings Asset", false, 3)]
    public static void CreateDefaultSettings()
    {
        EnsureFolder("Assets/WorldChunks/Settings");

        if (AssetDatabase.LoadAssetAtPath<ChunkSettings>(SettingsPath) != null)
        {
            Debug.Log("[WorldChunks] DefaultChunkSettings уже существует.");
            return;
        }

        var settings = ScriptableObject.CreateInstance<ChunkSettings>();
        AssetDatabase.CreateAsset(settings, SettingsPath);
        AssetDatabase.SaveAssets();
        Debug.Log($"[WorldChunks] Создан {SettingsPath}");
    }

    private static void SetProperty(SerializedObject so, string name, Object value)
    {
        SerializedProperty prop = so.FindProperty(name);
        if (prop == null)
        {
            Debug.LogError($"[WorldChunks] Поле {name} не найдено на ChunkStreamer. Перекомпилируйте скрипты.");
            return;
        }

        prop.objectReferenceValue = value;
    }

    private static void SetProperty(SerializedObject so, string name, bool value)
    {
        SerializedProperty prop = so.FindProperty(name);
        if (prop == null)
        {
            Debug.LogError($"[WorldChunks] Поле {name} не найдено на ChunkStreamer.");
            return;
        }

        prop.boolValue = value;
    }

    /// <summary>Создаёт белый спрайт в проекте (builtin UISprite в 2022 часто недоступен).</summary>
    public static void EnsureChunkQuadSpriteAsset()
    {
        EnsureFolder(ResourcesFolder);

        if (AssetDatabase.LoadAssetAtPath<Sprite>(ChunkQuadPath) != null)
            return;

        var tex = new Texture2D(4, 4, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Point;
        var pixels = new Color[16];
        for (int i = 0; i < pixels.Length; i++)
            pixels[i] = Color.white;
        tex.SetPixels(pixels);
        tex.Apply();

        File.WriteAllBytes(ChunkQuadPath, tex.EncodeToPNG());
        Object.DestroyImmediate(tex);

        AssetDatabase.Refresh();

        var importer = AssetImporter.GetAtPath(ChunkQuadPath) as TextureImporter;
        if (importer != null)
        {
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.spritePixelsPerUnit = 4f;
            importer.filterMode = FilterMode.Point;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.SaveAndReimport();
        }

        Debug.Log($"[WorldChunks] Создан спрайт {ChunkQuadPath}");
    }

    private static void EnsureFolder(string path)
    {
        if (AssetDatabase.IsValidFolder(path))
            return;

        string parent = Path.GetDirectoryName(path)?.Replace('\\', '/');
        string folderName = Path.GetFileName(path);
        if (!string.IsNullOrEmpty(parent) && !string.IsNullOrEmpty(folderName))
            AssetDatabase.CreateFolder(parent, folderName);
    }
}
