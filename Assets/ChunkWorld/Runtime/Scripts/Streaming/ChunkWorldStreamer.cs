using System.Collections.Generic;
using UnityEngine;

namespace ChunkWorld
{
    /// <summary>
    /// Стриминг чанков вокруг цели следования. Текстуры пола — из <see cref="ChunkWorldBiomeTextureCatalog"/>.
    /// </summary>
    [DisallowMultipleComponent]
    public class ChunkWorldStreamer : MonoBehaviour
    {
        [Header("Настройки модуля")]
        [SerializeField] private ChunkWorldConfig _config;

        [SerializeField] private Transform _chunkRoot;

        [Header("Следование")]
        [SerializeField] private Transform _followTarget;

        [Tooltip("Если цель не назначена — ищем GameObject с именем Player.")]
        [SerializeField] private bool _autoFindPlayerByName = true;

        private readonly Dictionary<Vector2Int, ActiveChunkEntry> _active = new();

        private ChunkWorldChunkPool _pool;
        private Rigidbody2D _followBody;
        private Vector2 _lastMoveDirection = Vector2.right;
        private float _updateTimer;
        private float _effectiveChunkSize;

        private struct ActiveChunkEntry
        {
            public ChunkWorldChunk Chunk;
            public int VariantIndex;
        }

        /// <summary>Назначить цель (вызывается из игры, например ChunkWorldSceneBootstrap).</summary>
        public void SetFollowTarget(Transform target)
        {
            _followTarget = target;
            _followBody = target != null ? target.GetComponent<Rigidbody2D>() : null;
        }

        private void Awake()
        {
            if (_config == null)
                Debug.LogError("[ChunkWorld] Не назначен ChunkWorldConfig.", this);

            if (_config != null && _config.biomeTextures != null
                && _config.biomeTextures.HasMissingSprites(out string msg))
                Debug.LogWarning($"[ChunkWorld] {msg}", this);

            if (_chunkRoot == null)
            {
                var rootGo = new GameObject("ChunkRoot");
                rootGo.transform.SetParent(transform, false);
                _chunkRoot = rootGo.transform;
            }

            if (_followTarget == null && _autoFindPlayerByName)
            {
                GameObject playerGo = GameObject.Find("Player");
                if (playerGo != null)
                    SetFollowTarget(playerGo.transform);
                else
                    Debug.LogWarning("[ChunkWorld] Объект Player не найден — чанки не обновятся.", this);
            }
            else if (_followTarget != null)
                _followBody = _followTarget.GetComponent<Rigidbody2D>();

            _effectiveChunkSize = _config.GetEffectiveChunkSize();
            _pool = new ChunkWorldChunkPool(_chunkRoot);
            _updateTimer = 0f;
            RefreshChunks();
        }

        private void Update()
        {
            if (_config == null || _followTarget == null)
                return;

            float interval = _config.updateInterval;
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

        public void ReloadAllChunks()
        {
            var coords = new List<Vector2Int>(_active.Keys);
            foreach (Vector2Int coord in coords)
                DespawnChunk(coord);

            RefreshChunks();
        }

        public IReadOnlyCollection<Vector2Int> GetActiveCoords()
        {
            return _active.Keys;
        }

        private void RefreshChunks()
        {
            Vector2 pos = _followTarget.position;
            Vector2Int playerChunk = ChunkWorldCoordUtil.WorldToChunk(pos, _effectiveChunkSize);

            Vector2 moveDir = GetMoveDirection();
            float speed = _followBody != null ? _followBody.velocity.magnitude : 0f;

            HashSet<Vector2Int> required = ChunkWorldRequiredSetBuilder.Build(
                playerChunk, moveDir, speed, _config);

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

                int dist = ChunkWorldCoordUtil.ManhattanDistance(kv.Key, playerChunk);
                if (dist > _config.despawnDistance)
                    toRemove.Add(kv.Key);
            }

            foreach (Vector2Int coord in toRemove)
                DespawnChunk(coord);
        }

        private void SpawnChunk(Vector2Int coord)
        {
            ChunkWorldBiome biome = ChunkWorldBiomeUtil.GetBiomeForChunk(coord, _config.worldSeed, _config);
            int variantIndex = ChunkWorldBiomeUtil.BiomeToVariantIndex(biome);
            Vector3 origin = ChunkWorldCoordUtil.ChunkToWorldOrigin(coord, _effectiveChunkSize);

            ChunkWorldChunk instance = _pool.Get(
                coord,
                biome,
                origin,
                _effectiveChunkSize,
                _config.biomeTextures,
                _config.groundLayout);

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
            if (_followBody != null && _followBody.velocity.sqrMagnitude > 0.01f)
            {
                _lastMoveDirection = _followBody.velocity.normalized;
                return _lastMoveDirection;
            }

            return _lastMoveDirection;
        }

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            if (_config == null || _followTarget == null)
                return;

            Vector2Int pc = ChunkWorldCoordUtil.WorldToChunk(_followTarget.position, _effectiveChunkSize);
            Vector3 origin = ChunkWorldCoordUtil.ChunkToWorldOrigin(pc, _effectiveChunkSize);
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireCube(
                origin + new Vector3(_effectiveChunkSize * 0.5f, _effectiveChunkSize * 0.5f, 0f),
                new Vector3(_effectiveChunkSize, _effectiveChunkSize, 0.1f));
        }
#endif
    }
}
