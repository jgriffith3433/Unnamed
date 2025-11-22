namespace ProceduralWorlds.Storm.Terrain
{
    public class StormVTEnums
    {
        /// <summary>
        /// Resolution of the page tiles
        /// </summary>
        public enum TileSize
        {
            _32 = 32,
            _64 = 64,
            _128 = 128,
            _256 = 256,
            _512 = 512,
            _1024 = 1024
        };

        /// <summary>
        /// Number of tiles, in power of 2. Used for row and column counts in the physical texture
        /// </summary>
        public enum TileCount
        {
            _1 = 1,
            _2 = 2,
            _4 = 4,
            _8 = 8,
            _16 = 16,
            _32 = 32,
            _64 = 64,
            _128 = 128,
            _256 = 256,
            _512 = 512
        };

        /// <summary>
        /// Number of pixels to pad each side of the page tile with.
        /// Used for fixing bilinear sampling artifacts from neighbouring tiles
        /// </summary>
        public enum Padding
        {
            _0 = 0,
            _1 = 1,
            _2 = 2,
            _4 = 4,
            _8 = 8
        };

        /// <summary>
        /// Resolution of the feedback renderer, relative to screen size
        /// </summary>
        public enum FeedbackResolution
        {
            _One = 1,
            _Half = 2,
            _Quarter = 4,
            _Eighth = 8,
            _Sixteenth = 16
        }

        /// <summary>
        /// Option to gather page tiles
        /// </summary>
        public enum LoadOption
        {
            File,
            Runtime,
            Procedural,
            TextureArray
        }

        /// <summary>
        /// Anisotrophic filtering quality on textures
        /// </summary>
        public enum AnisoLevel
        {
            _0 = 0,
            _2x = 2,
            _4x = 4,
            _8x = 8,
            _16x = 16
        }

        /// <summary>
        /// Texture array compression format
        /// </summary>
        public enum StormCompressionFormat
        {
            ARGB4444 = 2,
            RGBA32 = 4,
            ARGB32 = 5,
            DXT5 = 12,
            RGBA4444 = 13,
            BC7 = 25,
            PVRTC_RGBA2 = 31,
            PVRTC_RGBA4 = 33,
            ETC2_RGBA8 = 47,
            ASTC_4x4 = 48,
            ASTC_5x5 = 49,
            ASTC_6x6 = 50,
            ASTC_8x8 = 51,
            ASTC_10x10 = 52,
            ASTC_12x12 = 53
        }

        /// <summary>
        /// Pipeline the project is currently in
        /// </summary>
        public enum StormVTPipeline
        {
            HDRP,
            URP,
            BuiltIn
        }

        /// <summary>
        /// Platform options for quality managing
        /// </summary>
        public enum StormVTPlatform
        {
            PC,
            Low_End_Mobile_VR,
            High_End_Mobile_VR
        }

        /// <summary>
        /// Storm Terrain mode
        /// Unity Terrain uses a material on each terrain
        /// Clipmap replaces Unity Terrain with a custom mesh solution, using indirect instancing
        /// </summary>
        public enum StormVTMode
        {
            UnityTerrain,
            Clipmap
        }

        /// <summary>
        /// Resolution of the clipmap's vertices. 
        /// The higher the more quality of the tessellation, but will increase cost
        /// </summary>
        public enum StormVTClipmapDensity
        {
            _1_Vertex_Per_Meter = 1,
            _2_Vertices_Per_Meter = 2,
            _4_Vertices_Per_Meter = 4,
            _8_Vertices_Per_Meter = 8,
            _16_Vertices_Per_Meter = 16,
            _32_Vertices_Per_Meter = 32,
            _64_Vertices_Per_Meter = 64,
            _128_Vertices_Per_Meter = 128,
            _256_Vertices_Per_Meter = 256
        }

        /// <summary>
        /// Resolution of the page table array. 
        /// 64 can hold 7 unique zoom levels, used for high quality devices
        /// 32 can hold 6 unique zoom levels, used for mid range devices, like VR
        /// 16 can hold 5 unique zoom levels, used for low end mobile devices
        /// </summary>
        public enum StormVTPageTableResolution
        {
            _64 = 64,
            _32 = 32,
            _16 = 16
        }

        /// <summary>
        /// Category of physical textures to debug with the Storm Texture Viewer prefab
        /// </summary>
        public enum StormVTRawImageDebugOption
        {
            _Height,
            _Normal,
            _ColorSource0,
            _ColorSource1,
            _Feedback
        }

        /// <summary>
        /// Used for telling how to generate snow
        /// Runtime will generate snow within the terrain shader, after tiles have been rendererd
        /// Tile will generate snow as part of the tiles
        /// </summary>
        public enum StormVTFeatureMode
        {
            _Runtime,
            _Tile
        }

        /// <summary>
        /// Terrain source data types. Used for terrain texture manager
        /// </summary>
        public enum StormVTTerrainSourceData
        {
            _Height = 0,
            _Normal = 1,
            _Splat = 2,
            _Hole = 3
        }

        /// <summary>
        /// Resolution of the terrain collision that is generated
        /// </summary>
        public enum StormVTCollisionResolution
        {
            _Full = 1,
            _Half = 2,
            _Quarter = 4,
            _Eighth = 8,
            _Sixteenth = 16,
        }

        /// <summary>
        /// Maxmimum size that the texture arrays are allowed to be.
        /// Used for managing memory
        /// </summary>
        public enum TextureArrayMaxSize
        {
            _64 = 64,
            _128 = 128,
            _256 = 256,
            _512 = 512,
            _1024 = 1024,
            _2048 = 2048,
            _4096 = 4096
        };

        /// <summary>
        /// Type of decal
        /// Decal - mimicks the behaviour of a Decal Projector
        /// Mesh - draws a mesh directly on the terrain
        /// Road - similiar to Mesh type, but adds blending functionaltiy for use with GeNa roads
        /// </summary>
        public enum StormVTDecalType
        {
            _Decal = 0,
            _Mesh = 1,
            _Road = 2
        }
    }
}
