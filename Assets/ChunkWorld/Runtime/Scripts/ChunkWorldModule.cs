namespace ChunkWorld
{
    /// <summary>
    /// Точка входа модуля ChunkWorld. Пути к контенту — для Editor и будущей загрузки текстур.
    /// </summary>
    public static class ChunkWorldModule
    {
        public const string ModuleRoot = "Assets/ChunkWorld";
        public const string SettingsPath = ModuleRoot + "/Content/Settings";
        public const string TexturesPath = ModuleRoot + "/Content/Textures";
        public const string ChunkPrefabsPath = ModuleRoot + "/Content/Prefabs";
        public const string DecorPrefabsPath = ModuleRoot + "/Content/DecorPrefabs";
        public const string DefaultBiomeCatalogPath = SettingsPath + "/DefaultBiomeTextures.asset";
        public const string DefaultConfigPath = SettingsPath + "/DefaultChunkWorldConfig.asset";
    }
}
