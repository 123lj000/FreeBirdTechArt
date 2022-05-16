Shader "unlit/Fire"
{
    Properties
    {
        _NoiseTex ("噪音贴图", 2D) = "white" { }
        _FireNoiseSpeed("噪音速度",float) = 1.0
        _NoiseScale ("主噪音", Vector) = (1, 0.64, 0, 0)//噪音强度
        _DistortionStrength ("扭曲强度", float) = 1.0
        _FireBottomOffset ("火焰底部位置", float) = 1.3
        _YStretch ("Y轴扭曲", float) = 1
        [HDR]_Color ("火焰主颜色", color) = (1, 1, 1, 1)
    }
    SubShader
    {
        //定义不透明物体，用的是URP管线，定义队列是不透明队列
        Tags { "RenderType" = "Transparent" "RenderPipeline" = "UniversalPipeline" "Queue" = "Transparent" }
        LOD 100

        Pass
        {
            Cull Back
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            HLSLPROGRAM

            //定义hlsl语言

            //定义顶点着色器的名字
            #pragma vertex vert
            //定义片元着色器的名字
            #pragma fragment frag
            //汇入函数库以及汇入光照的hlsl
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"//函数库：主要用于各种的空间变换

            //汇入模型的数据
            struct appdata
            {
                float4 vertex : POSITION;//汇入模型的顶点
                float2 uv : TEXCOORD0;//汇入uv

            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            float Ellipse(float2 UV, float Width, float Height)
            {
                float d = length((UV * 2 - 1) / float2(Width, Height));
                return saturate((1 - d) / fwidth(d));
            }

            float Posterize(float In, float Steps)
            {
                return floor(In / (1 / Steps)) * (1 / Steps);
            }

            float _DistortionStrength, _FireBottomOffset, _YStretch,_FireNoiseSpeed;
            float4 _NoiseScale, _Color;
            TEXTURE2D(_NoiseTex);SAMPLER(sampler_NoiseTex);float4 _NoiseTex_ST;

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = TransformObjectToHClip(v.vertex);//模型空间转换到裁剪空间
                o.uv = v.uv;//UV由主贴图进行定义
                return o;
            }

            float4 frag(v2f i) : SV_Target
            {
                //【噪音处理】
                float2 noiseUVOffset = float2(0.0, _Time.y * - _FireNoiseSpeed);//噪音UV位移
                float noise = SAMPLE_TEXTURE2D(_NoiseTex, sampler_NoiseTex, i.uv * _NoiseTex_ST.xy + _NoiseTex_ST.zw + noiseUVOffset * _NoiseTex_ST.xy).r;//噪音采样

                //【UV处理】
                float2 UVFlowOffset = float2(0.0, (noise - 0.8) * _DistortionStrength * (pow(i.uv.y, _FireBottomOffset)));//UV扰动处理
                float2 UVFlowTiling = float2(1, _YStretch);//UV扰动处理
                float2 UVFlow = i.uv * UVFlowTiling + UVFlowOffset;//UV修正

                //【火焰遮罩】
                float FireColorGradient = pow(saturate((1 - UVFlow.y)), 2.7);//火焰基础颜色
                float2 FireMaskUV = float2(UVFlow.x, FireColorGradient);//火焰遮罩UV
                float FireMask = Ellipse(FireMaskUV, 0.55, 0.8);//火焰遮罩

                //【火焰颜色】
                float FireTerm = Posterize((FireColorGradient + 0.4) * 1.5, 4);//火焰颜色项
                float3 FireColor = _Color.rgb * (FireTerm + FireMask);//火焰颜色

                return float4(FireColor.rgb,FireMask);
            }
            ENDHLSL

        }
    }
}
