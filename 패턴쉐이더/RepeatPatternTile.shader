

 /// <summary>
 /// 2023 사천성 게임
 /// 패턴쉐이더
 /// </summary>

Shader "Custom/RepeatPatternTile" {
    Properties {
        _MainTex ("Texture", 2D) = "black" {}
        _TileX ("Tile X", Range(1, 10)) = 2
        _TileY ("Tile Y", Range(1, 10)) = 2
        _Color ("Color", Color) = (1, 1, 1, 1)
    }
    
    SubShader {
        Tags { "Queue" = "Overlay" }
        
        Pass {
            CGPROGRAM
            #pragma vertex vert
            #pragma exclude_renderers gles xbox360 ps3
            #pragma fragment frag
            #include "UnityCG.cginc"
            
            struct appdata {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };
            
            struct v2f {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };
            
            sampler2D _MainTex;
            float _TileX;
            float _TileY;
            fixed4 _Color;
            
            v2f vert (appdata v) {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv * float2(_TileX, _TileY);
                return o;
            }
            
              fixed4 frag (v2f i) : SV_Target {
                float2 tileUV = frac(i.uv);
                fixed4 texColor = tex2D(_MainTex, tileUV);
                fixed4 col = texColor * _Color;  // Color를 텍스처에 곱함
                col.a *= texColor.a * _Color.a;  // 텍스처 알파와 Color 알파를 모두 곱함
                return col;
            }
            ENDCG
        }
    }
    
    // Diffuse 대신 Transparent/Diffuse로 FallBack 설정
    FallBack "Transparent/Diffuse"
}