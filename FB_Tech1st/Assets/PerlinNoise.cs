using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class PerlinNoise : MonoBehaviour
{
    public int width = 256;//宽度
    public int height = 256;//高度

    public float scale = 20.0f;

    public float offsetX = 100.0f;
    public float offsetY = 100.0f;

    public  bool OnceGenerate = false;

    private void Start()
    {
        offsetX = Random.Range(0.0f, 1000.0f);
        offsetY = Random.Range(0.0f, 1000.0f);
    }

    private void Update()
    {
        Renderer renderer = GetComponent<Renderer>();//拿到Renderer
        renderer.material.mainTexture = GenerateTexture();//主贴图设置

        if (!OnceGenerate)
        {
            OnceGenerate = true;
            SaveTexture(GenerateTexture());
        }
    }

    void SaveTexture( Texture2D texture)
    {
        byte[] bytes = texture.EncodeToPNG();
        var dirPath = Application.dataPath + "/SaveImages/";
        Debug.Log("生成成功:"+ dirPath);
        if (!Directory.Exists(dirPath))
        {
            Directory.CreateDirectory(dirPath);
        }
        File.WriteAllBytes(dirPath + "Image" + ".png", bytes);
    }

    Texture2D GenerateTexture()
    {
        Texture2D texture = new Texture2D(width,height);//新建贴图

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Color color = CalculateColor(x, y);
                texture.SetPixel(x, y, color);
            }
        }
        texture.Apply();

        return texture;
    }

    Color CalculateColor(int x,int y)
    {
        float xCoord = (float)x / width * scale + offsetX;//0-1
        float yCoord = (float)y / height * scale + offsetY;//0-1

        float sample = Mathf.PerlinNoise(xCoord, yCoord);
        return new Color(sample, sample, sample);
    }
}
