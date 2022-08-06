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
        public ColorParameter mainColor = new ColorParameter(Color.white, true);//设置颜色
        public FloatParameter GodRayRes = new ClampedFloatParameter(1.0f,0.0f,1.0f);//光线追踪的结果

        public bool IsActive => true;
    }
}
