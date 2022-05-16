Shader "Unlit/FireLesson"
{
    Properties
    {
        _NoiseTex ("噪音图", 2D) = "white" { }
        _FireNoiseSpeed ("噪音扰动速度", float) = 1.0
        _FireBottomOffset ("火焰底部设置", float) = 1.0
        _DistortionStrength ("扭曲强度", float) = 1.0
        _YStretch ("Y轴缩放", float) = 1.0
        [HDR]_Color("火焰主题色",color) = (1,1,1,1)
    }
    SubShader
    {
        Tags { "RenderType" = "Transparent" "RenderPipeline" = "UniversalPipeline" "Queue" = "Transparent" }
        LOD 100

        Pass
        {
            Cull Back
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            HLSLPROGRAM

            #pragma vertex vert
            #pragma fragment frag

            //汇入函数库以及汇入光照的hlsl
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"//函数库：主要用于各种的空间变换

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
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
            //【颜色分块使用】
            float Posterize(float In, float Steps)
            {
                return floor(In / (1 / Steps)) * (1 / Steps);
            }

            sampler2D _MainTex;
            float4 _MainTex_ST;
            TEXTURE2D(_NoiseTex);SAMPLER(sampler_NoiseTex);float4 _NoiseTex_ST,_Color;
            float _FireNoiseSpeed, _FireBottomOffset, _DistortionStrength, _YStretch;

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = TransformObjectToHClip(v.vertex);//汇入到裁剪空间
                o.uv = v.uv;
                return o;
            }

            float4 frag(v2f i) : SV_Target
            {
                //【噪音图采样逻辑】
                float2 noiseUVOffset = float2(0.0, _Time.y * - _FireNoiseSpeed);//噪音图UV上扰动
                float noise = SAMPLE_TEXTURE2D(_NoiseTex, sampler_NoiseTex, (i.uv + noiseUVOffset) * _NoiseTex_ST.xy + _NoiseTex_ST.zw).r;//采样噪音

                //【UV处理】
                float2 UVFlowOffset = float2(0.0, (noise - 0.8) * _DistortionStrength * pow(i.uv.y, _FireBottomOffset));//UV位移
                float2 UVFlowTiling = float2(1, _YStretch);//对Y轴进行整体缩放
                float2 UVFlow = i.uv * UVFlowTiling + UVFlowOffset;//汇入UV扰动

                //【火焰遮罩】
                float FireColorGradient = pow(1 - UVFlow.y, 2.7);//火焰的颜色梯度
                float2 FireMaskUV = float2(UVFlow.x, FireColorGradient);//汇入火焰遮罩的UV:X方向不变，y方向由我们的UV扰动决定
                float FireMask = Ellipse(FireMaskUV, 0.55, 0.8);//椭圆函数

                //【火焰颜色】
                float FireTerm = Posterize((FireColorGradient + 0.4) * 1.5,4);//火焰颜色
                float3 FireColor = _Color.rgb * (FireTerm + FireMask);

                return float4(FireColor.rgb, FireMask);
            }
            ENDHLSL

        }
    }
}
