using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class ColorTint : VolumeComponent
{
    //【设置颜色参数】
    public ColorParameter colorChange = new ColorParameter(Color.white, true);//如果有两个true,则为HDR设置
    //【高斯模糊：次数】
    public IntParameter GuassianBlurTimes = new ClampedIntParameter(1, 0, 20);//模糊次数限制在0-5
    //【高斯模糊：半径】
    public FloatParameter GuassianBlurRange = new ClampedFloatParameter(1.0f, 0.0f, 5.0f);//模糊半径
    //【双重模糊：次数】
    public IntParameter DualBlurTimes = new ClampedIntParameter(1, 0, 5);//模糊次数限制在0-5
    //【景深模糊：焦点】
    public FloatParameter DOFForce = new ClampedFloatParameter(0.5f, 0.0f, 1.0f);
    //【景深模糊：强度】
    public FloatParameter DOFStrength1 = new ClampedFloatParameter(0.5f, 0.0f, 20.0f);
    public FloatParameter DOFStrength2 = new ClampedFloatParameter(0.5f, 0.0f, 20.0f);
    public BoolParameter DebugDOF = new BoolParameter(false);

    public bool IsActive => (DualBlurTimes.value > 0 && GuassianBlurTimes.value > 0);
}
