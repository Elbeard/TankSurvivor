using UnityEngine;

/// <summary>
/// Визуал чанков. Использует встроенный спрайт Unity + цвет (сохраняется в prefab).
/// После правок: Tools → Repair World Chunk Prefabs.
/// </summary>
public static class WorldChunkRuntimeFactory
{
    private static GameObject[] _cachedTemplates;
    private static Transform _templateRoot;
    private static Sprite _quadSprite;

    private enum ChunkVisualStyle
    {
        GrassPlain,
        GrassChecker,
        Rocky
    }

    private const string ChunkQuadResourcePath = "ChunkQuad";

    /// <summary>Белый квадрат из Assets/WorldChunks/Resources/ChunkQuad.png (создаётся через Tools → Repair).</summary>
    public static Sprite QuadSprite => GetQuadSprite();

    public static void InvalidateCaches()
    {
        _quadSprite = null;
        _cachedTemplates = null;
    }

    private static Sprite GetQuadSprite()
    {
        if (_quadSprite != null)
            return _quadSprite;

        _quadSprite = Resources.Load<Sprite>(ChunkQuadResourcePath);
        if (_quadSprite == null)
            _quadSprite = CreateFallbackSprite();

        return _quadSprite;
    }

    public static GameObject[] CreateDefaultVariantPrefabs(float chunkSize)
    {
        if (!Application.isPlaying)
            return BuildVariantArray(chunkSize);

        if (_cachedTemplates != null && _cachedTemplates.Length > 0)
            return _cachedTemplates;

        EnsureTemplateRoot();
        _cachedTemplates = BuildVariantArray(chunkSize);

        foreach (GameObject template in _cachedTemplates)
            template.transform.SetParent(_templateRoot, false);

        return _cachedTemplates;
    }

    /// <summary>Подставляет спрайты, если prefab сохранился с пустыми SpriteRenderer (старый Repair).</summary>
    public static void EnsureChunkVisuals(GameObject root, int variantIndex)
    {
        if (root == null)
            return;

        bool anyMissing = false;
        foreach (SpriteRenderer sr in root.GetComponentsInChildren<SpriteRenderer>(true))
        {
            if (sr.sprite == null)
            {
                anyMissing = true;
                break;
            }
        }

        if (!anyMissing)
            return;

        ChunkVisualStyle style = (ChunkVisualStyle)Mathf.Clamp(variantIndex, 0, 2);
        float chunkSize = 4800f;
        Transform ground = root.transform.Find("Ground");
        if (ground != null)
            chunkSize = Mathf.Max(ground.localScale.x, 1f);

        // Пересобрать визуал поверх существующего корня
        for (int i = root.transform.childCount - 1; i >= 0; i--)
            Object.Destroy(root.transform.GetChild(i).gameObject);

        var wc = root.GetComponent<WorldChunk>();
        if (wc == null)
            root.AddComponent<WorldChunk>();

        BuildVisualChildren(root.transform, chunkSize, style);
    }

    private static GameObject[] BuildVariantArray(float chunkSize)
    {
        return new[]
        {
            BuildStyledChunk("Chunk_Runtime_Grass_A", chunkSize, ChunkVisualStyle.GrassPlain),
            BuildStyledChunk("Chunk_Runtime_Grass_B", chunkSize, ChunkVisualStyle.GrassChecker),
            BuildStyledChunk("Chunk_Runtime_Rock_C", chunkSize, ChunkVisualStyle.Rocky)
        };
    }

    private static void EnsureTemplateRoot()
    {
        if (_templateRoot != null)
            return;

        var root = new GameObject("WorldChunks_RuntimeTemplates");
        root.hideFlags = HideFlags.HideAndDontSave;
        Object.DontDestroyOnLoad(root);
        _templateRoot = root.transform;
    }

    private static GameObject BuildStyledChunk(string name, float chunkSize, ChunkVisualStyle style)
    {
        var root = new GameObject(name);
        root.AddComponent<WorldChunk>();
        BuildVisualChildren(root.transform, chunkSize, style);
        root.SetActive(true);
        return root;
    }

    private static void BuildVisualChildren(Transform root, float chunkSize, ChunkVisualStyle style)
    {
        Color baseColor;
        switch (style)
        {
            case ChunkVisualStyle.GrassPlain:
                baseColor = new Color(0.45f, 0.78f, 0.35f);
                break;
            case ChunkVisualStyle.GrassChecker:
                baseColor = new Color(0.28f, 0.52f, 0.22f);
                break;
            default:
                baseColor = new Color(0.58f, 0.50f, 0.40f);
                break;
        }

        AddGroundQuad(root, chunkSize, baseColor);

        switch (style)
        {
            case ChunkVisualStyle.GrassPlain:
                AddCornerMarkers(root, chunkSize, new Color(0.95f, 0.92f, 0.55f));
                AddScatteredRocks(root, chunkSize, "GrassA", 2, new Color(0.22f, 0.38f, 0.18f), 0.6f, 1f);
                break;
            case ChunkVisualStyle.GrassChecker:
                AddCheckerGrid(root, chunkSize,
                    new Color(0.38f, 0.68f, 0.30f),
                    new Color(0.18f, 0.38f, 0.14f));
                AddScatteredRocks(root, chunkSize, "GrassB", 3, new Color(0.35f, 0.28f, 0.20f), 0.5f, 0.9f);
                break;
            case ChunkVisualStyle.Rocky:
                AddBorderStripes(root, chunkSize, new Color(0.32f, 0.28f, 0.24f));
                AddCenterBoulder(root, chunkSize, new Color(0.42f, 0.38f, 0.34f));
                AddScatteredRocks(root, chunkSize, "RockC", 7, new Color(0.25f, 0.22f, 0.20f), 0.7f, 1.4f);
                break;
        }
    }

