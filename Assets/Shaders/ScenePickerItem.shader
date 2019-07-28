Shader "Unlit/ScenePickerItem"
{
    Properties
    {
       [Header(Base Parameters)]
        _Color ("Tint", Color) = (1, 1, 1, 1)
        _MainTex ("Texture", 2D) = "white" {}
        _OutlineWidth ("Outline Width", Range(0,0.1)) = 0.05
        _OutlineIntensity ("Outline Intensity", Range(0,1)) = 0.05
        _OutlineColor ("Outline Color", Color) = (1, 1, 1, 1)
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200

    CGPROGRAM
    #pragma surface surf Toon fullforwardshadows

    fixed4 _Color;
    float _OutlineIntensity;
    fixed4 _OutlineColor;
    float _OutlineWidth;
    
    struct Input {
        float2 uv_MainTex;
    };
    
    sampler2D _MainTex;
    
    void surf (Input IN, inout SurfaceOutput o) {
        o.Albedo = _Color.rgb * tex2D (_MainTex, IN.uv_MainTex).rgb;
        float maxOutline = 1.0 - _OutlineWidth;
        if(IN.uv_MainTex.x < _OutlineWidth ||IN.uv_MainTex.x > maxOutline || IN.uv_MainTex.y < _OutlineWidth || IN.uv_MainTex.y > maxOutline){
            o.Albedo = lerp(o.Albedo,_OutlineColor.rgb,_OutlineIntensity);
        }
    }
    
    half4 LightingToon (SurfaceOutput s, fixed3 viewDir, UnityGI gi) {
        half4 c;
        c.rgb = s.Albedo;
        c.a = 1.0;
        return c;
    }
    
    inline void LightingToon_GI(SurfaceOutput s, UnityGIInput data, inout UnityGI gi)
    {
    }
    ENDCG
    }
    FallBack "Diffuse"
}
