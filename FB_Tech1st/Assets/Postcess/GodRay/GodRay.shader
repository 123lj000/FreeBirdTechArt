Shader "PostProcess/GodRay"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" { }
    }
    HLSLINCLUDE
    #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
    #include "Packages/com.unity.render-pipelines.universal/Shaders/PostProcessing/Common.hlsl"
    #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"
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

    sampler2D _MainTex;
    sampler2D _GodRayResTex;
    float4 _MainTex_ST;
    float4 _GodRayColor;//设置颜色校正位置

    //【光线追踪结果】
    float _GodRayRes;

    v2f GodRayvert(appdata v)
    {
        v2f o;
        o.vertex = TransformObjectToHClip(v.vertex);//裁剪空间转换
        o.uv = v.uv;
        return o;
    }

    //【光线追踪的过程】
    float4 GodRayfrag(v2f i) : SV_Target
    {
        float4 color = _GodRayRes.rrrr;

        return color;
    }

    //【光线追踪结果整合】
    float4 GodRayCombine(v2f i): SV_Target
    {
        float4 albedo = tex2D(_MainTex, i.uv);//主贴图
        float GodRayRes = tex2D(_GodRayResTex, i.uv);//光线追踪结果

        float4 final_color = lerp(max(albedo,0.0),_GodRayColor,GodRayRes);//光线追踪的结果

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
            #pragma vertex GodRayvert
            #pragma fragment GodRayCombine
            ENDHLSL
        }
        
    }
}
