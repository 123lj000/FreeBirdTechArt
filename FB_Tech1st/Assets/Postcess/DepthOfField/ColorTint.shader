Shader "PostProcess/ColorTint"//名字开放位置

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
    float4 _MainTex_ST;
    float4 _ColorTint;//设置颜色校正位置
    //【高斯模糊】
    float _BlurRange;
    //【景深模糊】
    float _DOFForce;
    float _DOFStrength1;
    float _DOFStrength2;
    
    //【函数库】
    float DOFRes(float2 uv)
    {
        // sample the texture
        float depth = SampleSceneDepth(uv);
        float depthValue = Linear01Depth(depth, _ZBufferParams);
        // apply fog
        //float4 res = tex2D(_MainTex, i.uv);
        float DOFRange;
        //y = -k * x + a * k
        DOFRange = (-_DOFStrength1 * depthValue + _DOFForce * _DOFStrength1) * (step(-_DOFForce, -depthValue)) + (_DOFStrength2 * depthValue - _DOFForce * _DOFStrength2) * (step(_DOFForce, depthValue));
        DOFRange = pow(DOFRange,2);
        //y = k * x - a * k
        return DOFRange;
    }
    //【函数库】

    v2f ColorTintvert(appdata v)
    {
        v2f o;
        o.vertex = TransformObjectToHClip(v.vertex);//裁剪空间转换
        o.uv = v.uv;
        return o;
    }

    float4 ColorTintfrag(v2f i) : SV_Target
    {
        float DOFRange = DOFRes(i.uv);

        return DOFRange;
    }

    float4 GuassianBluracrossfrag(v2f i) : SV_Target
    {
        // sample the texture
        //【包围盒:横模糊】
        float blurrange = _BlurRange / 50 * DOFRes(i.uv);
        blurrange = pow(blurrange,2);
        float4 Left = tex2D(_MainTex, i.uv + float2(-blurrange, 0.0)) * 0.2;
        float4 Mid = tex2D(_MainTex, i.uv + float2(0, 0.0)) * 0.6;
        float4 Right = tex2D(_MainTex, i.uv + float2(blurrange, 0.0)) * 0.2;
        float4 col = Left + Mid + Right;
        // apply fog
        return col;
    }
    float4 GuassianBlurcolumnfrag(v2f i) : SV_Target
    {
        // sample the texture
        //【包围盒:纵模糊】
        float blurrange = _BlurRange / 50 * DOFRes(i.uv);
        blurrange = pow(blurrange,2);
        float4 Down = tex2D(_MainTex, i.uv + float2(0.0, -blurrange)) * 0.2;
        float4 Mid = tex2D(_MainTex, i.uv + float2(0, 0.0)) * 0.6;
        float4 Up = tex2D(_MainTex, i.uv + float2(0.0, +blurrange)) * 0.2;
        float4 col = Down + Mid + Up;
        // apply fog
        return col;
    }
    ENDHLSL

    SubShader
    {
        Tags { "RenderPipeline" = "UniversalPipeline" }
        Cull Off ZWrite Off ZTest Always

        Pass
        {
            HLSLPROGRAM

            #pragma vertex ColorTintvert
            #pragma fragment ColorTintfrag
            ENDHLSL

        }

        Pass
        {
            HLSLPROGRAM

            #pragma vertex ColorTintvert
            #pragma fragment GuassianBluracrossfrag
            ENDHLSL

        }

        Pass
        {
            HLSLPROGRAM

            #pragma vertex ColorTintvert
            #pragma fragment GuassianBlurcolumnfrag
            ENDHLSL

        }
    }
}
