using UnityEngine;

namespace ChunkWorld
{
    /// <summary>Как рисовать текстуру пола на чанке.</summary>
    public enum ChunkGroundLayout
    {
        /// <summary>Один спрайт ровно на весь chunkSize × chunkSize (масштаб от размера спрайта в метрах).</summary>
        FitChunk = 0,

        /// <summary>Повтор плитки в нативном размере (PPU), без растягивания одной картинки.</summary>
        Tile = 1
    }

    /// <summary>
    /// Главные настройки модуля на сцене: каталог текстур и параметры стриминга.
    /// </summary>
    [CreateAssetMenu(fileName = "ChunkWorldConfig", menuName = "ChunkWorld/Chunk World Config", order = 1)]
    public class ChunkWorldConfig : ScriptableObject
    {
        [Header("Текстуры биомов")]
        [Tooltip("Реестр спрайтов пола. Перетащите asset из Content/Settings/.")]
        public ChunkWorldBiomeTextureCatalog biomeTextures;

        [Tooltip("FitChunk — одна текстура на чанк; Tile — замощение (для маленьких seamless-плиток).")]
        public ChunkGroundLayout groundLayout = ChunkGroundLayout.FitChunk;

        [Header("Биомы на уровне")]
        [Tooltip("Fixed — везде один биом; Procedural — трава/песок/камень/тайга по координатам.")]
        public BiomeSelectionMode biomeSelection = BiomeSelectionMode.FixedSingle;

        [Tooltip("Какой биом, если Biome Selection = Fixed Single.")]
        public ChunkWorldBiome fixedBiome = ChunkWorldBiome.Grass;

        [Header("Размер чанка (мировые единицы)")]
        [Tooltip("Manual — число ниже; MatchSprite — как у эталонного спрайта (1024px и PPU 100 → ~10.24).")]
        public ChunkSizeSource chunkSizeSource = ChunkSizeSource.MatchSprite;

        [Tooltip("От какого биома брать размер, если MatchSprite.")]
        public ChunkWorldBiome chunkSizeReferenceBiome = ChunkWorldBiome.Grass;

        [Min(1f)]
        [Tooltip("Используется при Chunk Size Source = Manual. Не ставьте 4800 для текстуры 1024@PPU100.")]
        public float chunkSize = 10.24f;

        [Header("Стриминг")]

        public int worldSeed = 12345;

        [Min(0)]
        public int chunksAhead = 2;

        [Min(0)]
        public int chunksBehind = 2;

        [Min(0)]
        public int chunksSide = 1;

        [Min(1)]
        public int despawnDistance = 4;

        [Min(0.01f)]
        public float minMoveSpeedForDirectedChunks = 0.5f;

        [Min(0)]
        public int idleRadius = 1;

        [Min(0f)]
        public float updateInterval = 0.15f;

        /// <summary>Фактический размер стороны чанка с учётом источника и каталога.</summary>
        public float GetEffectiveChunkSize()
        {
            if (chunkSizeSource != ChunkSizeSource.MatchSprite || biomeTextures == null)
                return chunkSize;

            Sprite reference = biomeTextures.GetGroundSprite(chunkSizeReferenceBiome);
            if (reference == null)
                return chunkSize;

            Vector2 size = reference.bounds.size;
            return Mathf.Max(size.x, size.y, 0.0001f);
        }
    }

    public enum ChunkSizeSource
    {
        Manual = 0,
        MatchSprite = 1
    }

    public enum BiomeSelectionMode
    {
        /// <summary>Один биом на всю карту (для отладки уровня).</summary>
        FixedSingle = 0,

        /// <summary>Выбор по seed и координатам (дистанция от старта).</summary>
        Procedural = 1
    }
}
