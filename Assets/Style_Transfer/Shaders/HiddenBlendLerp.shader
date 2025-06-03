Shader "Hidden/BlendLerp"
{
    Properties
    {
        _MainTex ("Base (RGB)", 2D) = "white" {}
        _BlendTex ("Blend Tex", 2D) = "black" {}
        _Alpha ("Blend Factor", Range(0,1)) = 0.5
    }
    SubShader
    {
        Pass
        {
            ZTest Always Cull Off ZWrite Off

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            sampler2D _MainTex;
            sampler2D _BlendTex;
            float _Alpha;

            struct appdata {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            v2f vert (appdata v) {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target {
                fixed4 current = tex2D(_MainTex, i.uv);
                fixed4 previous = tex2D(_BlendTex, i.uv);
                return lerp(previous, current, _Alpha);
            }
            ENDCG
        }
    }
}
