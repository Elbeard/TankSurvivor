using System.Collections.Generic;
using UnityEngine;

namespace ChunkWorld
{
    /// <summary>
    /// Пул чанков по индексу биома.
    /// </summary>
    public class ChunkWorldChunkPool
    {
        private readonly Transform _parent;
        private readonly Dictionary<int, Stack<ChunkWorldChunk>> _stacksByBiome = new();

        public ChunkWorldChunkPool(Transform parent)
        {
            _parent = parent;
        }

        public ChunkWorldChunk Get(
            Vector2Int coord,
            ChunkWorldBiome biome,
            Vector3 worldOrigin,
            float chunkSize,
            ChunkWorldBiomeTextureCatalog catalog,
            ChunkGroundLayout groundLayout)
        {
            int variantIndex = ChunkWorldBiomeUtil.BiomeToVariantIndex(biome);

            if (!_stacksByBiome.TryGetValue(variantIndex, out Stack<ChunkWorldChunk> stack))
            {
                stack = new Stack<ChunkWorldChunk>();
                _stacksByBiome[variantIndex] = stack;
            }

            ChunkWorldChunk chunk;

            if (stack.Count > 0)
            {
                chunk = stack.Pop();
                chunk.gameObject.SetActive(true);
            }
            else
            {
                chunk = ChunkWorldChunkBuilder.CreateChunkRoot();
                chunk.transform.SetParent(_parent, false);
            }

            chunk.transform.position = worldOrigin;
            chunk.transform.rotation = Quaternion.identity;
            chunk.Initialize(coord, biome, chunkSize, catalog, groundLayout);
            return chunk;
        }

        public void Release(ChunkWorldChunk chunk, int variantIndex)
        {
            if (chunk == null)
                return;

            chunk.OnReturnedToPool();
            chunk.gameObject.SetActive(false);

            if (!_stacksByBiome.TryGetValue(variantIndex, out Stack<ChunkWorldChunk> stack))
            {
                stack = new Stack<ChunkWorldChunk>();
                _stacksByBiome[variantIndex] = stack;
            }

            stack.Push(chunk);
        }

        public void ClearAll()
        {
            foreach (Stack<ChunkWorldChunk> stack in _stacksByBiome.Values)
            {
                while (stack.Count > 0)
                {
                    ChunkWorldChunk chunk = stack.Pop();
                    if (chunk != null)
                        Object.Destroy(chunk.gameObject);
                }
            }

            _stacksByBiome.Clear();
        }
    }
}
