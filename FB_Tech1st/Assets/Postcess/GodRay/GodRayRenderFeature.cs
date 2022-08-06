using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace FreeBird.Rendering
{
    public class GodRayRenderFeature : ScriptableRendererFeature
    {
        [System.Serializable]
        public class Settings
        {
            public RenderPassEvent renderPassEvent = RenderPassEvent.BeforeRenderingPostProcessing;//在后处理前执行我们的颜色校正
            public Shader shader;//汇入shader
        }
        public Settings settings = new Settings();//开放设置
        GodRayPass godRayPass;//Pass
        public override void Create()//新建pass
        {
            this.name = "GodRayPass";//名字
            godRayPass = new GodRayPass(settings.renderPassEvent,settings.shader);
        }
        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)//Pass逻辑
        {
            godRayPass.Setup(renderer.cameraColorTarget);//初始化
            renderer.EnqueuePass(godRayPass);//汇入队列
        }
    }

}
