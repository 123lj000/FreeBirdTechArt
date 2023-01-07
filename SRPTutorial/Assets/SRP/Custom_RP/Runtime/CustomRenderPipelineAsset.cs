using UnityEngine;
using UnityEngine.Rendering;

[CreateAssetMenu(menuName = "Rendering/Custom Render Pipeline")]//创建渲染管线的菜单位置
public class CustomRenderPipelineAsset : RenderPipelineAsset //继承渲染管线的asset
{
    protected override RenderPipeline CreatePipeline() //创建渲染管线，abstract类需要实现
    {
        return new CustomRenderPipeline();
    }
}
