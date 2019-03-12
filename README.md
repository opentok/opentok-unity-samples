Unity OpenTok Sample
================================

This sample will show a couple of 3d planes on which the video stream of the participants in a Session will be rendered.

How to Run the sample
-------------------------

For the sake of simplicity, this sample already has OpenTok Windows SDK v2.12.0 in the `DLL/` folder. However, you might want to update the SDK to the latest version of it. Please check [OpenTok at NuGet gallery](https://www.nuget.org/packages/OpenTok.Client/).

In the `Assets` Folder, you will also find a version of [NativePlugin](../../NativePlugin/RenderPlugin) already built into a DLL so you can open this project, fill the typical `API_KEY`, `TOKEN`, and `SESSION_ID` data and run the project.

Exploring the Code
-----------------------

The core of this sample lives in `Assets/Script` folder, there you will find:

### SceneScript.cs

This script is attached to a GameObject Scene, and it will be run upon the sample starts. Mainly, it creates a `OpenTokSession` object, and call its `Connect()` method. Please pay attention to the `OnApplicationQuit()` method, this will be called whenever you hit _stop_ on the Unity editor. For threading and memory issues it's very important to call on `OpenTokSession.Stop()` so the required resources are freed. Since this usually takes some time, we put a waiting loop to ensure that everythig is freed before exiting `OnApplicationQuit()`

### OpenTokRenderer.cs

This is a Unity/MonoBehaviour subclass compontent that will be attached to Publisher and Subscribers GameObjects inside the unity scene. This Component, in each Update loop, will be in charge of issue plugin events by calling, `GL.IssuePluginEvent(RenderPlugin.GetRenderEventFunc(), rendererId);` or in other words, every time _IssuePluginEvent_ is called, it will call [`OnRenderEvent`](https://github.com/opentok/UnityRenderingSample/blob/master/NativePlugin/RenderPlugin/RenderPlugin.cpp#L222) in the Native Plugin.

Besides this, this component will also create the `Texture2D` object, send the reference to the Native Plugin, and assign it to the GameObject it is attached:

```csharp
texture = new Texture2D(newWidth, newHeight, TextureFormat.BGRA32, false);
RenderPlugin.SetRendererTexture(rendererId, texture.GetNativeTexturePtr(),
                                newWidth, newHeight);
GetComponent<MeshRenderer>().material.mainTexture = texture;
```

### OpenTokSession.cs

This class operates the OpenTok Windows SDK. This will connect to the session, listen for the _Connected_, or _StreamReceived_ events and creating _Publisher_ and _Subscriber_ objects.

The class takes two parameters in its constructor method, they are the GameObjects where the video will be rendered into. Note that these two GameObjects need to have a OpenTokRenderer attached.

Whenever the session is connected or a new stream is received, it will create a _Publisher_ and a _Subscriber_ object, and will configure them to use a `VideoRender` instance.

### VideoRender.cs

This is the _Custom Video Driver_ class associated to the Publisher or Subscriber. As every _Custom Video Driver_, its `public void RenderFrame(VideoFrame frame)` method will be called on each frame. The Role of this _Video Driver_ is to feed the Native plugin with the frames, by doing:

```csharp
frame.ConvertInPlace(PixelFormat.FormatArgb32, buffer, strides);
RenderPlugin.SetRendererFrame(rendererId, buffer[0], frame.Width, frame.Height);
frame.Dispose();
```

### RenderPlugin.cs

This is just the c# to c bridge that will allow us to call the NativePlugin C methods from c# using [Platform Invoke](https://msdn.microsoft.com/en-us/library/55d3thsc.aspx)