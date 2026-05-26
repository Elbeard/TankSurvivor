using UnityEngine;

namespace ChunkWorld
{
    /// <summary>
    /// Экземпляр чанка в сцене: координата и биом.
    /// </summary>
    public class ChunkWorldChunk : MonoBehaviour
    {
        [SerializeField] private Vector2Int _coord;
        [SerializeField] private ChunkWorldBiome _biome;

        public Vector2Int Coord => _coord;
        public ChunkWorldBiome Biome => _biome;
        public int VariantIndex => ChunkWorldBiomeUtil.BiomeToVariantIndex(_biome);

        public void Initialize(
            Vector2Int coord,
            ChunkWorldBiome biome,
            float chunkSize,
            ChunkWorldBiomeTextureCatalog catalog,
            ChunkGroundLayout groundLayout = ChunkGroundLayout.FitChunk)
        {
            _coord = coord;
            _biome = biome;
            name = $"Chunk_{coord.x}_{coord.y}";
            ChunkWorldChunkBuilder.RebuildGround(transform, biome, chunkSize, catalog, groundLayout);
        }

        public void OnReturnedToPool()
        {
            // Точка расширения: сброс декора с DecorPrefabs.
        }
    }
}
