using UnityEngine;

namespace ChunkWorld
{
    /// <summary>
    /// Собирает визуал чанка: пол из <see cref="ChunkWorldBiomeTextureCatalog"/>.
    /// </summary>
    public static class ChunkWorldChunkBuilder
    {
        private const string GroundName = "Ground";
        private const string QuadResourcePath = "ChunkQuad";
        private static Sprite _fallbackQuad;

        public static ChunkWorldChunk CreateChunkRoot()
        {
            var root = new GameObject("ChunkWorldChunk");
            root.AddComponent<ChunkWorldChunk>();
            return root.GetComponent<ChunkWorldChunk>();
        }

        public static void RebuildGround(
            Transform root,
            ChunkWorldBiome biome,
            float chunkSize,
            ChunkWorldBiomeTextureCatalog catalog,
            ChunkGroundLayout layout = ChunkGroundLayout.FitChunk)
        {
            if (root == null)
                return;

            Transform ground = root.Find(GroundName);
            if (ground == null)
            {
                var groundGo = new GameObject(GroundName);
                ground = groundGo.transform;
                ground.SetParent(root, false);
            }

            Sprite sprite = catalog != null ? catalog.GetGroundSprite(biome) : null;
            if (sprite == null)
                sprite = GetFallbackQuad();

            ground.localPosition = new Vector3(chunkSize * 0.5f, chunkSize * 0.5f, 0f);
            ground.localRotation = Quaternion.identity;

            var sr = ground.GetComponent<SpriteRenderer>();
            if (sr == null)
                sr = ground.gameObject.AddComponent<SpriteRenderer>();

            sr.sprite = sprite;
            sr.color = Color.white;
            sr.sortingOrder = -100;

            ApplyGroundLayout(ground, sr, chunkSize, layout);
        }

        /// <summary>
        /// Масштаб так, чтобы спрайт занял ровно chunkSize × chunkSize (не chunkSize как множитель поверх PPU).
        /// </summary>
        private static void ApplyGroundLayout(Transform ground, SpriteRenderer sr, float chunkSize, ChunkGroundLayout layout)
        {
            if (layout == ChunkGroundLayout.Tile)
            {
                sr.drawMode = SpriteDrawMode.Tiled;
                sr.size = new Vector2(chunkSize, chunkSize);
                ground.localScale = Vector3.one;
                return;
            }

            sr.drawMode = SpriteDrawMode.Simple;
            Vector2 native = sr.sprite != null ? sr.sprite.bounds.size : Vector2.one;
            float w = Mathf.Max(native.x, 0.0001f);
            float h = Mathf.Max(native.y, 0.0001f);
            ground.localScale = new Vector3(chunkSize / w, chunkSize / h, 1f);
        }

        private static Sprite GetFallbackQuad()
        {
            if (_fallbackQuad != null)
                return _fallbackQuad;

            _fallbackQuad = Resources.Load<Sprite>(QuadResourcePath);
            if (_fallbackQuad != null)
                return _fallbackQuad;

            var tex = new Texture2D(4, 4, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Point;
            var pixels = new Color[16];
            for (int i = 0; i < pixels.Length; i++)
                pixels[i] = Color.white;
            tex.SetPixels(pixels);
            tex.Apply();
            _fallbackQuad = Sprite.Create(tex, new Rect(0, 0, 4, 4), new Vector2(0.5f, 0.5f), 4f);
            return _fallbackQuad;
        }
    }
}
