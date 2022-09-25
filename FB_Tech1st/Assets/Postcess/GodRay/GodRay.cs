using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;


namespace FreeBird.Rendering
{
    public class GodRay : VolumeComponent
    {
        //【设置颜色参数】
        public IntParameter GodRayTextureDownScale = new ClampedIntParameter(1, 1, 16);//汇入后处理分辨率
        public ColorParameter mainColor = new ColorParameter(Color.white, true);//设置颜色
        public FloatParameter GodRayRes = new ClampedFloatParameter(1.0f,0.0f,1.0f);//光线追踪的结果
        public FloatParameter MaxDistance = new ClampedFloatParameter(1.0f,0.0f,200.0f);//最大距离
        public FloatParameter MinDistance = new ClampedFloatParameter(1.0f,0.0f,20.0f);//最大距离
        public IntParameter MaxIterations = new ClampedIntParameter(1,0,200);//迭代次数
        public FloatParameter Intensity = new ClampedFloatParameter(1.0f,0.0f,1.0f);//最大距离
        public IntParameter LightRangePower = new ClampedIntParameter(1, 0, 8);//汇入光照衰减函数
        public FloatParameter BlurRange = new ClampedFloatParameter(1, 0, 5);//汇入光照衰减函数
        public IntParameter BlurTimes = new ClampedIntParameter(1, 0, 10);//汇入光照衰减函数

        public bool IsActive => true;
    }
}
