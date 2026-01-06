Shader "Custom/BlinkOnHit"
{
    Properties
    {
        _MainTex ("Main Texture", 2D) = "white" {}
        _BlinkTex ("Blink Texture", 2D) = "white" {}
        _BlinkColor ("Blink Color", Color) = (1, 1, 1, 1)
        _BlinkAmount ("Blink Amount", Range(0, 1)) = 0
    }

    SubShader
    {
        Tags { "RenderType" = "Opaque" }
        LOD 200

        CGPROGRAM
        #pragma surface surf Standard fullforwardshadows
        #pragma target 3.0

        sampler2D _MainTex;
        sampler2D _BlinkTex;
        fixed4 _BlinkColor;
        float _BlinkAmount;

        struct Input
        {
            float2 uv_MainTex;
            float2 uv_BlinkTex;
        };

        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            fixed4 main = tex2D(_MainTex, IN.uv_MainTex);
            fixed4 blink = tex2D(_BlinkTex, IN.uv_BlinkTex) * _BlinkColor;
            fixed4 finalColor = lerp(main, blink, _BlinkAmount);

            o.Albedo = finalColor.rgb;
            o.Alpha = finalColor.a;
        }
        ENDCG
    }

    FallBack "Diffuse"
}
