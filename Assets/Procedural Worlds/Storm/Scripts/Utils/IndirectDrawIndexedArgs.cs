namespace ProceduralWorlds.Storm.Utils
{
    //
    // Summary:
    //     Defines the data layout for indexed indirect render calls.
    public struct IndirectDrawIndexedArgs
    {
        //
        // Summary:
        //     The size of the struct in bytes.
        public const int size = 20;

        //
        // Summary:
        //     The number of vertex indices per instance.
        public uint indexCountPerInstance { readonly get; set; }
        //
        // Summary:
        //     The number of instances to render.
        public uint instanceCount { readonly get; set; }
        //
        // Summary:
        //     The first index to use in the index buffer of the rendering call.
        public uint startIndex { readonly get; set; }
        //
        // Summary:
        //     The index to add to values fetched from the index buffer.
        public uint baseVertexIndex { readonly get; set; }
        //
        // Summary:
        //     The first instance to render.
        public uint startInstance { readonly get; set; }
    }
}
