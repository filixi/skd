Shader "Unlit/BackgroundTiling"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "red" {}
        _Tint ("Tint", Vector) = (1, 1, 1, 1)
    }
    SubShader
    {
        Tags {"Queue"="Transparent" "RenderType"="Transparent"}
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma require 2darray

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
            };
            
            sampler2D _MainTex;
            float4 _MainTex_ST;

            float4 _Tint;

            v2f vert (appdata v)
            {
                v2f o;


                float4 worldPos = mul(unity_ObjectToWorld, v.vertex);
                v.vertex = mul(unity_WorldToObject, worldPos);

                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            float2 flipbook(float2 uv, float2 size, float progress){
                float2 frame = float2(fmod(progress, size.x), floor(progress/size.x));
                float2 frame_size = float2(1, 1) / size;
                return frame * frame_size + uv / size;
            }


            float4 frag (v2f i) : SV_Target
            {
                float2 tiling_uv = float2(i.uv.x * 15 % 1, i.uv.y * 15 % 1);
                
                float2 index = float2((i.uv.x - 0.5) * 15, (i.uv.y - 0.5) * 15);
                float shift = index.x * index.x + index.y * index.y;


                float middle1 = round(_Time.y * 15) + round(shift);
                float middle2 = round(_Time.y * 15) + round(shift + 0.5);

                // sample the texture
                float4 col1 = tex2D(_MainTex, flipbook(tiling_uv, float2(13, 1), middle1 % 13));
                float4 col2 = tex2D(_MainTex, flipbook(tiling_uv, float2(13, 1), middle2 % 13));

                return lerp(col2, col1, frac(shift));
                
                // return col * _Tint;
            }
            ENDCG
        }
    }
}
