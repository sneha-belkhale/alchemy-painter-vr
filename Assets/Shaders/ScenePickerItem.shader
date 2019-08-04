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
        _Highlight ("Highlight Amount", Range(0,3)) = 0
    }
    SubShader
    {
        Tags { "Queue" = "Transparent" "RenderType"="Transparent" }
        LOD 200
        Blend SrcAlpha OneMinusSrcAlpha 

    CGPROGRAM
    #pragma surface surf Toon fullforwardshadows vertex:vert alpha:blend

    fixed4 _Color;
    float _OutlineIntensity;
    fixed4 _OutlineColor;
    float _OutlineWidth;
    float _Highlight;
    
    struct Input {
        float2 uv_MainTex;
    };
    
    sampler2D _MainTex;

    void vert (inout appdata_full v, out Input o) {
        UNITY_INITIALIZE_OUTPUT(Input,o);
               
        if(abs(v.vertex.x) > 4 && abs(v.vertex.z) > 4){
            v.vertex.z -= sign(v.vertex.x) * sign(v.vertex.z) * (v.vertex.x - sign(v.vertex.x) * 4);
        }         
    }
    
    void surf (Input IN, inout SurfaceOutput o) {
        o.Albedo = tex2D (_MainTex, IN.uv_MainTex).rgb;
        float maxOutline = 1.0 - _OutlineWidth;
        if(IN.uv_MainTex.x < _OutlineWidth ||IN.uv_MainTex.x > maxOutline || IN.uv_MainTex.y < _OutlineWidth || IN.uv_MainTex.y > maxOutline){
            o.Albedo = lerp(o.Albedo,_OutlineColor.rgb, 0.5 + _OutlineIntensity);
        } 
        o.Alpha = 2 * distance(o.Albedo, float3(1,1,1));
        o.Albedo = lerp(o.Albedo, _Color, _Highlight);
    }
    
    half4 LightingToon (SurfaceOutput s, fixed3 viewDir, UnityGI gi) {
        half4 c;
        c.rgb = s.Albedo;
        c.a = s.Alpha;
        return c;
    }
    
    inline void LightingToon_GI(SurfaceOutput s, UnityGIInput data, inout UnityGI gi)
    {
    }
    ENDCG
    }
    FallBack "Diffuse"
}
