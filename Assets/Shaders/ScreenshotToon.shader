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
        _FresnelColor ("Fresnel Color", Color) = (1, 1, 1, 1)
        _FresnelPower ("Fresnel Power", Range(1, 10)) = 0
        _MainTex ("Main Noise", 2D) = "white" {}
    }
    SubShader
    {
        //Tags { "Queue" = "Transparent" "RenderType"="Transparent" }
        LOD 200

    Blend SrcAlpha OneMinusSrcAlpha 

    CGPROGRAM

    #pragma surface surf SimpleFresnel fullforwardshadows vertex:vert alpha:blend
        
        fixed4 _FresnelColor;
        float _FresnelPower;
        fixed4 _Color;
        float _FillAmount;
        float _RimAmount;
        fixed4 _RimColor;
        fixed4 _AmbientLightColor;
        
        struct Input {
            float2 uv_MainTex;
            float3 worldPos;
        };
        
        void vert (inout appdata_full v, out Input o) {
            UNITY_INITIALIZE_OUTPUT(Input,o);
        }
           
        void surf (Input IN, inout SurfaceOutput o) {
            float3 dpdx = ddx(IN.worldPos);
            float3 dpdy = ddy(IN.worldPos);
            float3 normal2 = normalize(cross(dpdy, dpdx));
            o.Albedo = normal2;
        }
        
        half4 LightingSimpleFresnel (SurfaceOutput s, fixed3 viewDir, UnityGI gi) {
        
            float3 normal = s.Albedo;
            float vDot = dot(viewDir, normal);
            float fresnelIntensity = _FresnelPower * pow(saturate(1-vDot), 1);
            float3 fresnelColor = fresnelIntensity * (0.6*_FresnelColor + 0.4*_Color) + (1-fresnelIntensity)*_AmbientLightColor;
            
            half NdotL = dot (normal, gi.light.dir);
            float lightIntensity = smoothstep(0, 0.05, NdotL);
            float3 lightColor = lightIntensity * gi.light.color;

            float4 rimDot = 1 - dot(viewDir, normal);
            float rimIntensity = rimDot * _RimAmount * pow( NdotL, 3.0);

            rimIntensity = smoothstep(_RimAmount - 0.01, _RimAmount + 0.01, rimIntensity);
            float4 rim = rimIntensity * _RimColor;

            half4 c;
            c.rgb = fresnelColor * (lightColor + (1-lightIntensity)*_AmbientLightColor + rim.rgb);
            c.a = 1;

            return c;
        }
        
        inline void LightingSimpleFresnel_GI(SurfaceOutput s, UnityGIInput data, inout UnityGI gi)
        {
        }
        ENDCG

    }
    
    FallBack "Diffuse"
}

