using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace FreeBird.Rendering
{
    public class GodRayPass : CustomPostcessPass<GodRay>
    {
        protected override string RenderTag => "GodRay";
        static readonly int GodRayResultTexture = Shader.PropertyToID("_GodRayResTex");//设置暂存贴图

        #region 设置渲染事件
        public GodRayPass(RenderPassEvent renderPassEvent, Shader shader) : base(renderPassEvent, shader)
        {
        }
        #endregion

        #region 【开启】
        protected override bool IsActive()
        {
            return component.IsActive;
        }
        #endregion

        #region 【渲染前】
        protected override void BeforeRender(CommandBuffer commandBuffer, ref RenderingData renderingData)
        {
            ref var cameraData = ref renderingData.cameraData;//汇入摄像机数据
            commandBuffer.GetTemporaryRT(GodRayResultTexture, cameraData.camera.scaledPixelWidth, cameraData.camera.scaledPixelHeight, 0, FilterMode.Trilinear, RenderTextureFormat.Default);//设置目标贴图
            material.SetColor("_GodRayColor", component.mainColor.value);//汇入颜色校正
            material.SetFloat("_GodRayRes", component.GodRayRes.value);//汇入颜色校正
        }
        #endregion

        #region 【渲染】
        protected override void Render(CommandBuffer commandBuffer, ref RenderingData renderingData, RenderTargetIdentifier source, RenderTargetIdentifier dest)
        {
            //光线追踪位置
            commandBuffer.Blit(null, GodRayResultTexture, material, 0);
            //光线追踪结果
            commandBuffer.Blit(source, source, material, 1);
        }
        #endregion
    }
}