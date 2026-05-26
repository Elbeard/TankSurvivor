using UnityEngine;

namespace ChunkWorld
{
    /// <summary>
    /// Детерминированные числа от coord и seed (тот же чанк — тот же результат).
    /// </summary>
    public static class ChunkWorldHash
    {
        public static int GetRoll(int chunkX, int chunkY, int worldSeed, int range)
        {
            if (range <= 0)
                return 0;

            unchecked
            {
                int hash = 17;
                hash = hash * 31 + chunkX;
                hash = hash * 31 + chunkY;
                hash = hash * 31 + worldSeed;
                hash = Mathf.Abs(hash);
                return hash % range;
            }
        }
    }
}
