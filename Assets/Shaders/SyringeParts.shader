﻿Shader "Unlit/SyringeParts"
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
        _GlitterPercent ("Glitter Percent", Range(0, 1)) = 0
        _ColorPercent ("Color Percent", Range(0, 1)) = 0
        _PoisonPercent ("Poison Percent", Range(0, 1)) = 0
        _RainbowPercent ("Rainbow Percent", Range(0, 1)) = 0
        _octaves ("Octaves", Int) = 4
        _Size ("Size", Range(0,1)) = 1
    }
    SubShader
    {
        Tags { "Queue" = "Transparent" "RenderType"="Transparent" }
        LOD 200
        Blend SrcAlpha OneMinusSrcAlpha 

    CGPROGRAM
    #pragma surface surf Toon fullforwardshadows alpha:blend
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
    };
    
    float3 hardLight(float blend, float3 target) {
        return (blend > 0.5) * (1 - (1-target) * (1-2*(blend-0.5))) + (blend <= 0.5) * (target * (2*blend));
    }
    void surf (Input IN, inout SurfaceOutput o) {
        float2 uv = IN.uv_MainTex;
        float2 uv2 = uv;
        uv.y += _Time.y / 10.0;
        uv.x -= (sin(_Time.y/10.0)/2.0);
        
        
        uv2.y += _Time.y / 14.0;
        uv2.x += (sin(_Time.y/10.0)/9.0);
        float result = 0.0;
        result += tex2D(_NoiseTex, uv * 0.4 + _Time.y*0.003).r;
        result *= tex2D(_NoiseTex, uv2 * 0.6 + _Time.y*0.002).b;
        result = clamp(3 * pow(result, 3.0), 0, 1);   
            
        //Glitter Effect 
        float4 glitterAlpha = (1-_GlitterPercent)*1 +_GlitterPercent * (result + 0.01);
        float4 glitterColor = 15*result*float4(1,1,1,1);

        //Rainbow Effect
        float4 rainbowColor = _Color + 0.5 * fixed4(0.5*(sin(2*3.14*IN.uv_MainTex.y + _Time.y)),0.5*(sin(2*3.14*IN.uv_MainTex.x +  _Time.y)),1,1);
        //float4 rainbowColor = 0.5 * fixed4(0.5*(1+sin(2*3.14*IN.uv_MainTex.y + _Time.y)),0.5*(1+sin(2*3.14*IN.uv_MainTex.x +  _Time.y)),1,1);
        float2 uvr = float2(3.0*IN.uv_MainTex.xy-1.0);
        rainbowColor = _Color;
        rainbowColor.r += 2*sin(_Time.y)*(fbm(uvr+0.1*_Time.y, 1))*_Color.r;
        rainbowColor.g += 2*cos(_Time.y)*(fbm(uvr-0.1*_Time.y, 1))*_Color.g;
        rainbowColor.b += 2*sin(_Time.y + 1)*(fbm(uvr-0.1*_Time.y + 100, 1))*_Color.b;
       
        //Ridge Affect 
        float2 uvm = float2(_PoisonPercent*2.0*IN.uv_MainTex.xy-1.0);
        
        float f = fbm(uvm+fbm(5*uvm + 0.2*_Time.y, _octaves), _octaves);
        float colorTotal = _RainbowPercent + _ColorPercent + 0.0001;
        o.Albedo = (_ColorPercent / colorTotal) * _Color + _GlitterPercent * glitterColor + (_RainbowPercent / colorTotal) * _RainbowPercent * rainbowColor;
        o.Albedo = hardLight(0.5 + _PoisonPercent * (2*f - 0.5), o.Albedo);
        
        float empty = 0;
        if ( _FillAmount > -1) {
            empty = 1;
        }
        o.Alpha = empty;
    }
    
    half4 LightingToon (SurfaceOutput s, fixed3 viewDir, UnityGI gi) {
        // ?? lighting ??
        //half NdotL = dot (s.Normal, gi.light.dir);
        //float lightIntensity = smoothstep(0, 0.05, NdotL);
        //float3 lightColor = lightIntensity * gi.light.color;
        
        //float4 rimDot = 1 - dot(viewDir, s.Normal);
        //float rimIntensity = rimDot * _RimAmount * pow( NdotL, 3.0);

        //rimIntensity = smoothstep(_RimAmount - 0.01, _RimAmount + 0.01, rimIntensity);
        //float4 rim = rimIntensity * _RimColor;

        half4 c;
        //c.rgb = s.Albedo * (lightColor + (1-lightIntensity)*_AmbientLightColor + rim.rgb);
        c.rgb = s.Albedo;
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
            v.vertex.xyz += 0.001 * v.normal.xyz;
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

