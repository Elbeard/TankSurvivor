using UnityEngine;

/// <summary>
    /// Преобразование мировых координат в индексы чанков и обратно.
    /// </summary>
    public static class ChunkCoordUtil
    {
        /// <summary>Возвращает индекс чанка, в котором находится точка.</summary>
        public static Vector2Int WorldToChunk(Vector2 worldPosition, float chunkSize)
        {
            int x = Mathf.FloorToInt(worldPosition.x / chunkSize);
            int y = Mathf.FloorToInt(worldPosition.y / chunkSize);
            return new Vector2Int(x, y);
        }

        /// <summary>Мировая позиция левого нижнего угла чанка (Z = 0).</summary>
        public static Vector3 ChunkToWorldOrigin(Vector2Int coord, float chunkSize)
        {
            return new Vector3(coord.x * chunkSize, coord.y * chunkSize, 0f);
        }

        /// <summary>Центр чанка в мировых координатах (удобно для спавна врагов).</summary>
        public static Vector2 ChunkToWorldCenter(Vector2Int coord, float chunkSize)
        {
            float half = chunkSize * 0.5f;
            return new Vector2(
                coord.x * chunkSize + half,
                coord.y * chunkSize + half);
        }

        /// <summary>Manhattan-расстояние между индексами чанков.</summary>
        public static int ManhattanDistance(Vector2Int a, Vector2Int b)
        {
            return Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y);
        }

        /// <summary>
        /// Доминирующая ось направления (вправо/влево/вверх/вниз) для построения буфера ahead/behind.
        /// </summary>
        public static Vector2Int GetDominantAxis(Vector2 direction)
        {
            if (direction.sqrMagnitude < 0.0001f)
                return Vector2Int.right;

            if (Mathf.Abs(direction.x) >= Mathf.Abs(direction.y))
                return direction.x >= 0f ? Vector2Int.right : Vector2Int.left;

            return direction.y >= 0f ? Vector2Int.up : Vector2Int.down;
        }
    }
