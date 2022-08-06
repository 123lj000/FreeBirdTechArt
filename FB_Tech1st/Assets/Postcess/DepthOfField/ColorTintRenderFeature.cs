using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace FreeBird.Rendering
{
    public class ColorTintRenderFeature : ScriptableRendererFeature
    {
        [System.Serializable]
        public class Settings
        {
            public RenderPassEvent renderPassEvent = RenderPassEvent.BeforeRenderingPostProcessing;//在后处理前执行我们的颜色校正
            public Shader shader;//汇入shader
        }
        public Settings settings = new Settings();//开放设置
        ColorTintPass colorTintPass;//设置渲染pass
        public override void Create()//新建pass
        {
            this.name = "ColorTintPass";//名字
            colorTintPass = new ColorTintPass(settings.renderPassEvent, settings.shader);//初始化
        }
        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)//Pass逻辑
        {
            colorTintPass.Setup(renderer.cameraColorTarget);//初始化
            renderer.EnqueuePass(colorTintPass);//汇入队列
        }
    }
}
