using UnityEngine;

/// <summary>
    /// Выбор префаба для логической координаты чанка.
    /// Реализации: <see cref="PrefabChunkProvider"/> (сейчас), позже — tilemap/noise.
    /// </summary>
    public interface IChunkProvider
    {
        /// <summary>Префаб для спавна или null, если вариантов нет.</summary>
        GameObject GetPrefabForChunk(Vector2Int coord, int worldSeed, out int variantIndex);
    }
