using System.Collections.Generic;
using UnityEngine;

/// <summary>
    /// Пул экземпляров <see cref="WorldChunk"/>. Отдельная очередь на каждый индекс варианта префаба.
    /// </summary>
    public class ChunkPool
    {
        private readonly Transform _parent;
        private readonly Dictionary<int, Stack<WorldChunk>> _stacksByVariant = new();

        public ChunkPool(Transform parent)
        {
            _parent = parent;
        }

        /// <summary>Берёт чанк из пула или создаёт новый из prefab.</summary>
        public WorldChunk Get(GameObject prefab, int variantIndex, Vector3 worldOrigin)
        {
            if (!_stacksByVariant.TryGetValue(variantIndex, out Stack<WorldChunk> stack))
            {
                stack = new Stack<WorldChunk>();
                _stacksByVariant[variantIndex] = stack;
            }

            WorldChunk chunk;

            if (stack.Count > 0)
            {
                chunk = stack.Pop();
                chunk.gameObject.SetActive(true);
            }
            else
            {
                GameObject instance = Object.Instantiate(prefab, _parent);
                instance.SetActive(true);
                chunk = instance.GetComponent<WorldChunk>();
                if (chunk == null)
                    chunk = instance.AddComponent<WorldChunk>();
            }

            chunk.gameObject.SetActive(true);
            chunk.transform.position = worldOrigin;
            chunk.transform.rotation = Quaternion.identity;
            return chunk;
        }

        /// <summary>Возвращает чанк в пул (деактивация, без Destroy).</summary>
        public void Release(WorldChunk chunk, int variantIndex)
        {
            if (chunk == null)
                return;

            chunk.OnReturnedToPool();
            chunk.gameObject.SetActive(false);

            if (!_stacksByVariant.TryGetValue(variantIndex, out Stack<WorldChunk> stack))
            {
                stack = new Stack<WorldChunk>();
                _stacksByVariant[variantIndex] = stack;
            }

            stack.Push(chunk);
        }

        /// <summary>Очистка при выгрузке сцены.</summary>
        public void ClearAll()
        {
            foreach (Stack<WorldChunk> stack in _stacksByVariant.Values)
            {
                while (stack.Count > 0)
                {
                    WorldChunk chunk = stack.Pop();
                    if (chunk != null)
                        Object.Destroy(chunk.gameObject);
                }
            }

            _stacksByVariant.Clear();
        }
    }
