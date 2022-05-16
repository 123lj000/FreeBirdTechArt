using System.IO;
using UnityEngine;
using UnityEditor;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class NoiseGenerater : EditorWindow
{
    //【噪音选项设置】
    public enum NOISEOPTIONS
    {
        PerlinNoise = 0,
        SimpleNoise = 1,
        cellular_noise = 2,
        fbm=3
    }
    public NOISEOPTIONS noiseoption;

    //【噪音宽度设置】
    public enum NOISEWIDTH
    {
        W1 = 1,
        W2 = 2,
        W4 = 4,
        W8 = 8,
        W16 = 16,
        W32 = 32,
        W64 = 64,
        W128 = 128,
        W256 = 256,
        W512 = 512,
        W1024 = 1024,
    }
    public NOISEWIDTH noisewidthoption = NOISEWIDTH.W256;
    //【噪音高度设置】
    public enum NOISEHEIGHT
    {
        H1 = 1,
        H2 = 2,
        H4 = 4,
        H8 = 8,
        H16 = 16,
        H32 = 32,
        H64 = 64,
        H128 = 128,
        H256 = 256,
        H512 = 512,
        H1024 = 1024,
    }
    public NOISEHEIGHT noiseheightoption = NOISEHEIGHT.H256;

    int width = 256;//宽度
    int height = 256;//高度
    float scale = 1.0f;//缩放
    float offsetX = 10.0f;//X位移
    float offsetY = 10.0f;//Y位移
    string AssetsName = "SaveImages";//保存文件夹名字
    Texture2D texture;//纹理初始化
    Vector2 scrollPos;//界面DropDown

    [MenuItem("Tools/Free Birdの噪音生成器")]
    #region 界面初始化
    static void Init()
    {
        EditorWindow window = GetWindow<NoiseGenerater>();
        window.Show();
    }
    #endregion

    #region 上一帧设置
    class NoiseTextureSetting
    {
        public int width = 1;
        public int height = 1;
        public float scale = 1;
        public NOISEOPTIONS noiseoption = NOISEOPTIONS.PerlinNoise;
        public float offsetX = 0;
        public float offsetY = 0;
    }
    NoiseTextureSetting noiseTextureSetting = new NoiseTextureSetting();
    #endregion

    #region 主界面设置
    private void OnGUI()
    {
        noiseoption = (NOISEOPTIONS)EditorGUILayout.EnumPopup("噪音选项设置:", noiseoption);//设置噪音选择
        scrollPos = EditorGUILayout.BeginScrollView(scrollPos, true, true);//DropDown避免界面过死
        var dirPath = Application.dataPath + "/" + AssetsName + "/";
        GUILayout.Label("文件夹路径:"+ dirPath);//当前保存文件夹名字
        AssetsName = EditorGUILayout.TextField(AssetsName);//文件夹名字设置

        GUILayout.BeginHorizontal();
        GUILayout.Label("噪音宽 = ");
        noisewidthoption = (NOISEWIDTH)EditorGUILayout.EnumPopup(noisewidthoption);//噪音宽度设置
        width = (int)noisewidthoption;
        GUILayout.Label("噪音高 = ");
        noiseheightoption =(NOISEHEIGHT)EditorGUILayout.EnumPopup(noiseheightoption);//噪音高度设置
        height = (int)noiseheightoption;
        GUILayout.Label("噪音缩放 = ");
        scale = EditorGUILayout.Slider(scale, 0.0f, 100.0f);//噪音缩放设置
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        GUILayout.Label("噪音位移X = ");
        offsetX = EditorGUILayout.FloatField(offsetX);//噪音位移设置
        GUILayout.Label("噪音位移Y = ");
        offsetY = EditorGUILayout.FloatField(offsetY);//噪音位移设置
        GUILayout.EndHorizontal();

        if (NoiseCheck(width,height,scale,noiseoption,offsetX,offsetY))
        {
            texture = GenerateTexture();//新建贴图
        }

        GUI.skin.button.wordWrap = true;
        if (GUILayout.Button("保存图像"))
        {
            SaveTexture(texture);//保存图像
        }

        GUILayout.Label("纹理浏览：");
        EditorGUI.DrawPreviewTexture(new Rect(25, 120, width, height), texture);//纹理浏览
        EditorGUILayout.EndScrollView();

        NoiseSettingSave();
    }
    #endregion

    #region 设置保存
    void NoiseSettingSave()
    {
        noiseTextureSetting.width = width;
        noiseTextureSetting.height = height;
        noiseTextureSetting.scale = scale;
        noiseTextureSetting.noiseoption = noiseoption;
        noiseTextureSetting.offsetX = offsetX;
        noiseTextureSetting.offsetY = offsetY;
    }

    bool NoiseCheck(int width,int height,float scale,NOISEOPTIONS noiseoption,float offsetX,float offsetY)
    {
        if (noiseTextureSetting.width == width && noiseTextureSetting.height == height&& noiseTextureSetting.scale == scale&& noiseTextureSetting.noiseoption == noiseoption && noiseTextureSetting.offsetX == offsetX && noiseTextureSetting.offsetY == offsetY)
        {
            return false;
        }
        return true;
    }
    #endregion

    #region 保存图像函数
    void SaveTexture(Texture2D texture)
    {
        byte[] bytes = texture.EncodeToPNG();//读取图像为PNG
        var dirPath = Application.dataPath + "/" + AssetsName + "/";//当前文件夹路径
        Debug.Log("生成路径:" + dirPath);//生成路径位置
        if (!Directory.Exists(dirPath))
        {
            Directory.CreateDirectory(dirPath);//没有路径则生成
        }
        for (int i = 0; i < 1000; i++)
        {
            if (!File.Exists(dirPath + "Image" + "(" + i + ")" + ".png"))
            {
                File.WriteAllBytes(dirPath + "Image" + "(" + i + ")" + ".png", bytes);//写入文件里面
                break;
            }
        }
    }
    #endregion

    #region 生成贴图的主函数
    Texture2D GenerateTexture()
    {
        Texture2D texture = new Texture2D(width, height);//新建贴图

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Color color = CalculateColor(x, y);//计算颜色，遍历像素
                texture.SetPixel(x, y, color);//设置像素颜色
            }
        }
        texture.Apply();//应用贴图修改

        return texture;
    }
    #endregion

    #region 颜色计算结果
    Color CalculateColor(int x, int y)
    {
        float xCoord = (float)x / width * scale + offsetX;//UV X
        float yCoord = (float)y / height * scale + offsetY;//UV Y
        float sample = 1;

        switch (noiseoption)
        {
            case NOISEOPTIONS.PerlinNoise:
                sample = Mathf.PerlinNoise(xCoord, yCoord);//Perlin噪音直接输出
                break; 
            case NOISEOPTIONS.SimpleNoise:
                sample = value_noise(new Vector2(xCoord, yCoord));//简单噪音
                break;
            case NOISEOPTIONS.cellular_noise:
                sample = cellular_noise(new Vector2(xCoord, yCoord));//cellular噪音
                break;
            case NOISEOPTIONS.fbm:
                sample = fbm(new Vector2(xCoord, yCoord));//fbm噪音
                break;
            default:
                break;
        }

        return new Color(sample, sample, sample);//输出颜色
    }
    #endregion

    #region 数学库
    Vector2 mod(Vector2 coord, float a)
    {
        return new Vector2(coord.x % a, coord.y % a);
    }
    float fract(float x)
    {
        return x - Mathf.Floor(x);
    }
    Vector2 fract(Vector2 x)
    {
        return new Vector2(x.x - Mathf.Floor(x.x), x.y - Mathf.Floor(x.y));
    }
    Vector2 floor(Vector2 x)
    {
        return new Vector2(Mathf.Floor(x.x), Mathf.Floor(x.y));
    }
    float rand(Vector2 coord)
    {
        // prevents randomness decreasing from coordinates too large
        coord = mod(coord, 10000.0f);
        // returns "random" float between 0 and 1
        return fract(Mathf.Sin(Vector2.Dot(coord, new Vector2(12.9898f, 78.233f))) * 43758.5453f);
    }
    float mix(float x,float y,float level)
    {
        return x * (1 - level) + y * level;
    }
    Vector2 rand2(Vector2 coord)
    {
        // prevents randomness decreasing from coordinates too large
        coord = mod(coord, 10000.0f);
        // returns "random" vec2 with x and y between 0 and 1
        return fract((new Vector2(Mathf.Sin(Vector2.Dot(coord, new Vector2(127.1f, 311.7f))), Mathf.Sin(Vector2.Dot(coord, new Vector2(269.5f, 183.3f))))) * 43758.5453f);
    }
    #endregion

    #region 简单噪音

    float value_noise(Vector2 coord)
    {
        Vector2 i = floor(coord);
        Vector2 f = fract(coord);

        // 4 corners of a rectangle surrounding our point
        float tl = rand(i);
        float tr = rand(i + new Vector2(1.0f, 0.0f));
        float bl = rand(i + new Vector2(0.0f, 1.0f));
        float br = rand(i + new Vector2(1.0f, 1.0f));

        Vector2 cubic = f * f * (new Vector2(3.0f, 3.0f) - 2.0f * f);

        float topmix = mix(tl, tr, cubic.x);
        float botmix = mix(bl, br, cubic.x);
        float wholemix = mix(topmix, botmix, cubic.y);

        return wholemix;

    }
    #endregion

    #region cellular噪音
    float cellular_noise(Vector2 coord)
    {
        Vector2 i = floor(coord);
        Vector2 f = fract(coord);

        float min_dist = 99999.0f;
        // going through the current tile and the tiles surrounding it
        for (float x = -1.0f; x <= 1.0; x++)
        {
            for (float y = -1.0f; y <= 1.0; y++)
            {

                // generate a random point in each tile,
                // but also account for whether it's a farther, neighbouring tile
                Vector2 node = rand2(i + new Vector2(x, y)) + new Vector2(x, y);

                // check for distance to the point in that tile
                // decide whether it's the minimum
                float dist = Mathf.Sqrt((f - node).x * (f - node).x + (f - node).y * (f - node).y);
                min_dist = Mathf.Min(min_dist, dist);
            }
        }
        return min_dist;
    }
    #endregion

    #region fbm噪音
    float fbm(Vector2 coord)
    {
        int OCTAVES = 4;

        float normalize_factor = 0.0f;
        float value = 0.0f;
        float scale = 0.5f;

        for (int i = 0; i < OCTAVES; i++)
        {
            value += Mathf.PerlinNoise(coord.x,coord.y) * scale;
            normalize_factor += scale;
            coord *= 2.0f;
            scale *= 0.5f;
        }
        return value / normalize_factor;
    }
    #endregion

}
