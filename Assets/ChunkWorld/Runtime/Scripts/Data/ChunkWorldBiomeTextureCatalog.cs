using System;
using UnityEngine;

namespace ChunkWorld
{
    /// <summary>
    /// Одна запись: биом → спрайт пола чанка (назначается в Inspector).
    /// </summary>
    [Serializable]
    public struct ChunkWorldBiomeTextureEntry
    {
        public ChunkWorldBiome biome;
        public Sprite groundSprite;
    }

    /// <summary>
    /// Реестр текстур биомов. Меняйте спрайты вручную без правки кода.
    /// </summary>
    [CreateAssetMenu(
        fileName = "BiomeTextureCatalog",
        menuName = "ChunkWorld/Biome Texture Catalog",
        order = 0)]
    public class ChunkWorldBiomeTextureCatalog : ScriptableObject
    {
        [Tooltip("По одному спрайту на биом. Пустые поля — модуль может подставить запасной цвет.")]
        public ChunkWorldBiomeTextureEntry[] entries =
        {
            new() { biome = ChunkWorldBiome.Grass },
            new() { biome = ChunkWorldBiome.Sand },
            new() { biome = ChunkWorldBiome.Stone },
            new() { biome = ChunkWorldBiome.Taiga }
        };

        /// <summary>Возвращает спрайт пола для биома или null.</summary>
        public Sprite GetGroundSprite(ChunkWorldBiome biome)
        {
            if (entries == null)
                return null;

            for (int i = 0; i < entries.Length; i++)
            {
                if (entries[i].biome == biome)
                    return entries[i].groundSprite;
            }

            return null;
        }

        /// <summary>Проверка перед Play: все ли биомы заполнены.</summary>
        public bool HasMissingSprites(out string message)
        {
            message = null;
            if (entries == null || entries.Length == 0)
            {
                message = "Массив entries пуст.";
                return true;
            }

            foreach (ChunkWorldBiome biome in Enum.GetValues(typeof(ChunkWorldBiome)))
            {
                if (GetGroundSprite(biome) == null)
                {
                    message = $"Не назначен спрайт для биома {biome}.";
                    return true;
                }
            }

            return false;
        }
    }
}
