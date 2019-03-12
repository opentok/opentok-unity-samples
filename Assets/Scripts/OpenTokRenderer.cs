using UnityEngine;
using System.Collections;
using System.Runtime.InteropServices;
using System;
using OpenTok;

public class OpenTokRenderer: MonoBehaviour, IVideoRenderer
{    
    private Texture2D texture;
    private int width = 1;
    private int height = 1;
    VideoFrame currentFrame;

    public bool Enabled { get; set; }
    void Start()
    {
        texture = new Texture2D(width, height, TextureFormat.ARGB32, false)
        {
            filterMode = FilterMode.Point,
            wrapMode = TextureWrapMode.Clamp
        };
        texture.Apply();
        GetComponent<MeshRenderer>().material.mainTexture = texture;
    }

    public void RenderFrame(VideoFrame frame)
    {
        lock (this)
        {
            currentFrame = frame;
        }

    }

    void Update()
    {
        VideoFrame frame;
        lock (this)
        {
            frame = currentFrame;
            currentFrame = null;
        }

        if (frame != null) {

            if (frame.Width != width || frame.Height != height)
            {
                width = frame.Width;
                height = frame.Height;
                texture = new Texture2D(width, height, TextureFormat.RGBA32, false)
                {
                    filterMode = FilterMode.Point,
                    wrapMode = TextureWrapMode.Clamp
                };

                GetComponent<Renderer>().material.mainTexture = texture;
            }

            var data = texture.GetRawTextureData<Color32>();

            unsafe
            {
                IntPtr ptr = (IntPtr)Unity.Collections.LowLevel.Unsafe.NativeArrayUnsafeUtility.GetUnsafePtr<Color32>(data);
                IntPtr[] buffer = { ptr };
                int[] stride = { width * 4 };

                frame.ConvertInPlace(PixelFormat.FormatAbgr32, buffer, stride);
                frame.Dispose();
            }

            texture.Apply();
        }
    }
}
