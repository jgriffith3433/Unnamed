Shader "PWStorm/PolygonOverlay"
{
    Properties
    {
        _PolyColor ("Fill Colour (RGBA – A = Opacity)", Color) = (1,0.4,0,0.35)
    }

    SubShader
    {
        /*  Queue = Transparent+100     → stays in the Transparent bucket but
                                          sorts after most other transparents.
            RenderType = Transparent     → keeps lighting / batching happy.     */
        Tags { "RenderType"="Transparent"  "Queue"="Transparent+100" }
        LOD 100

        Pass
        {
            Blend SrcAlpha OneMinusSrcAlpha
            Cull  Off

            /*  --- THE 2 LINES THAT MAKE IT "ALWAYS-ON-TOP" --- */
            ZWrite Off          // don’t alter the depth buffer
            ZTest  Always       // never reject against existing depth

            HLSLPROGRAM
            #pragma target 4.5          // StructuredBuffer needs SM4.5+
            #pragma vertex   vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            StructuredBuffer<float3> _Points;   // world-space vertices
            uint                     _PointCount;
            float4                   _PolyColor;

            struct appdata { float3 vertex : POSITION; };
            struct v2f
            {
                float4 pos      : SV_POSITION;
                float3 worldPos : TEXCOORD0;
            };

            v2f vert (appdata v)
            {
                v2f o;
                o.pos      = UnityObjectToClipPos(v.vertex);
                o.worldPos = mul(unity_ObjectToWorld, float4(v.vertex,1)).xyz;
                return o;
            }

            // ---------- winding-number test on world X-Z ----------
            bool PointInPoly (float2 p)
            {
                int wn = 0;
                UNITY_LOOP
                for (uint i = 0; i < _PointCount; ++i)
                {
                    uint j = (i + 1 == _PointCount) ? 0u : i + 1u;
                    float2 a = _Points[i].xz;
                    float2 b = _Points[j].xz;

                    bool up   = a.y <= p.y && b.y >  p.y;
                    bool down = a.y >  p.y && b.y <= p.y;
                    float side = (p.x - a.x) * (b.y - a.y) -
                                 (p.y - a.y) * (b.x - a.x);

                    if (up   && side >  0) ++wn;
                    if (down && side < 0) --wn;
                }
                return wn != 0;
            }
            // ------------------------------------------------------

            fixed4 frag (v2f i) : SV_Target
            {
                if (!PointInPoly(i.worldPos.xz))
                    discard;

                return _PolyColor;
            }
            ENDHLSL
        }
    }
}
