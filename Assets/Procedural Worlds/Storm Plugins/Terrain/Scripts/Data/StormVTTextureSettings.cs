namespace ProceduralWorlds.Storm.Terrain
{
    public class StormVTTextureSettings 
    {
        /// <summary>
        /// Shortened list of avaliable texture compression formats for the physical textures
        /// Can only be a select few, as not all compression formats support RGBA textures
        /// </summary>
        public enum StormCompressionFormat
        {
            RGBA32 = 4,
            DXT5 = 12,
            //ASTC_4x4 = 48,
        }

    }
}


