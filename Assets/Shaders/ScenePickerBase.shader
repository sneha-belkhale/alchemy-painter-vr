Shader "Unlit/ScenePickerBase"
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
        Tags { "Queue" = "Transparent" "RenderType"="Transparent" }
        LOD 200
        Blend SrcAlpha OneMinusSrcAlpha 

    CGPROGRAM
    #pragma surface surf Toon fullforwardshadows vertex:vert alpha:blend

    fixed4 _Color;
    float _OutlineIntensity;
    fixed4 _OutlineColor;
    float _OutlineWidth;
    
    struct Input {
        float2 uv_MainTex;
    };
    
    void vert (inout appdata_full v, out Input o) {
        UNITY_INITIALIZE_OUTPUT(Input,o);
               
        if(abs(v.vertex.x) > 4 && abs(v.vertex.z) > 4){
            v.vertex.z -= sign(v.vertex.x) * sign(v.vertex.z) * (v.vertex.x - sign(v.vertex.x) * 4);
        }         
    }
    
    sampler2D _MainTex;
    
    void surf (Input IN, inout SurfaceOutput o) {
        o.Albedo = _Color.rgb * tex2D (_MainTex, IN.uv_MainTex).rgb;
        o.Alpha = 0.2;

        float maxOutline = 1.0 - _OutlineWidth;
        if(IN.uv_MainTex.x < _OutlineWidth ||IN.uv_MainTex.x > maxOutline || IN.uv_MainTex.y < _OutlineWidth || IN.uv_MainTex.y > maxOutline){
            o.Albedo = lerp(o.Albedo,_OutlineColor.rgb,_OutlineIntensity);
            
            float2 p = float2(cos(2*_Time.y) + 0.5, sin(2*_Time.y)+ 0.5);
            float dist = length(IN.uv_MainTex - p);
            o.Alpha = 0.3;
            if(dist < 0.6) {
                o.Alpha += dist * max(sin(0.75*_Time.y)-0.3,0);
            }
        }
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
