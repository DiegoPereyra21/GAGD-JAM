using UnityEngine;
using System.IO;

public class CapturePotion : MonoBehaviour
{
    public Camera captureCamera;
    public int width = 512;
    public int height = 512;

    [ContextMenu("Capture PNG")]
    public void CapturePNG()
    {
        RenderTexture renderTexture = new RenderTexture(
            width,
            height,
            24,
            RenderTextureFormat.ARGB32
        );

        renderTexture.Create();

        RenderTexture previousTarget = captureCamera.targetTexture;
        RenderTexture previousActive = RenderTexture.active;

        captureCamera.targetTexture = renderTexture;
        captureCamera.Render();

        RenderTexture.active = renderTexture;

        Texture2D texture = new Texture2D(
            width,
            height,
            TextureFormat.RGBA32,
            false
        );

        texture.ReadPixels(
            new Rect(0, 0, width, height),
            0,
            0
        );

        texture.Apply();

        captureCamera.targetTexture = previousTarget;
        RenderTexture.active = previousActive;

        byte[] png = texture.EncodeToPNG();

        string folder = Path.Combine(Application.dataPath, "PotionCaptures");

        if (!Directory.Exists(folder))
        {
            Directory.CreateDirectory(folder);
        }

        string path = Path.Combine(
            folder,
            "Potion_" + System.DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".png"
        );

        File.WriteAllBytes(path, png);

        Debug.Log("Potion captured: " + path);

        DestroyImmediate(texture);
        renderTexture.Release();
        DestroyImmediate(renderTexture);
    }
}