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
            Camera camera = cameraData.camera;//获得摄像机
            commandBuffer.GetTemporaryRT(GodRayResultTexture, cameraData.camera.scaledPixelWidth, cameraData.camera.scaledPixelHeight, 0, FilterMode.Trilinear, RenderTextureFormat.Default);//设置目标贴图

            material.SetMatrix("_CameraFrustum", FrustumCorners(camera));//四条矢量
            material.SetColor("_GodRayColor", component.mainColor.value);//汇入颜色校正
            material.SetFloat("_GodRayRes", component.GodRayRes.value);//汇入颜色校正
            material.SetFloat("_MaxDistance", component.MaxDistance.value);//汇入最大距离
            material.SetFloat("_MinDistance", component.MinDistance.value);//汇入最小距离
            material.SetFloat("_Intensity", component.Intensity.value);//汇入最小距离
            material.SetFloat("_MaxIterations", component.MaxIterations.value);//汇入迭代次数
        }
        #endregion

        #region 【渲染】
        protected override void Render(CommandBuffer commandBuffer, ref RenderingData renderingData, RenderTargetIdentifier source, RenderTargetIdentifier dest)
        {
            //光线追踪位置
            commandBuffer.Blit(null, GodRayResultTexture, material, 0);
            //光线追踪结果
            commandBuffer.Blit(dest, source, material, 1);
        }
        protected override void CleanupRenderTexture(CommandBuffer commandBuffer, ref RenderingData renderingData)
        {
            commandBuffer.ReleaseTemporaryRT(GodRayResultTexture); //释放RT
            commandBuffer.ReleaseTemporaryRT(TempColorBufferId); //释放RT
        }
        #endregion

        #region 【摄像机四个角】
        private Matrix4x4 FrustumCorners(Camera camera)
        {
            Transform cameraTransform = camera.transform;
            //【获得四个角的位置】
            Vector3[] corners = new Vector3[4];
            camera.CalculateFrustumCorners(new Rect(0, 0, 1, 1), camera.farClipPlane, Camera.MonoOrStereoscopicEye.Mono, corners);

            Matrix4x4 FrustumVectors = Matrix4x4.identity;//初始化

            FrustumVectors.SetRow(0, cameraTransform.TransformVector(corners[0]));//左下角
            FrustumVectors.SetRow(1, cameraTransform.TransformVector(corners[3]));//右下角
            FrustumVectors.SetRow(2, cameraTransform.TransformVector(corners[1]));//左上角
            FrustumVectors.SetRow(3, cameraTransform.TransformVector(corners[2]));//右上角

            return FrustumVectors;
        }
        #endregion
    }
}