    private static void AddGroundQuad(Transform parent, float chunkSize, Color color)
    {
        var ground = new GameObject("Ground");
        ground.transform.SetParent(parent, false);
        ground.transform.localPosition = new Vector3(chunkSize * 0.5f, chunkSize * 0.5f, 0f);
        ground.transform.localScale = new Vector3(chunkSize, chunkSize, 1f);
        SetupSprite(ground, color, -100);
    }

    private static void AddCornerMarkers(Transform parent, float chunkSize, Color color)
    {
        float inset = 1.2f;
        float size = 1.8f;
        Vector3[] corners =
        {
            new Vector3(inset, inset, 0f),
            new Vector3(chunkSize - inset, inset, 0f),
            new Vector3(inset, chunkSize - inset, 0f),
            new Vector3(chunkSize - inset, chunkSize - inset, 0f)
        };

        for (int i = 0; i < corners.Length; i++)
            AddDecorQuad(parent, $"Corner_{i}", corners[i], new Vector3(size, size, 1f), color, -88);
    }

    /// <summary>Сетка из квадратов — каждый с builtin-спрайтом, сохраняется в prefab.</summary>
    private static void AddCheckerGrid(Transform parent, float chunkSize, Color colorA, Color colorB)
    {
        const int cells = 4;
        float cellSize = chunkSize / cells;
        var gridRoot = new GameObject("CheckerGrid");
        gridRoot.transform.SetParent(parent, false);

        for (int y = 0; y < cells; y++)
        {
            for (int x = 0; x < cells; x++)
            {
                bool useA = (x + y) % 2 == 0;
                var cell = new GameObject($"Cell_{x}_{y}");
                cell.transform.SetParent(gridRoot.transform, false);
                float cx = x * cellSize + cellSize * 0.5f;
                float cy = y * cellSize + cellSize * 0.5f;
                cell.transform.localPosition = new Vector3(cx, cy, 0f);
                cell.transform.localScale = new Vector3(cellSize, cellSize, 1f);
                SetupSprite(cell, useA ? colorA : colorB, -99);
            }
        }
    }

    private static void AddBorderStripes(Transform parent, float chunkSize, Color color)
    {
        float t = 0.55f;
        AddDecorQuad(parent, "Border_Bottom", new Vector3(chunkSize * 0.5f, t * 0.5f, 0f), new Vector3(chunkSize, t, 1f), color, -95);
        AddDecorQuad(parent, "Border_Top", new Vector3(chunkSize * 0.5f, chunkSize - t * 0.5f, 0f), new Vector3(chunkSize, t, 1f), color, -95);
        AddDecorQuad(parent, "Border_Left", new Vector3(t * 0.5f, chunkSize * 0.5f, 0f), new Vector3(t, chunkSize, 1f), color, -95);
        AddDecorQuad(parent, "Border_Right", new Vector3(chunkSize - t * 0.5f, chunkSize * 0.5f, 0f), new Vector3(t, chunkSize, 1f), color, -95);
    }

    private static void AddCenterBoulder(Transform parent, float chunkSize, Color color)
    {
        AddDecorQuad(parent, "Boulder", new Vector3(chunkSize * 0.5f, chunkSize * 0.5f, 0f),
            new Vector3(4.5f, 4.5f, 1f), color, -92);
    }

    private static void AddScatteredRocks(Transform parent, float chunkSize, string seedName, int count, Color color, float scaleMin, float scaleMax)
    {
        var decorRoot = new GameObject("Decor");
        decorRoot.transform.SetParent(parent, false);
        var rng = new System.Random(seedName.GetHashCode());

        for (int i = 0; i < count; i++)
        {
            float px = 2f + (float)rng.NextDouble() * (chunkSize - 4f);
            float py = 2f + (float)rng.NextDouble() * (chunkSize - 4f);
            float s = scaleMin + (float)rng.NextDouble() * (scaleMax - scaleMin);
            AddDecorQuad(decorRoot.transform, $"Rock_{i}", new Vector3(px, py, 0f), new Vector3(s, s, 1f), color, -90);
        }
    }

    private static void AddDecorQuad(Transform parent, string name, Vector3 localPos, Vector3 scale, Color color, int sortingOrder)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        go.transform.localPosition = localPos;
        go.transform.localScale = scale;
        SetupSprite(go, color, sortingOrder);
    }

    private static void SetupSprite(GameObject go, Color color, int sortingOrder)
    {
        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = QuadSprite;
        sr.color = color;
        sr.sortingOrder = sortingOrder;
    }

    private static Sprite CreateFallbackSprite()
    {
        var tex = new Texture2D(4, 4, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Point;
        var pixels = new Color[16];
        for (int i = 0; i < pixels.Length; i++)
            pixels[i] = Color.white;
        tex.SetPixels(pixels);
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, 4, 4), new Vector2(0.5f, 0.5f), 4f);
    }
}
