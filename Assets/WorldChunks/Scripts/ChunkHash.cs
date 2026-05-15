using UnityEngine;

/// <summary>
    /// Детерминированный выбор варианта чанка по координатам и seed мира.
    /// Один и тот же (chunkX, chunkY, worldSeed) всегда даёт один variantIndex.
    /// </summary>
    public static class ChunkHash
    {
        /// <summary>Индекс варианта в массиве префабов [0 .. variantCount).</summary>
        public static int GetVariantIndex(int chunkX, int chunkY, int worldSeed, int variantCount)
        {
            if (variantCount <= 0)
                return 0;

            unchecked
            {
                int hash = 17;
                hash = hash * 31 + chunkX;
                hash = hash * 31 + chunkY;
                hash = hash * 31 + worldSeed;
                hash = Mathf.Abs(hash);
                return hash % variantCount;
            }
        }
    }
