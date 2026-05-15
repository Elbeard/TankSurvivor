using UnityEngine;

/// <summary>
    /// Настройки стриминга чанков. Создайте asset через
    /// <b>Create → World Chunks → Chunk Settings</b> и назначьте в <see cref="ChunkStreamer"/>.
    /// </summary>
    [CreateAssetMenu(fileName = "ChunkSettings", menuName = "World Chunks/Chunk Settings")]
    public class ChunkSettings : ScriptableObject
    {
        [Header("Размер чанка (мировые единицы)")]
        [Tooltip("Сторона квадратного чанка. Мировая позиция угла: (chunkX * chunkSize, chunkY * chunkSize).")]
        [Min(1f)]
        public float chunkSize = 4800f;

        [Header("Сид мира")]
        [Tooltip("Один seed на забег. Вместе с coord даёт один и тот же вариант префаба при повторном заходе.")]
        public int worldSeed = 12345;

        [Header("Буфер по направлению движения")]
        [Tooltip("Сколько чанков держать впереди по velocity.")]
        [Min(0)]
        public int chunksAhead = 2;

        [Tooltip("Сколько чанков держать сзади (чтобы при развороте земля ещё была в сцене).")]
        [Min(0)]
        public int chunksBehind = 2;

        [Tooltip("Полоса чанков слева/справа от оси движения.")]
        [Min(0)]
        public int chunksSide = 1;

        [Header("Удаление")]
        [Tooltip("Despawn, если Manhattan-расстояние от игрока больше этого значения и coord не в required set.")]
        [Min(1)]
        public int despawnDistance = 4;

        [Header("Поведение при остановке")]
        [Tooltip("Если скорость ниже — загружается симметричный квадрат idleRadius вокруг игрока.")]
        [Min(0.01f)]
        public float minMoveSpeedForDirectedChunks = 0.5f;

        [Tooltip("Радиус квадрата чанков вокруг игрока, когда танк стоит.")]
        [Min(0)]
        public int idleRadius = 1;

        [Header("Обновление")]
        [Tooltip("Интервал пересчёта набора чанков (сек). 0 = каждый кадр.")]
        [Min(0f)]
        public float updateInterval = 0.15f;
    }
