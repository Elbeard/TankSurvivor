using System.Collections.Generic;
using UnityEngine;

namespace ChunkWorld
{
    /// <summary>
    /// Набор координат чанков, которые должны быть активны вокруг цели следования.
    /// </summary>
    public static class ChunkWorldRequiredSetBuilder
    {
        public static HashSet<Vector2Int> Build(
            Vector2Int playerChunk,
            Vector2 moveDirection,
            float speed,
            ChunkWorldConfig config)
        {
            var set = new HashSet<Vector2Int>();

            bool moving = speed >= config.minMoveSpeedForDirectedChunks;

            if (!moving)
            {
                int r = config.idleRadius;
                for (int dx = -r; dx <= r; dx++)
                for (int dy = -r; dy <= r; dy++)
                    set.Add(new Vector2Int(playerChunk.x + dx, playerChunk.y + dy));

                return set;
            }

            Vector2Int forward = ChunkWorldCoordUtil.GetDominantAxis(moveDirection);
            Vector2Int right = new Vector2Int(-forward.y, forward.x);

            for (int along = -config.chunksBehind; along <= config.chunksAhead; along++)
            {
                for (int side = -config.chunksSide; side <= config.chunksSide; side++)
                {
                    Vector2Int coord = playerChunk + forward * along + right * side;
                    set.Add(coord);
                }
            }

            set.Add(playerChunk);
            return set;
        }
    }
}
