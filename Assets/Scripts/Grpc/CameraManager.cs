using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;

public class CameraManager : MonoBehaviour
{
    public Camera MainCamera;

    string m_ImagePath;

    private int resolutionWidth;
    private int resolutionHeight;

    private int timestep = 0;

    // Start is called before the first frame update
    void Start()
    {
        resolutionWidth = Screen.width;
        resolutionHeight = Screen.height;

        m_ImagePath = Application.dataPath + "/Screenshots/";
        Debug.Log($"[ScreenShot] Path: {m_ImagePath}");

        DirectoryInfo dirInfo = new DirectoryInfo(m_ImagePath);
        if (!dirInfo.Exists)
        {
            Directory.CreateDirectory(m_ImagePath);
        }
    }

    // Update is called once per frame
    void Update()
    {
        timestep += 1;

        if (timestep < 512)
        {
            CaptureCameraFrame();
        }
    }

    public byte[] CaptureCameraFrame()
    {
        DateTimeOffset now = DateTimeOffset.UtcNow;
        long unixTimeMilliseconds = now.ToUnixTimeMilliseconds();
        //long now = DateTime.Now.Millisecond;

        // string name = $"{m_ImagePath}{now}.png";
        string name = $"{m_ImagePath}{System.DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss-ffff")}.png";
        RenderTexture renderTexture = new RenderTexture(resolutionWidth, resolutionHeight, 24);

        MainCamera.targetTexture = renderTexture;
        Texture2D screenShot = new Texture2D(resolutionWidth, resolutionHeight, TextureFormat.RGB24, false);
        //Rect rec = new Rect(0, 0, screenShot.width, screenShot.height);
        MainCamera.Render();
        RenderTexture.active = renderTexture;
        screenShot.ReadPixels(new Rect(0, 0, resolutionWidth, resolutionHeight), 0, 0);
        screenShot.Apply();

        byte[] bytes = screenShot.EncodeToPNG();
        File.WriteAllBytes(name, bytes);

        // Debug.Log($"[Capture:{timestep}] {DateTime.Now.Millisecond - now}ms");
        Debug.Log($"[Capture:{timestep}] {DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - unixTimeMilliseconds}ms");

        return bytes;
    }
}
