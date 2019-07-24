Shader "Unlit/ObjectOutline"
{
    Properties
    {
        _MainColor ("Color", Color) = (0,0,0,1)
        _Thickness ("Thickness", float) = 1
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            #pragma multi_compile_fog

            #include "UnityCG.cginc"

            struct v2f
            {
                float2 uv : TEXCOORD0;
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
            };

            fixed4 _MainColor;
            float _Thickness;
            

            v2f vert (appdata_full v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                UNITY_TRANSFER_FOG(o,o.vertex);
                
                o.vertex.xyz += v.normal * _Thickness;
                v.normal.xyz *= -1;
                
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                return _MainColor;
            }
            ENDCG
        }
    }
}
