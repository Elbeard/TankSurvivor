using System.Collections.Generic;
using UnityEngine;

/// <summary>
    /// Строит набор координат чанков, которые должны быть активны вокруг игрока.
    /// </summary>
    public static class ChunkRequiredSetBuilder
    {
        /// <summary>
        /// Возвращает coord, которые нужно держать загруженными.
        /// </summary>
        /// <param name="playerChunk">Чанк под позицией игрока.</param>
        /// <param name="moveDirection">Нормализованное направление движения (последнее известное, если стоит).</param>
        /// <param name="speed">Текущая скорость rigidbody.</param>
        /// <param name="settings">Настройки буфера.</param>
        public static HashSet<Vector2Int> Build(
            Vector2Int playerChunk,
            Vector2 moveDirection,
            float speed,
            ChunkSettings settings)
        {
            var set = new HashSet<Vector2Int>();

            bool moving = speed >= settings.minMoveSpeedForDirectedChunks;

            if (!moving)
            {
                int r = settings.idleRadius;
                for (int dx = -r; dx <= r; dx++)
                for (int dy = -r; dy <= r; dy++)
                    set.Add(new Vector2Int(playerChunk.x + dx, playerChunk.y + dy));

                return set;
            }

            Vector2Int forward = ChunkCoordUtil.GetDominantAxis(moveDirection);
            Vector2Int right = new Vector2Int(-forward.y, forward.x);

            for (int along = -settings.chunksBehind; along <= settings.chunksAhead; along++)
            {
                for (int side = -settings.chunksSide; side <= settings.chunksSide; side++)
                {
                    Vector2Int coord = playerChunk + forward * along + right * side;
                    set.Add(coord);
                }
            }

            // Всегда держим чанк под игроком
            set.Add(playerChunk);
            return set;
        }
    }
