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
            public GameObject directionalLight = null;//平行光
        }
        public Settings settings = new Settings();//开放设置
        GodRayPass godRayPass;//Pass
        public override void Create()//新建pass
        {
            this.name = "GodRayPass";//名字
            godRayPass = new GodRayPass(settings.renderPassEvent,settings.shader);//汇入pass的构造函数
            settings.directionalLight = GameObject.Find("Directional Light");//找到我们的平行光
            if (settings.directionalLight == null)//平行光汇入失败
            {
                Debug.LogError("光线追踪平行光汇入失败");
                return;
            }
            godRayPass.SetDirectionalLight(settings.directionalLight);//设置平行光
        }
        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)//Pass逻辑
        {
            godRayPass.Setup(renderer.cameraColorTarget);//初始化
            renderer.EnqueuePass(godRayPass);//汇入队列
        }
    }

}
