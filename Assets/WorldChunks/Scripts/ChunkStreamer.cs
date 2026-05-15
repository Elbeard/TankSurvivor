using System.Collections.Generic;
using UnityEngine;

/// <summary>
    /// Главный компонент стриминга: следит за игроком, спавнит и убирает чанки.
    /// Повесьте на объект с дочерним <c>ChunkRoot</c> (см. prefab WorldChunkSystem).
    /// </summary>
    [DisallowMultipleComponent]
    public class ChunkStreamer : MonoBehaviour
    {
        [Header("Ссылки")]
        [SerializeField] private ChunkSettings _settings;
        [SerializeField] private Transform _chunkRoot;
        [SerializeField] private Transform _player;
        [SerializeField] private GameObject[] _chunkPrefabs;

        [Header("Автопоиск")]
        [Tooltip("Если Player не назначен — ищем объект с компонентом Player.")]
        [SerializeField] private bool _autoFindPlayer = true;

        private readonly Dictionary<Vector2Int, ActiveChunkEntry> _active = new();

        private IChunkProvider _provider;
        private ChunkPool _pool;
        private Rigidbody2D _playerBody;
        private Vector2 _lastMoveDirection = Vector2.right;
        private float _updateTimer;

        private struct ActiveChunkEntry
        {
            public WorldChunk Chunk;
            public int VariantIndex;
        }

        private void Awake()
        {
            if (_settings == null)
                Debug.LogError("[ChunkStreamer] Не назначен Chunk Settings.", this);

            if (_chunkRoot == null)
            {
                var rootGo = new GameObject("ChunkRoot");
                rootGo.transform.SetParent(transform, false);
                _chunkRoot = rootGo.transform;
            }

            if (_autoFindPlayer && _player == null)
            {
                Player player = FindObjectOfType<Player>();
                if (player != null)
                    _player = player.transform;
                else
                    Debug.LogWarning("[ChunkStreamer] Player не найден — чанки не обновятся.", this);
            }

            if (_player != null)
                _playerBody = _player.GetComponent<Rigidbody2D>();

            if (_chunkPrefabs == null || _chunkPrefabs.Length == 0)
                _chunkPrefabs = WorldChunkRuntimeFactory.CreateDefaultVariantPrefabs(_settings != null ? _settings.chunkSize : 4800f);

            _provider = new PrefabChunkProvider(_chunkPrefabs);
            _pool = new ChunkPool(_chunkRoot);

            // Первый кадр — сразу заполнить сетку
            _updateTimer = 0f;
            RefreshChunks();
        }

        private void Update()
        {
            if (_settings == null || _player == null)
                return;

            float interval = _settings.updateInterval;
            if (interval <= 0f)
            {
                RefreshChunks();
                return;
            }

            _updateTimer -= Time.deltaTime;
            if (_updateTimer <= 0f)
            {
                _updateTimer = interval;
                RefreshChunks();
            }
        }

        private void OnDestroy()
        {
            _pool?.ClearAll();
        }

        /// <summary>Принудительно пересоздать все активные чанки (например, смена visual tier).</summary>
        public void ReloadAllChunks()
        {
            var coords = new List<Vector2Int>(_active.Keys);
            foreach (Vector2Int coord in coords)
                DespawnChunk(coord);

            RefreshChunks();
        }

        /// <summary>Текущие активные логические координаты (для спавна врагов с краёв).</summary>
        public IReadOnlyCollection<Vector2Int> GetActiveCoords()
        {
            return _active.Keys;
        }

        private void RefreshChunks()
        {
            Vector2 playerPos = _player.position;
            Vector2Int playerChunk = ChunkCoordUtil.WorldToChunk(playerPos, _settings.chunkSize);

            Vector2 moveDir = GetMoveDirection();
            float speed = _playerBody != null ? _playerBody.velocity.magnitude : 0f;

            HashSet<Vector2Int> required = ChunkRequiredSetBuilder.Build(
                playerChunk, moveDir, speed, _settings);

            foreach (Vector2Int coord in required)
            {
                if (_active.ContainsKey(coord))
                    continue;

                SpawnChunk(coord);
            }

            var toRemove = new List<Vector2Int>();
            foreach (KeyValuePair<Vector2Int, ActiveChunkEntry> kv in _active)
            {
                if (required.Contains(kv.Key))
                    continue;

                int dist = ChunkCoordUtil.ManhattanDistance(kv.Key, playerChunk);
                if (dist > _settings.despawnDistance)
                    toRemove.Add(kv.Key);
            }

            foreach (Vector2Int coord in toRemove)
                DespawnChunk(coord);
        }

        private void SpawnChunk(Vector2Int coord)
        {
            GameObject prefab = _provider.GetPrefabForChunk(coord, _settings.worldSeed, out int variantIndex);
            if (prefab == null)
            {
                Debug.LogWarning("[ChunkStreamer] Нет префабов чанков.", this);
                return;
            }

            Vector3 origin = ChunkCoordUtil.ChunkToWorldOrigin(coord, _settings.chunkSize);
            WorldChunk instance = _pool.Get(prefab, variantIndex, origin);
            instance.Initialize(coord, variantIndex);

            _active[coord] = new ActiveChunkEntry
            {
                Chunk = instance,
                VariantIndex = variantIndex
            };
        }

        private void DespawnChunk(Vector2Int coord)
        {
            if (!_active.TryGetValue(coord, out ActiveChunkEntry entry))
                return;

            _active.Remove(coord);
            _pool.Release(entry.Chunk, entry.VariantIndex);
        }

        private Vector2 GetMoveDirection()
        {
            if (_playerBody != null && _playerBody.velocity.sqrMagnitude > 0.01f)
            {
                _lastMoveDirection = _playerBody.velocity.normalized;
                return _lastMoveDirection;
            }

            return _lastMoveDirection;
        }

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            if (_settings == null || _player == null)
                return;

            Vector2Int pc = ChunkCoordUtil.WorldToChunk(_player.position, _settings.chunkSize);
            Vector3 origin = ChunkCoordUtil.ChunkToWorldOrigin(pc, _settings.chunkSize);
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireCube(
                origin + new Vector3(_settings.chunkSize * 0.5f, _settings.chunkSize * 0.5f, 0f),
                new Vector3(_settings.chunkSize, _settings.chunkSize, 0.1f));
        }
#endif
}
