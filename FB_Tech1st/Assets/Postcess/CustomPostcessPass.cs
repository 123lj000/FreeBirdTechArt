using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

//【用来后处理的命名空间】
namespace FreeBird.Rendering
{
    public abstract class CustomPostcessPass<T> : ScriptableRenderPass where T: VolumeComponent
    {
        //ID
        protected static readonly int TempColorBufferId = Shader.PropertyToID("_TempColorBuffer");

        //渲染物
        protected Shader shader;
        protected Material material;
        protected T component;

        //渲染目标
        private RenderTargetIdentifier _renderTargetIdentifier;
        private RenderTargetIdentifier _tempRenderTargetIdentifier;

        //标签tag
        protected abstract string RenderTag { get; }

        //【构造函数】
        public CustomPostcessPass(RenderPassEvent renderPassEvent,Shader shader)
        {
            this.renderPassEvent = renderPassEvent;//汇入当前的渲染事件   
            this.shader = shader;//

            if (this.shader == null)
            {
                Debug.Log("后处理：" + RenderTag + "的shader失效，请排查");
                return;
            }

            material = CoreUtils.CreateEngineMaterial(shader);//创造材质
        }
        //【有没有开】
        protected abstract bool IsActive();
        //【初始化】
        public virtual void Setup(in RenderTargetIdentifier renderTargetIdentifier)
        {
            _renderTargetIdentifier = renderTargetIdentifier;
            _tempRenderTargetIdentifier = new RenderTargetIdentifier(TempColorBufferId);
        }
        //【执行】
        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            //判断摄像机开了后处理了么
            if (!renderingData.cameraData.postProcessEnabled)
            {
                return;
            }
            if (material == null)
            {
                Debug.Log("后处理：" + RenderTag + "的material失效，请排查");
                return;
            }
            //【渲染设置】
            var stack = VolumeManager.instance.stack;//传入volume数据
            component = stack.GetComponent<T>();//拿到我们的Volume
            if (component == null || !component.active ||!IsActive())//Volume有没有开放
            {
                return;
            }

            CommandBuffer commandBuffer = CommandBufferPool.Get(RenderTag);//获得cmd
            Render(commandBuffer, ref renderingData);//渲染
            context.ExecuteCommandBuffer(commandBuffer);//执行cmd
            CommandBufferPool.Release(commandBuffer);//释放cmd
        }
        //【渲染command】
        private void Render(CommandBuffer commandBuffer,ref RenderingData renderingData)
        {
            RenderTargetIdentifier source = _renderTargetIdentifier;//当前渲染画面
            RenderTargetIdentifier dest = _tempRenderTargetIdentifier;//渲染终点

            //【准备RT】
            SetupRenderTexture(commandBuffer, ref renderingData);
            //【渲染前你要干啥】
            BeforeRender(commandBuffer, ref renderingData);
            //【复制到tempBuffer，两者贯通】
            CopyToTempBuffer(commandBuffer, ref renderingData, source, dest);
            //【开始渲染】
            Render(commandBuffer, ref renderingData, source, dest);
            //【清除RT】
            CleanupRenderTexture(commandBuffer, ref renderingData);
        }
        #region 【准备RT】
        protected virtual void SetupRenderTexture(CommandBuffer commandBuffer,ref RenderingData renderingData)
        {
            ref CameraData cameraData = ref renderingData.cameraData;//汇入摄像机数据

            var desc = new RenderTextureDescriptor(cameraData.camera.scaledPixelWidth, cameraData.camera.scaledPixelHeight);//设置渲染终点
            desc.colorFormat = cameraData.isHdrEnabled ? RenderTextureFormat.DefaultHDR : RenderTextureFormat.Default;//设置颜色格式

            commandBuffer.GetTemporaryRT(TempColorBufferId, desc);//获得目标RT
        }
        #endregion

        #region【渲染前你要干啥】
        protected abstract void BeforeRender(CommandBuffer commandBuffer, ref RenderingData renderingData);
        #endregion

        #region 【复制到tempBuffer，两者贯通】
        protected virtual void CopyToTempBuffer(CommandBuffer commandBuffer, ref RenderingData renderingData,RenderTargetIdentifier source,RenderTargetIdentifier dest)
        {
            commandBuffer.Blit(source, dest);
        }
        #endregion

        #region 【开始渲染】
        protected virtual void Render(CommandBuffer commandBuffer, ref RenderingData renderingData, RenderTargetIdentifier source, RenderTargetIdentifier dest)
        {
            commandBuffer.Blit(source, dest, material);
        }
        #endregion

        #region 【清除RT】
        protected virtual void CleanupRenderTexture(CommandBuffer commandBuffer, ref RenderingData renderingData)
        {
            commandBuffer.ReleaseTemporaryRT(TempColorBufferId); //释放RT
        }
        #endregion

    }
}
