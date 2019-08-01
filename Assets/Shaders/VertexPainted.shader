Shader "Custom/VertexPainted"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _AmbientLightColor ("Ambient Light Color", Color) = (1, 1, 1, 1)
        _RimColor ("Rim Color", Color) = (1, 1, 1, 1)
        _RimAmount ("Rim Amount", Range(0, 2)) = 1.2
        _FillAmount ("Fill Amount", Range(-1, 1)) = 0
        _FillColor ("Fill Color", Color) = (1, 1, 1, 1)
        _FogDensity ("Fog Density", Range(0, 1)) = 0
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
        _MainTex2 ("Albedo (RGB)", 2D) = "white" {}
        _MainTex3 ("Albedo (RGB)", 2D) = "white" {}
        _MainTex4 ("Albedo (RGB)", 2D) = "white" {}
        _NoiseTex ("Noise Texture", 2D) = "white" {}
        _Glossiness ("Smoothness", Range(0,1)) = 0.5
        _Metallic ("Metallic", Range(0,1)) = 0.0
        _octaves ("Octaves", Int) = 4
        _UvScale ("UV Scale", Range(1,20)) = 1
        _HighlightColor ("Highlight Color", Color) = (0,0,0,1)
        _Wireframe ("Wireframe", Int) = 0
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200
        
        CGPROGRAM
        #include "fbm.cginc"

        // Physically based Standard lighting model, and enable shadows on all light types
        #pragma surface surf Toon fullforwardshadows vertex:vert

        // Use shader model 3.0 target, to get nicer looking lighting
        #pragma target 3.5
        
        float _FillAmount;
        float _RimAmount;
        fixed4 _RimColor;
        fixed4 _AmbientLightColor;
        float _FogDensity;
        fixed4 _HighlightColor;
    
        sampler2D _MainTex;
        sampler2D _NoiseTex;
        float _UvScale;
        int _Wireframe;

        struct Input
        {
            float2 uv_MainTex: TEXCOORD0;
            float2 uv2_MainTex2;
            float2 uv3_MainTex3;
            float2 uv4_MainTex4;
            half4 tangent;
            float depth;
        };

        half _Glossiness;
        half _Metallic;
        fixed4 _Color;

        // Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
        // See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
        // #pragma instancing_options assumeuniformscaling
        UNITY_INSTANCING_BUFFER_START(Props)
            // put more per-instance properties here
        UNITY_INSTANCING_BUFFER_END(Props)
        
        void vert (inout appdata_full v, out Input o) {
            UNITY_INITIALIZE_OUTPUT(Input,o);
            o.tangent = v.tangent;
            o.depth = length(UnityObjectToViewPos(v.vertex).xyz);
        }
        int _octaves;
        
        float3 hardLight(float blend, float3 target) {
            return (blend > 0.5) * (1 - (1-target) * (1-2*(blend-0.5))) + (blend <= 0.5) * (target * (2*blend));
        }

        void surf (Input IN, inout SurfaceOutput o) {
            float2 uv = _UvScale * IN.uv_MainTex;
            float2 uv2 = uv;
            uv.y += _Time.y / 10.0;
            uv.x -= (sin(_Time.y/10.0)/2.0);
            
            float _ColorPercent = IN.tangent.x;
            float _GlitterPercent = IN.tangent.y;
            float _PoisonPercent = IN.tangent.z;
            float _RainbowPercent = IN.tangent.w;
            
            //float r = IN.uv2_MainTex2.x/255;
            //float t = floor(IN.uv2_MainTex2.y/255);
            //float b = (IN.uv2_MainTex2.y - 255 * t)/255;
            //float g = t/255;
            //float3 color = float3(r,g,b);
            
            float3 color = float3(IN.uv2_MainTex2.x,IN.uv2_MainTex2.y,IN.uv3_MainTex3.x);

            uv2.y += _Time.y / 14.0;
            uv2.x += (sin(_Time.y/10.0)/9.0);
            float result = 0.0;
            result += tex2D(_NoiseTex, uv * 0.4 + _Time.y*0.003).r;
            result *= tex2D(_NoiseTex, uv2 * 0.6 + _Time.y*0.002).b;
            result = clamp(3 * pow(result, 3.0), 0, 1);   
                
            //Glitter Effect 
            float4 glitterColor = 15*result*float4(1,1,1,1);
            float4 glitterAlpha = (1-_GlitterPercent)*1 +_GlitterPercent * (result + 0.01);
            
            //Rainbow Effect 
            float2 uvr = float2(2.0*_UvScale*IN.uv_MainTex.xy-1.0);
            float3 rainbowColor = color;
            rainbowColor.r += 2*sin(_Time.y)*(fbm(uvr+0.1*_Time.y, 1))*color.r;
            rainbowColor.g += 2*cos(_Time.y)*(fbm(uvr-0.1*_Time.y, 1))*color.g;
            rainbowColor.b += 2*sin(_Time.y + 1)*(fbm(uvr-0.1*_Time.y + 100, 1))*color.b;
            
            //Ridge Affect 
            float2 uvm = float2(_PoisonPercent*2.0*_UvScale*IN.uv_MainTex.xy-1.0);
            
            float f = fbm(uvm+fbm(5*uvm + 0.2*_Time.y, _octaves), _octaves);
            
            float colorTotal = _RainbowPercent + _ColorPercent + 0.0001;

            o.Albedo = (_ColorPercent/colorTotal) * color + _GlitterPercent * glitterColor + _RainbowPercent * _RainbowPercent * rainbowColor / (colorTotal * colorTotal);
            o.Albedo = hardLight(0.5 + _PoisonPercent * (2*f - 0.5), o.Albedo);
            o.Alpha = 1;
            
            // default depth shade 
            if(length(IN.tangent) < 0.001){
                o.Albedo = (1/exp(min(IN.depth,10) * _FogDensity)) * float3(1,1,1) + _AmbientLightColor;
                o.Albedo = clamp(o.Albedo, 0, 1);

            }
            // highlighting 
            if(IN.uv3_MainTex3.y > 0) {
                o.Albedo = _HighlightColor;
            }
            // wireframe 
            if ( _Wireframe > 0 ) {
                float3 barys = float3(IN.uv4_MainTex4.xy, 1-IN.uv4_MainTex4.x-IN.uv4_MainTex4.y);
                float3 deltas = fwidth(barys);
                barys = smoothstep(0, deltas, barys);

                float minBary = min(min(barys.x,barys.y),barys.z);
                //o.Albedo += (1 - minBary)*float3(0.5,0.5,0.5);
                o.Albedo -= 0.14*(1-minBary);
            }
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
            
            if(s.Albedo.x < 0){
                c.rgb = float3(1,1,1) / viewDir.z;
            }
            
            return c;
        }
        
        inline void LightingToon_GI(SurfaceOutput s, UnityGIInput data, inout UnityGI gi)
        {
        }
        ENDCG
    }
    FallBack "Diffuse"
}
