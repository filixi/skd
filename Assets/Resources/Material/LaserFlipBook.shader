Shader "Unlit/LaserFlipBook"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "red" {}
        _Tint ("Tint", Vector) = (1, 1, 1, 1)
        _FrameCount ("FrameCount", float) = 3
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
            float _FrameCount;

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

            fixed4 frag (v2f i) : SV_Target
            {
                // sample the texture
                float4 col = tex2D(_MainTex, flipbook(float2(i.uv.x, i.uv.y), float2(_FrameCount, 1), (int)round(_Time.y * 20) % _FrameCount));
                
                return col * _Tint;
            }
            ENDCG
        }
    }
}
