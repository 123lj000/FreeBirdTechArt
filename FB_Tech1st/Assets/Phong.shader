Shader "lit/Phong模型"//shader名字

{
    Properties
    {
        _MainTex ("主贴图", 2D) = "white" { }//汇入数据的位置
        _NormalTex ("法线贴图", 2D) = "bump" { }//汇入法线
        _NormalTexScale ("法线强度", float) = 1.0//汇入法线强度
        _GlossScale ("高光放缩", range(0.0, 50.0)) = 1.0
    }
    SubShader
    {
        //定义不透明物体，用的是URP管线，定义队列是不透明队列
        Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline" "Queue" = "Geometry" }
        LOD 100

        Pass
        {
            HLSLPROGRAM

            //定义hlsl语言

            //定义顶点着色器的名字
            #pragma vertex vert
            //定义片元着色器的名字
            #pragma fragment frag
            //汇入函数库以及汇入光照的hlsl
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"//函数库：主要用于各种的空间变换
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"//从unity中取得我们的光照

            //汇入模型的数据
            struct appdata
            {
                float4 vertex : POSITION;//汇入模型的顶点
                float3 normal : NORMAL;//汇入法线
                float2 uv : TEXCOORD0;//汇入uv
                float4 tangent : TANGENT;//汇入切线

            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float3 normal_world : TEXCOORD1;//世界空间的法线
                float3 tangent_world : TEXCOORD2;//世界空间的切线
                float3 bitangent_world : TEXCOORD3;//世界空间的次切线
                float3 vertex_world : TEXCOORD4;//顶点的世界空间
                float4 vertex : SV_POSITION;
            };

            TEXTURE2D(_MainTex);SAMPLER(sampler_MainTex);float4 _MainTex_ST;//采样贴图，采样前面汇入的maintex贴图，贴图采样器，以及贴图st
            TEXTURE2D(_NormalTex);SAMPLER(sampler_NormalTex);//采样贴图，采样前面汇入的法线贴图，贴图采样器，以及贴图st
            float _NormalTexScale, _GlossScale;//汇入法线scale

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = TransformObjectToHClip(v.vertex);//模型空间转换到裁剪空间
                o.vertex_world = TransformObjectToWorld(v.vertex.xyz);//顶点世界空间
                o.normal_world = TransformObjectToWorldNormal(v.normal);//法线从模型空间转换到世界空间
                o.tangent_world = TransformObjectToWorldDir(v.tangent);//切线变换
                o.bitangent_world = normalize(cross(o.normal_world, o.tangent_world)) * v.tangent.w * unity_WorldTransformParams.w;//次切线
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);//UV由主贴图进行定义
                return o;
            }

            float4 frag(v2f i) : SV_Target
            {
                // 【主贴图采样】
                float4 col = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv);//主贴图采样
                float4 pack_normal = SAMPLE_TEXTURE2D(_NormalTex, sampler_NormalTex, i.uv);//法线采样
                float3 unpack_normal = UnpackNormal(pack_normal);//得到法线具体数据

                //法线图的修正
                unpack_normal.xy *= _NormalTexScale;
                unpack_normal.z = 1.0 - saturate(dot(unpack_normal.xy, unpack_normal.xy));

                //【光照与各种向量】
                Light light = GetMainLight();//获取主光源
                float3 L = light.direction;//获取光照方向
                float3 N = normalize(unpack_normal.x * i.tangent_world + unpack_normal.y * i.bitangent_world + unpack_normal.z * i.normal_world);//法线
                float3 V = normalize(_WorldSpaceCameraPos - i.vertex_world);//视角方向
                float3 H = normalize(L + V);//半程向量

                //【计算漫反射】
                float diffuse_Term = max(dot(L, N), 0.0);//定义漫反射项
                float3 diffuse = light.color.rgb * diffuse_Term * col.rgb;//光照颜色*漫反射*物体颜色
                //【计算高光反射】
                float NoH = max(0.0, dot(N, H));//高光noh
                float3 specular = light.color.rgb * col.rgb * pow(NoH, _GlossScale);//高光项

                return float4(diffuse + specular, 1.0);
            }
            ENDHLSL

        }
        //【pass：深度】
        Pass
        {
            Name "DepthOnly"
            Tags{"LightMode" = "DepthOnly"}

            ZWrite On
            ColorMask 0
            Cull off

            HLSLPROGRAM
            // Required to compile gles 2.0 with standard srp library

            #pragma vertex DepthOnlyVertex
            #pragma fragment DepthOnlyFragment

            #include "Packages/com.unity.render-pipelines.universal/Shaders/LitInput.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/Shaders/DepthOnlyPass.hlsl"
            ENDHLSL
        }
        Pass
        {
            Name "ShadowCaster"
            Tags{"LightMode" = "ShadowCaster"}

            ZWrite On
            ZTest LEqual
            ColorMask 0
            Cull[_Cull]

            HLSLPROGRAM
            // Required to compile gles 2.0 with standard srp library
            #pragma prefer_hlslcc gles
            #pragma exclude_renderers d3d11_9x
            #pragma target 2.0

            // -------------------------------------
            // Material Keywords
            #pragma shader_feature _ALPHATEST_ON

            //--------------------------------------
            // GPU Instancing
            #pragma multi_compile_instancing
            #pragma shader_feature _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A

            #pragma vertex ShadowPassVertex
            #pragma fragment ShadowPassFragment

            #include "Packages/com.unity.render-pipelines.universal/Shaders/LitInput.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/Shaders/ShadowCasterPass.hlsl"
            ENDHLSL
        }

    }
    }

