using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

public class SceneScript : MonoBehaviour {

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void MyDelegate(string str);

    public GameObject publisher;
    public GameObject subscriber;

    private OpenTokSession opentok;

    void Start () {
        Debug.LogFormat("Starting OpenTok sample");
        opentok = new OpenTokSession(publisher, subscriber);
        opentok.Connect();
    }

    // Update is called once per frame
    void Update () {
        opentok?.Update();
    }

    void OnApplicationQuit()
    {
        if (opentok != null && opentok.Connected)
        {
            opentok.Stop();

            while(opentok.Connected)
            {
                // Wait until session is disconnected
                new WaitForSeconds(1);
            }
        }
    }

    public void StopSession()
    {
        opentok?.Stop();            
    }
}
