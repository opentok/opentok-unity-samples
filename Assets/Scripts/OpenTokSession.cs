using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using OpenTok;
using System.Runtime.InteropServices;

public class OpenTokSession {

    string API_KEY = "";
    string TOKEN = "";
    string SESSION_ID = "";


    public bool Connected;

    Session session;
    Publisher publisher;
    Subscriber subscriber;

    IVideoRenderer publisherRenderer;
    IVideoRenderer subscriberRenderer;

    OpenTokVideoCapturer videoCapturer;

    public OpenTokSession(GameObject publisherGo, GameObject subscriberGo)
    {
        Debug.LogFormat("Creating session");
        session = new Session(Context.Instance, API_KEY, SESSION_ID);
        session.Connected += Session_Connected;
        session.Disconnected += Session_Disconnected;
        session.StreamReceived += Session_StreamReceived;

        publisherRenderer = publisherGo.GetComponent<OpenTokRenderer>();
        subscriberRenderer = subscriberGo.GetComponent<OpenTokRenderer>();

        videoCapturer = new OpenTokVideoCapturer();

    }

    private void Session_Disconnected(object sender, System.EventArgs e)
    {
        Debug.Log("Session Disconnected");
        session?.Dispose();
        session = null;
        publisher?.Dispose();
        publisher = null;
        subscriber?.Dispose();
        subscriber = null;
        Context.Instance.Dispose();

        Connected = false;
        Debug.Log("Object disposed");

    }

    public void Update()
    {
        videoCapturer.Update();
    }

    public void Connect()
    {
        Debug.LogFormat("Connecting...");
        session.Connect(TOKEN);
    }

    private void Session_StreamReceived(object sender, Session.StreamEventArgs e)
    {
        if (subscriber != null)
        {
            return; // This sample can only handle one subscriber
        }
        Debug.LogFormat("Stream received {0}", e.Stream.Id);

        subscriber = new Subscriber(Context.Instance, e.Stream, subscriberRenderer);
        session.Subscribe(subscriber);
    }

    private void Session_Connected(object sender, System.EventArgs e)
    {
        Debug.Log("Session Connected");
        Connected = true;

        Debug.Log("Creating Publisher");
        publisher = new Publisher(Context.Instance, renderer: publisherRenderer, capturer:videoCapturer);
        publisher.StreamCreated += Publisher_StreamCreated;
        session.Publish(publisher);
    }

    private void Publisher_StreamCreated(object sender, Publisher.StreamEventArgs e)
    {
        Debug.Log("Publisher Stream Created");
    }

    public void Stop()
    {
        Debug.Log("Stopping OT");
        if (Connected)
        {
            session.Disconnect();
        }
    }
}
