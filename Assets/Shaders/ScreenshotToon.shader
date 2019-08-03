Shader "Unlit/ScreenshotToon"
{
    Properties
    {
       [Header(Base Parameters)]
        _Color ("Tint", Color) = (1, 1, 1, 1)
        _AmbientLightColor ("Ambient Light Color", Color) = (1, 1, 1, 1)
        _RimColor ("Rim Color", Color) = (1, 1, 1, 1)
        _RimAmount ("Rim Amount", Range(0, 2)) = 1.2
        _FillAmount ("Fill Amount", Range(-1, 1)) = 0
        _FillColor ("Fill Color", Color) = (1, 1, 1, 1)
        _FresnelColor ("Fresnel Color", Color) = (1, 1, 1, 1)
        _FresnelPower ("Fresnel Power", Range(1, 10)) = 0
        _NoiseTex ("Glitter Noise", 2D) = "white" {}
        _MainTex ("Main Noise", 2D) = "white" {}
    }
    SubShader
    {
        Tags { "Queue" = "Transparent" "RenderType"="Transparent" }
        LOD 200
        Blend SrcAlpha OneMinusSrcAlpha 

    CGPROGRAM
    #pragma surface surf Toon fullforwardshadows vertex:vert alpha:blend
    #include "fbm.cginc"

    float _FillAmount;
    float _RimAmount;
    fixed4 _RimColor;
    fixed4 _Color;
    fixed4 _AmbientLightColor;
    
    float _GlitterPercent;
    float _RainbowPercent;
    float _ColorPercent;
    float _PoisonPercent;
    int _octaves;
    float _Size;
    
    sampler2D _NoiseTex; 
    
    struct Input {
        float2 uv_MainTex;
        float3 vPos;
        float empty;
    };
    
    void vert (inout appdata_full v, out Input o) {
        UNITY_INITIALIZE_OUTPUT(Input,o);
    }
        
    void surf (Input IN, inout SurfaceOutput o) {
    }
    
    half4 LightingToon (SurfaceOutput s, fixed3 viewDir, UnityGI gi) {
         ?? lighting ??
        half NdotL = dot (s.Normal, gi.light.dir);
        float lightIntensity = smoothstep(0, 0.05, NdotL);
        float3 lightColor = lightIntensity * gi.light.color;
        
        float4 rimDot = 1 - dot(viewDir, s.Normal);
        float rimIntensity = rimDot * _RimAmount * pow( NdotL, 3.0);

        rimIntensity = smoothstep(_RimAmount - 0.01, _RimAmount + 0.01, rimIntensity);
        float4 rim = rimIntensity * _RimColor;

        half4 c;
        c.rgb = s.Albedo * (lightColor + (1-lightIntensity)*_AmbientLightColor + rim.rgb);
        c.a = s.Alpha;
        return c;
    }
    
    inline void LightingToon_GI(SurfaceOutput s, UnityGIInput data, inout UnityGI gi)
    {
    }
    ENDCG
    
    Blend SrcAlpha OneMinusSrcAlpha 

    CGPROGRAM

    #pragma surface surf SimpleFresnel fullforwardshadows vertex:vert alpha:blend
        
        fixed4 _FresnelColor;
        float _FresnelPower;
        fixed4 _Color;
        struct Input {
            float2 uv_MainTex;
        };
        
        void vert (inout appdata_full v, out Input o) {
            UNITY_INITIALIZE_OUTPUT(Input,o);
            v.vertex.xyz += 0.01 * v.normal.xyz;
        }
           
        void surf (Input IN, inout SurfaceOutput o) {
        }
        
        half4 LightingSimpleFresnel (SurfaceOutput s, fixed3 viewDir, UnityGI gi) {
            float vDot = dot(viewDir, s.Normal);
            float fresnelIntensity = _FresnelPower * pow(saturate(1-vDot), 1);
            float3 fresnelColor = fresnelIntensity * (0.6*_FresnelColor + 0.4*_Color);

            half4 c;
            c.rgb = fresnelColor;
            c.a = 0.1;
            return c;
        }
        
        inline void LightingSimpleFresnel_GI(SurfaceOutput s, UnityGIInput data, inout UnityGI gi)
        {
        }
        ENDCG

    }
    
    FallBack "Diffuse"
}

