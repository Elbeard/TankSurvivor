using UnityEngine;

/// <summary>
    /// Выбирает один из префабов-вариантов по детерминированному hash(coord, worldSeed).
    /// </summary>
    public class PrefabChunkProvider : IChunkProvider
    {
        private readonly GameObject[] _variants;

        public PrefabChunkProvider(GameObject[] variants)
        {
            _variants = variants;
        }

        public int VariantCount => _variants?.Length ?? 0;

        public GameObject GetPrefabForChunk(Vector2Int coord, int worldSeed, out int variantIndex)
        {
            variantIndex = 0;

            if (_variants == null || _variants.Length == 0)
                return null;

            variantIndex = ChunkHash.GetVariantIndex(coord.x, coord.y, worldSeed, _variants.Length);
            return _variants[variantIndex];
        }
    }
