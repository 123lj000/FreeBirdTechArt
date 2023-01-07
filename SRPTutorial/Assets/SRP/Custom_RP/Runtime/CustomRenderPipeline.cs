using UnityEngine;
using UnityEngine.Rendering;

public class CustomRenderPipeline : RenderPipeline //继承渲染管线
{
    CameraRenderer renderer = new CameraRenderer();//摄像机渲染的类
    protected override void Render(ScriptableRenderContext context, Camera[] cameras)//实现渲染抽象类
    {
        foreach (Camera camera in cameras)//遍历每个摄像机的渲染
        {
            renderer.Render(context, camera);//每个摄像机的渲染过程
        }
    }
}
