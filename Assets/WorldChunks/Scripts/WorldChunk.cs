using UnityEngine;

/// <summary>
    /// Компонент на экземпляре чанка в сцене. Хранит логическую координату и индекс варианта.
    /// </summary>
    public class WorldChunk : MonoBehaviour
    {
        [SerializeField] private Vector2Int _coord;
        [SerializeField] private int _variantIndex;

        /// <summary>Логическая координата чанка в сетке.</summary>
        public Vector2Int Coord => _coord;

        /// <summary>Индекс варианта префаба (из <see cref="ChunkHash"/>).</summary>
        public int VariantIndex => _variantIndex;

        /// <summary>Вызывается <see cref="ChunkStreamer"/> при появлении чанка.</summary>
        public void Initialize(Vector2Int coord, int variantIndex)
        {
            _coord = coord;
            _variantIndex = variantIndex;
            name = $"Chunk_{coord.x}_{coord.y}";
            WorldChunkRuntimeFactory.EnsureChunkVisuals(gameObject, variantIndex);
        }

        /// <summary>Вызывается перед возвратом в пул.</summary>
        public void OnReturnedToPool()
        {
            // Точка расширения: сброс декора, сохранение состояния в кэш и т.д.
        }
    }
