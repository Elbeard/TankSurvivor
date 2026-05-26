using UnityEngine;

namespace ChunkWorld
{
    /// <summary>
    /// Биомы пола чанка. Имена совпадают с подпапками в Content/Textures/.
    /// </summary>
    public enum ChunkWorldBiome
    {
        Grass = 0,
        Sand = 1,
        Stone = 2,
        Taiga = 3
    }

    /// <summary>
    /// Детерминированный выбор биома по координатам чанка и seed мира.
    /// </summary>
    public static class ChunkWorldBiomeUtil
    {
        public const int BiomeCount = 4;

        public static ChunkWorldBiome GetBiomeForChunk(Vector2Int coord, int worldSeed, ChunkWorldConfig config = null)
        {
            if (config != null && config.biomeSelection == BiomeSelectionMode.FixedSingle)
                return config.fixedBiome;

            int dist = Mathf.Abs(coord.x) + Mathf.Abs(coord.y);
            int roll = ChunkWorldHash.GetRoll(coord.x, coord.y, worldSeed, 100);

            if (dist <= 2)
                return RollToBiome(roll, grassMax: 55, sandMax: 70, taigaMax: 88);

            if (dist <= 5)
                return RollToBiome(roll, grassMax: 30, sandMax: 50, taigaMax: 75);

            return RollToBiome(roll, grassMax: 10, sandMax: 30, taigaMax: 60);
        }

        public static int BiomeToVariantIndex(ChunkWorldBiome biome)
        {
            return Mathf.Clamp((int)biome, 0, BiomeCount - 1);
        }

        private static ChunkWorldBiome RollToBiome(int roll, int grassMax, int sandMax, int taigaMax)
        {
            if (roll < grassMax)
                return ChunkWorldBiome.Grass;

            if (roll < sandMax)
                return ChunkWorldBiome.Sand;

            if (roll < taigaMax)
                return ChunkWorldBiome.Taiga;

            return ChunkWorldBiome.Stone;
        }
    }
}
