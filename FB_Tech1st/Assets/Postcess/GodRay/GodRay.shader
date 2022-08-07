﻿Shader "PostProcess/GodRay"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" { }
    }
    HLSLINCLUDE
    #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
    #include "Packages/com.unity.render-pipelines.universal/Shaders/PostProcessing/Common.hlsl"
    #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"
    #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"//从unity中取得我们的光照

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

    struct Rayv2f
    {
        float4 vertex : SV_POSITION;
        float2 uv : TEXCOORD0;
        float4 ray : TEXCOORD1;
    };

    TEXTURE2D(_MainTex);SAMPLER(sampler_MainTex);//屏幕贴图
    TEXTURE2D(_GodRayResTex);SAMPLER(sampler_GodRayResTex);//光线追踪的结果
    float4 _MainTex_ST;
    float4 _GodRayColor;//设置颜色校正位置
    float4x4 _CameraFrustum;//摄像机四条矢量
    float _MaxDistance;//最大距离
    float _MinDistance;//最大距离
    float _MaxIterations;//迭代次数
    float _Intensity;//godray强度

    //【光线追踪结果】
    float _GodRayRes;

    Rayv2f GodRayvert(appdata v)
    {
        Rayv2f o;
        o.vertex = TransformObjectToHClip(v.vertex);//裁剪空间转换
        o.uv = v.uv;

        int index = o.uv + 2 * o.uv.y;//索引
        o.ray = _CameraFrustum[index];//四个方向

        return o;
    }

    v2f GodRayCombinevert(appdata v)
    {
        v2f o;
        o.vertex = TransformObjectToHClip(v.vertex);//裁剪空间转换
        o.uv = v.uv;
        return o;
    }

    inline float GetRandomNumber(float2 uv, float seed)
    {
        return frac(sin(dot(uv, float2(12.9898, 78.233)) + seed) * 43758.5453);
    }

    float RayMarching(float3 rayOrgin, float3 rayDirection, float depthDistance)
    {
        float step = _MaxDistance / _MaxIterations;//最大距离除以步进次数
        float t = _MinDistance + GetRandomNumber(rayDirection, _Time.y * 100);//开始步进的起点
        
        float Result = 0;

        for (int i = 0; i < _MaxIterations; i++)
        {
            //【步进的位置超出既定范围或者深度最大范围】
            if (t > _MaxDistance || t > depthDistance)
            {
                break;
            }
            float3 p = rayOrgin + rayDirection * t;//当前点的位置

            float4 shadowCoord = TransformWorldToShadowCoord(p);//获得采样shadow的参数
            float shadow = SAMPLE_TEXTURE2D_SHADOW(_MainLightShadowmapTexture, sampler_MainLightShadowmapTexture, shadowCoord);//判断点是不是在阴影里面
            if (shadow >= 1)//不在阴影里面
            {
                Result += step * 0.01;//不在阴影里面，加光照

            }
            t += step;//步进

        }

        return saturate(Result);
    }

    //【光线追踪的过程】
    float4 GodRayfrag(Rayv2f i) : SV_Target
    {
        float depth = SampleSceneDepth(i.uv);//深度
        float depthValue = Linear01Depth(depth, _ZBufferParams);//01深度值
        depthValue *= length(i.ray);

        float3 rayOrigin = _WorldSpaceCameraPos.xyz;//摄像机的起始位置
        float3 rayDir = normalize(i.ray.xyz);//射线方向
        float rayResult = RayMarching(rayOrigin, rayDir, depthValue);//汇入摄像机位置，射线方向，和深度值

        return rayResult;
    }

    //【光线追踪结果整合】
    float4 GodRayCombine(v2f i) : SV_Target
    {
        float4 albedo = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv);//主贴图
        float GodRayRes = SAMPLE_TEXTURE2D(_GodRayResTex, sampler_GodRayResTex, i.uv);//光线追踪结果

        float4 final_color = lerp(max(albedo, 0.0), _GodRayColor, GodRayRes.r * _Intensity);//光线追踪的结果

        return final_color;
    }

    ENDHLSL

    SubShader
    {
        Tags { "RenderPipeline" = "UniversalPipeline" }
        Cull Off ZWrite Off ZTest Always

        //【光线追踪的过程】:目的为了把光线最终的结果传出来
        Pass
        {
            HLSLPROGRAM

            #pragma vertex GodRayvert
            #pragma fragment GodRayfrag
            ENDHLSL

        }
        //【光线追踪的合并】：合并到我们的主贴图
        Pass
        {
            HLSLPROGRAM

            #pragma vertex GodRayCombinevert
            #pragma fragment GodRayCombine
            ENDHLSL

        }
    }
}
