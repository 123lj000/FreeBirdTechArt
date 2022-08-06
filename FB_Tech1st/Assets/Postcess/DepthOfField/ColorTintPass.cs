using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace FreeBird.Rendering
{
    //【执行pass】
    public class ColorTintPass : CustomPostcessPass<ColorTint>
    {
        protected override string RenderTag => "ColorTint Effects";//设置tags
        static readonly int MainTexId = Shader.PropertyToID("_MainTex");//设置主贴图
        static readonly int TempBuffer1 = Shader.PropertyToID("_TempBuffer1");//设置暂存贴图
        static readonly int TempBuffer2 = Shader.PropertyToID("_TempBuffer2");//设置暂存贴图

        #region 设置渲染事件
        public ColorTintPass(RenderPassEvent renderPassEvent, Shader shader):base(renderPassEvent, shader)
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
            if (!component.DebugDOF.value)
            {
                commandBuffer.GetTemporaryRT(TempBuffer1, cameraData.camera.scaledPixelWidth, cameraData.camera.scaledPixelHeight, 0, FilterMode.Trilinear, RenderTextureFormat.Default);//设置目标贴图
                commandBuffer.GetTemporaryRT(TempBuffer2, cameraData.camera.scaledPixelWidth, cameraData.camera.scaledPixelHeight, 0, FilterMode.Trilinear, RenderTextureFormat.Default);//设置目标贴图
            }
            material.SetColor("_ColorTint", component.colorChange.value);//汇入颜色校正
            material.SetFloat("_BlurRange", component.GuassianBlurRange.value);//汇入颜色校正
            material.SetFloat("_DOFForce", component.DOFForce.value);//汇入颜色校正
            material.SetFloat("_DOFStrength1", component.DOFStrength1.value);//汇入颜色校正
            material.SetFloat("_DOFStrength2", component.DOFStrength2.value);//汇入颜色校正
        }
        #endregion

        #region 【渲染】
        protected override void Render(CommandBuffer commandBuffer, ref RenderingData renderingData, RenderTargetIdentifier source, RenderTargetIdentifier dest)
        {
            ref var cameraData = ref renderingData.cameraData;
            commandBuffer.SetGlobalTexture(MainTexId, source);//汇入当前渲染图片
            if (!component.DebugDOF.value)
            {
                commandBuffer.Blit(source, TempBuffer1);//输入
                DualBlur(commandBuffer, cameraData);//双重模糊
                commandBuffer.Blit(TempBuffer1, source);//输出
            }
            else
            {
                commandBuffer.Blit(source, source, material, 0);
            }
        }
        #endregion

        #region【工具箱】
        int Pow2(float a)
        {
            return (int)Mathf.Pow(2, a);
        }
        //【双重模糊函数】
        void DualBlur(CommandBuffer cmd, CameraData cameraData)
        {
            //【双重模糊】
            for (int i = 0; i < component.DualBlurTimes.value; i++)//降采样
            {
                GuassianBlur(cmd);//做高斯模糊

                cmd.ReleaseTemporaryRT(TempBuffer2);

                cmd.GetTemporaryRT(TempBuffer2, cameraData.camera.scaledPixelWidth / Pow2(i + 1), cameraData.camera.scaledPixelHeight / Pow2(i + 1));//设置目标贴图
                cmd.Blit(TempBuffer1, TempBuffer2);
                cmd.ReleaseTemporaryRT(TempBuffer1);
                cmd.GetTemporaryRT(TempBuffer1, cameraData.camera.scaledPixelWidth / Pow2(i + 1), cameraData.camera.scaledPixelHeight / Pow2(i + 1));//设置目标贴图
                cmd.Blit(TempBuffer2, TempBuffer1);
            }

            for (int i = component.DualBlurTimes.value - 1; i >= 0; i--)//升采样
            {
                GuassianBlur(cmd);//做高斯模糊

                cmd.ReleaseTemporaryRT(TempBuffer2);

                cmd.GetTemporaryRT(TempBuffer2, cameraData.camera.scaledPixelWidth / Pow2(i), cameraData.camera.scaledPixelHeight / Pow2(i));//设置目标贴图
                cmd.Blit(TempBuffer1, TempBuffer2);
                cmd.ReleaseTemporaryRT(TempBuffer1);
                cmd.GetTemporaryRT(TempBuffer1, cameraData.camera.scaledPixelWidth / Pow2(i), cameraData.camera.scaledPixelHeight / Pow2(i));//设置目标贴图
                cmd.Blit(TempBuffer2, TempBuffer1);
            }
        }
        //【高斯模糊函数】
        void GuassianBlur(CommandBuffer cmd)
        {
            for (int i = 0; i < component.GuassianBlurTimes.value; i++)
            {
                cmd.Blit(TempBuffer1, TempBuffer2, material, 1);//高斯模糊横处理
                cmd.Blit(TempBuffer2, TempBuffer1, material, 2);//高斯模糊纵处理
            }
        }
        #endregion
    }

}
