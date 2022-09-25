using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace FreeBird.Rendering
{
    public class GodRayPass : CustomPostcessPass<GodRay>
    {
        protected override string RenderTag => "GodRay";
        static readonly int GodRayResultTexture = Shader.PropertyToID("_GodRayResTex");//设置暂存贴图
        private GameObject directionLightGameObject = null;
        static readonly int TempBuffer1 = Shader.PropertyToID("_TempBuffer1");//设置暂存贴图
        static readonly int TempBuffer2 = Shader.PropertyToID("_TempBuffer2");//设置暂存贴图

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
            int DownScale = component.GodRayTextureDownScale.value;//汇入分辨率下降
            commandBuffer.GetTemporaryRT(GodRayResultTexture, cameraData.camera.scaledPixelWidth / DownScale, cameraData.camera.scaledPixelHeight / DownScale, 0, FilterMode.Trilinear, RenderTextureFormat.R16);//设置目标贴图
            commandBuffer.GetTemporaryRT(TempBuffer1, cameraData.camera.scaledPixelWidth / DownScale, cameraData.camera.scaledPixelHeight / DownScale, 0, FilterMode.Trilinear, RenderTextureFormat.R16);//设置目标贴图
            commandBuffer.GetTemporaryRT(TempBuffer2, cameraData.camera.scaledPixelWidth / DownScale, cameraData.camera.scaledPixelHeight / DownScale, 0, FilterMode.Trilinear, RenderTextureFormat.R16);//设置目标贴图

            material.SetMatrix("_CameraFrustum", FrustumCorners(camera));//四条矢量
            material.SetColor("_GodRayColor", component.mainColor.value);//汇入颜色校正
            material.SetFloat("_GodRayRes", component.GodRayRes.value);//汇入颜色校正
            material.SetFloat("_MaxDistance", component.MaxDistance.value);//汇入最大距离
            material.SetFloat("_MinDistance", component.MinDistance.value);//汇入最小距离
            material.SetFloat("_Intensity", component.Intensity.value);//汇入最小距离
            material.SetInt("_LightRangePower", component.LightRangePower.value);//汇入光照衰减函数
            material.SetFloat("_MaxIterations", component.MaxIterations.value);//汇入迭代次数
            material.SetFloat("_BlurRange", component.BlurRange.value);//汇入模糊半径

            //汇入平行光的视角空间位置                               //摄像机的位置 + 平行光的远平面得到太阳的位置
            Vector3 DirectionalLightPos = camera.WorldToViewportPoint(camera.transform.position + directionLightGameObject.transform.forward * camera.farClipPlane);
            Vector4 viewDirectionalLightPos = new Vector4(DirectionalLightPos.x, DirectionalLightPos.y, 0, 0);//传递
            material.SetVector("_LightViewPos", viewDirectionalLightPos);//汇入视角空间的位置
        }
        #endregion

        #region 【渲染】
        protected override void Render(CommandBuffer commandBuffer, ref RenderingData renderingData, RenderTargetIdentifier source, RenderTargetIdentifier dest)
        {
            //光线追踪位置
            commandBuffer.Blit(null, GodRayResultTexture, material, 0);
            //高斯模糊
            GussianBlurGodRay(commandBuffer, GodRayResultTexture);
            //光线追踪结果
            commandBuffer.Blit(dest, source, material, 1);
        }
        protected override void CleanupRenderTexture(CommandBuffer commandBuffer, ref RenderingData renderingData)
        {
            commandBuffer.ReleaseTemporaryRT(GodRayResultTexture); //释放RT
            commandBuffer.ReleaseTemporaryRT(TempColorBufferId); //释放RT
        }
        #endregion

        #region 高斯模糊
        void GussianBlurGodRay(CommandBuffer commandBuffer,int BlurTexture)
        {
            commandBuffer.Blit(BlurTexture, TempBuffer1);//初始化

            for (int i = 0; i < component.BlurTimes.value; i++)
            {
                commandBuffer.Blit(TempBuffer1, TempBuffer2, material, 2);//横
                commandBuffer.Blit(TempBuffer2, TempBuffer1, material, 3);//纵
            }

            commandBuffer.Blit(TempBuffer2, BlurTexture);//初始化
            commandBuffer.ReleaseTemporaryRT(TempBuffer1);
            commandBuffer.ReleaseTemporaryRT(TempBuffer2);
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

        #region 【汇入光照OBJ】
        public void SetDirectionalLight(GameObject light)
        {
            this.directionLightGameObject = light;
        }
        #endregion
    }
}