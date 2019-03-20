using System;
using System.Runtime.InteropServices;
using OpenTok;
using UnityEngine;

public class OpenTokVideoCapturer: IVideoCapturer
{
    WebCamTexture webcamTexture;
    Color32[] data;
    private GCHandle gcHandle;

    int width, height;

    IVideoFrameConsumer frameConsumer;

    bool started = false;

    private void UpdateBuffer()
    {
        if (gcHandle.IsAllocated) {
            gcHandle.Free();
        }
        width = webcamTexture.width;
        height = webcamTexture.height;
        data = new Color32[width * height];
        gcHandle = GCHandle.Alloc(data, GCHandleType.Pinned);
    }

    public OpenTokVideoCapturer()
    {
        webcamTexture = new WebCamTexture(WebCamTexture.devices[WebCamTexture.devices.Length - 1].name, 640, 480, 30);
        webcamTexture.Play();
        UpdateBuffer();
    }

    public void Destroy()
    {
        gcHandle.Free();
    }

    public VideoCaptureSettings GetCaptureSettings()
    {
        return new VideoCaptureSettings();
    }

    public void Init(IVideoFrameConsumer frameConsumer)
    {
        this.frameConsumer = frameConsumer;

    }

    public void Start()
    {
        lock (this)
        {
            started = true;
        }
    }

    public void Stop()
    {
        lock (this)
        {
            started = false;
        }
    }

    public void Update()
    {
        if (webcamTexture.didUpdateThisFrame)
        {
            lock(this)
            {
                if (started)
                {
                    if (webcamTexture.width != width || 
                        webcamTexture.height != height)
                    {
                        UpdateBuffer();
                    }
                    webcamTexture.GetPixels32(data);

                    int h = webcamTexture.videoVerticallyMirrored ? height : -height;
                    VideoFrame frame = VideoFrame.CreateFrameFromBuffer(PixelFormat.FormatAbgr32, width, h, gcHandle.AddrOfPinnedObject());
                    int rotation = webcamTexture.videoRotationAngle;
                    if ((rotation == 0 || rotation == 180) && !webcamTexture.videoVerticallyMirrored) {
                        rotation = (rotation + 180) % 360;
                    }                    
                    frameConsumer?.Consume(frame, rotation);
                    frame.Dispose();
                }
            }
        }
    }
}
