OpenTok Unity Sample
================================

The purpose of this sample is to show OpenTok SDK usage from Unity. This sample works in all the platforms where OpenTok SDK is officially released: Windows, iOS, and Android. It works also on MacOS.

This sample will show a couple of 3d planes on which the video stream of the participants in an OpenTok Session will be rendered.

Given that Unity supports C# language we have used the same interface that we expose in the [Windows SDK](https://tokbox.com/developer/sdks/windows/reference/), however it is important to note that although it uses the same API, it works on **all platforms**, not just in Windows.

How to Run the sample
-------------------------

For the sake of simplicity, this sample already has iOS, MacOS, Android and Windows OpenTok SDK v2.16.1 in the `Assets/DLL/` folder, so you don't need to download them.

Open the project in Unity 2018.4.2f1, and fill the usual API_KEY, SESSIONID, TOKEN, in the `Assets/Scripts/OpenTokSession.cs` file.
You can run the sample in the Unity Editor, or you can generate a platform specific artifact by clicking on `File/Build Settings...` and by choosing the platform you want to generate the binary for.

In the case of iOS you will need to setup the right code signing attributes and provisioning profiles.

Exploring the Code
-----------------------

The core of this sample lives in `Assets/Script` folder, there you will find:

### SceneScript.cs

This script is attached to a GameObject Scene, and which will run when the sample starts. Mainly, it creates a `OpenTokSession` object, and call its `Connect()` method. Please pay attention to the `OnApplicationQuit()` method because this will be called whenever you hit _stop_ on the Unity editor. For threading and memory issues it's very important to call on `OpenTokSession.Stop()` so the required resources are freed. Since this usually takes some time, we put a waiting loop to ensure that everythig is freed before exiting `OnApplicationQuit()`.

### OpenTokSession.cs

This class uses the OpenTok SDK and is responsible for connecting to the session, listening for the `Connected`, or `StreamReceived` events and creating `Publisher` and `Subscriber` objects.

The class takes two parameters in its constructor method, they are the GameObjects where the video will be rendered into. Note that these two GameObjects need to have a OpenTokRenderer attached.

Whenever the session is connected or a new stream is received, it will create a `Publisher` and a `Subscriber` object. In both instance contruction, it will use the `OpenTokRenderer` instance we will see below. This`OpenTokRenderer` will be in charge of drawing the actual video frames.

### OpenTokRenderer.cs

This is a Unity/MonoBehaviour subclass compontent that will be attached to Publisher and Subscribers GameObjects inside the unity scene. This class also implements the `IVideoRenderer` from OpenTok SDK. Implementing this interface means that `RenderFrame(VideoFrame frame)` method will be called whenever a frame is ready to be renderer.

In this method we save the frame locally so the `Update` method will draw it

```csharp
public void RenderFrame(VideoFrame frame)
{
    lock (this)
    {
        currentFrame = frame;
    }

}
```

Since this is a `MonoBehaviour` subclass, the `Update` method will be called regularly. In this method we create a `Texture2D` instance where will be copying the frame image data

```csharp
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
```

That texture is shown in the `GameObject` since it is assigned to its material.

### OpenTokVideoCapture.cs

This class is a `IVideoCapturer` implementation and will use Unity SDK to capture images from the computer camera to send them using OpenTok SDK. Using `Unity/WebCamTexture` class we ensure that same code will run in any platform where Unity is supported.

For more details on how to build a custom capturer, please refer to [our documentation](https://tokbox.com/developer/sdks/windows/reference/interface_open_tok_1_1_i_video_capturer.html).
