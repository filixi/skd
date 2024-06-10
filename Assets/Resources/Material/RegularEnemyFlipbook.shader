Shader "Unlit/RegularEnemyFlipbook"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "" {}

        _Clip ("Clip", Vector) = (0, 0, 0, 0)
        _Tint ("Tint", Vector) = (1, 1, 1, 1)
        _ProgressSpeed ("Progess speed", Float) = 1
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
            // UNITY_DECLARE_TEX2DARRAY(_MainTex);
            float4 _MainTex_ST;

            float4 _Clip;
            float4 _Tint;
            float _ProgressSpeed;

            fixed4 _Color;
            // x: col span
            // y: row span
            // z: atlas id
            // w: progress
            float4 _Data;

            // [0] (translate x, translate z, translate progress, white fade out)
            // [1] (splice progress, damage progress, swing coef, x_flip)
            // [2] (alpha fade out, 0, 0, 0)
            float4x4 _AnimationData_0;

            v2f vert (appdata v)
            {
                v2f o;

                float4x4 ad_0 = _AnimationData_0;
                float4 translation = float4(ad_0[0].x, 0, ad_0[0].y, 0) * ad_0[0].z;

                float4 worldPos = mul(unity_ObjectToWorld, v.vertex);
                worldPos += translation;
                v.vertex = mul(unity_WorldToObject, worldPos);

                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            float2 flipbook(float2 uv, float2 size, float progress, float x, float y){
	            progress = floor( fmod(progress, (size.x * size.y)) );
	            float2 frame = float2(fmod(progress, size.x), floor(progress/size.x));
	            float2 frame_size = float2(1, 1) / size;
	
	            frame.y = 1.0 - frame_size.y * (frame.y + y);
	            frame.x = frame_size.x * frame.x;
                frame += float2(uv.x * x, uv.y * y) / size;
	
	            return frame;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // sample the texture
                float4 data = _Data;
                float4x4 ad_0 = _AnimationData_0;

                float x = data.x;
                float y = data.y;
                float atlas_id = data.z;
                float atlas_progress = data.w;
                // x = 1.0f;
                // y = 1.0f;
                // p = 3.0f;

                float x_shift = 0;
                float y_shift = 0;

                // swing
                {
                    float swing = sin(_Time.y * 2) * i.uv.y * ad_0[1].z * 0.05;
                    x_shift += swing;
                }

                // splice, lr shift
                {
                    float splice_lr_shift_progress = smoothstep(0.5, 1, ad_0[1].x);
                    float r_shift_coef = step(0.5, i.uv.x);
                    float shift_coef = 0.05;
                    y_shift = (shift_coef * 2 * r_shift_coef - shift_coef) * splice_lr_shift_progress;
                }

                float2 adj_uv = float2(i.uv.x + x_shift, i.uv.y + y_shift);

                if (ad_0[1].w > 0)
                    adj_uv.x = 1 - adj_uv.x;

                // float4 col = UNITY_SAMPLE_TEX2DARRAY(
                //     _MainTex,
                //     float3(flipbook(adj_uv, _Clip.xy, atlas_progress * _ProgressSpeed, x, y), atlas_id));
                float4 col = tex2D(_MainTex,
                    flipbook(adj_uv, _Clip.xy, atlas_progress * _ProgressSpeed, x, y));

                if (adj_uv.x < 0 || adj_uv.x > 1 || adj_uv.y < 0 || adj_uv.y > 1)
                    col.a = 0;

                { // damage taken
                    float blink_progress =
                        smoothstep(0, 0.1, ad_0[1].y) *
                        (1 - smoothstep(0.1, 1, ad_0[1].y));
                    col = lerp(col, float4(.8, 0, 0, col.w), blink_progress);
                }
                
                { // splice, alpha fade out
                    float splice_bound =
                        smoothstep(0.45, 0.5, i.uv.x) *
                        (1 - smoothstep(0.5, 0.55, i.uv.x));
                    float splice_progress = smoothstep(0, 0.5, ad_0[1].x);
                    float alpha_progress = smoothstep(0.5, 1, ad_0[1].x);

                    col = lerp(col, float4(1, 1, 1, col.a), splice_bound * step(1 - i.uv.y, splice_progress));
                    col.a = lerp(col.a, 0, alpha_progress);
                }

                { // white fade out
                    col = lerp(col, float4(1, 1, 1, 0), ad_0[0].w);
                }

                { // alpha fade out
                    col = lerp(col, float4(col.xyz, 0), ad_0[2].x);
                }

                // apply fog
                // UNITY_APPLY_FOG(i.fogCoord, col);
                return col * _Tint;
            }
            ENDCG
        }
    }
}
