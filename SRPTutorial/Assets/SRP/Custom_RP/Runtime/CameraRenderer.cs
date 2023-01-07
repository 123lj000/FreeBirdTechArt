using UnityEngine;
using UnityEngine.Rendering;

public partial class CameraRenderer
{
    ScriptableRenderContext context;//放置渲染的目录

    Camera camera;//摄像机

    const string bufferName = "Render Camera";//buffer缓冲名字

    CullingResults cullingResults;//裁剪结果汇入

    static ShaderTagId unlitShaderTagId = new ShaderTagId("SRPDefaultUnlit");//无光照ID

    public void Render(ScriptableRenderContext context, Camera camera)
    {
        this.context = context;//汇入渲染CMD信息
        this.camera = camera;//汇入摄像机信息

        Setup();//准备摄像机信息

        PrepareForSceneWindow();
        if (!Cull())
        {
            return;
        }

        DrawVisibleGeometry();//渲染可见几何体

        DrawUnSupportedShaders();//渲染不支持的shader

        DrawGizmos();

        Submit();//提交渲染信息
    }

    CommandBuffer buffer = new CommandBuffer //渲染规矩的位置
    {
        name = bufferName//传入name
    };


    void Setup()//设置摄像机相关信息
    {
        context.SetupCameraProperties(camera);//准备摄像机的属性
        buffer.ClearRenderTarget(true, true, Color.clear);//清除RT
        buffer.BeginSample(bufferName);//渲染信息收集开始的位置
        ExecuteBuffer();//执行buffer
    }

    bool Cull()
    {
        //没有被裁剪
        if (camera.TryGetCullingParameters(out ScriptableCullingParameters p))
        {
            cullingResults = context.Cull(ref p);//保存裁剪结果
            return true;//没被裁剪
        }
        return false;//有被裁剪
    }

    void DrawVisibleGeometry()//渲染可见几何体
    {
        //【不透明物体】+++++++++++++++++++++++++++
        SortingSettings sortingSettings = new SortingSettings(camera)//汇入不透明物体渲染队列规则
        {
            criteria = SortingCriteria.CommonOpaque
        };

        //汇入整合物体的设置
        DrawingSettings drawingSettings = new DrawingSettings(unlitShaderTagId, sortingSettings);//渲染设置
        FilteringSettings filteringSettings = new FilteringSettings(RenderQueueRange.opaque);//过滤设置，用于区分不同的渲染

        context.DrawRenderers(cullingResults, ref drawingSettings, ref filteringSettings);//把裁剪结果汇入到渲染设置里面去
        //【不透明物体】+++++++++++++++++++++++++++

        //【天空盒】+++++++++++++++++++++++++++
        context.DrawSkybox(camera);//绘制天空盒
        //【天空盒】+++++++++++++++++++++++++++

        //【透明物体】+++++++++++++++++++++++++++

        //汇入整合物体的设置
        sortingSettings.criteria = SortingCriteria.CommonTransparent;//设定归类规则为透明位置
        drawingSettings.sortingSettings = sortingSettings;//修改渲染规则
        filteringSettings.renderQueueRange = RenderQueueRange.transparent;//设定透明渲染队列

        context.DrawRenderers(cullingResults, ref drawingSettings, ref filteringSettings);//把裁剪结果汇入到渲染设置里面去

        //【透明物体】+++++++++++++++++++++++++++
    }

    void Submit()
    {
        buffer.EndSample(bufferName);//渲染信息收集结束的位置
        ExecuteBuffer();//执行buffer
        context.Submit();//提交drawcall
    }
    void ExecuteBuffer()//执行buffer
    {
        context.ExecuteCommandBuffer(buffer);//汇入渲染缓冲
        buffer.Clear();//记得clear释放
    }
}
