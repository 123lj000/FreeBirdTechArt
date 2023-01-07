using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Profiling;

partial class CameraRenderer  //【Editor层面代码】
{
    //前置声明
    partial void DrawGizmos();//绘制物体editor的线框
    partial void DrawUnSupportedShaders();//不支持的shader绘制
    partial void PrepareForSceneWindow();//绘制屏幕窗口UI

    partial void PrepareBuffer();//准备buffer：摄像机名字汇入

#if UNITY_EDITOR
    static Material errorMaterial;//错误材质设置

    static ShaderTagId[] legacyShaderTagIds =
    {
        new ShaderTagId("Always"),
        new ShaderTagId("ForwardBase"),
        new ShaderTagId("PrepassBase"),
        new ShaderTagId("Vertex"),
        new ShaderTagId("VertexLMRGBM"),
        new ShaderTagId("VertexLM")
    };

    string SampleName { get; set; }//采样信息名

    //准备buffer：摄像机名字汇入
    partial void PrepareBuffer()
    {
        Profiler.BeginSample("Editor Only");
        buffer.name = SampleName = camera.name;
        Profiler.EndSample();
    }

    //绘制屏幕UI
    partial void PrepareForSceneWindow()
    {
        if (camera.cameraType == CameraType.SceneView)
        {
            ScriptableRenderContext.EmitWorldGeometryForSceneView(camera);//绘制屏幕UI几何体
        }
    }

    //绘制线框
    partial void DrawGizmos()
    {
        if (Handles.ShouldRenderGizmos())
        {
            context.DrawGizmos(camera, GizmoSubset.PreImageEffects);//屏幕特效前gizmos
            context.DrawGizmos(camera, GizmoSubset.PostImageEffects);//屏幕特效后gizmos
        }
    }

    //不支持的shader绘制
    partial void DrawUnSupportedShaders()
    {
        //不支持的材质汇入
        if (errorMaterial == null)
        {
            errorMaterial = new Material(Shader.Find("Hidden/InternalErrorShader"));
        }
        DrawingSettings drawingSettings = new DrawingSettings(legacyShaderTagIds[0], new SortingSettings(camera))
        {
            overrideMaterial = errorMaterial//设定材质为不支持
        };//渲染设置
        for (int i = 1; i < legacyShaderTagIds.Length; i++)
        {
            drawingSettings.SetShaderPassName(i, legacyShaderTagIds[i]);
        }
        FilteringSettings filteringSettings = FilteringSettings.defaultValue;//默认过滤值
        context.DrawRenderers(cullingResults, ref drawingSettings, ref filteringSettings);//渲染context汇入设置
    }
#else
    string SampleName => bufferName;//采样信息名
#endif
}
