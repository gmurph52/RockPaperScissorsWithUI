using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Leap;
using UnityEngine;


/**
 *
 */
public class HandMirror
{
    public bool doneWithHandMirroring = false;
    HandMirrorListner listener;
    Controller controller;

    public void start()
    {
        Debug.Log("in start");

          listener = new HandMirrorListner();
          controller = new Controller();
          controller.AddListener(listener);

        // Keep this process running the hand mirroring button is turned off
        // Console.WriteLine("Press Enter to quit...");
        // Console.ReadLine();

    }

    public void end()
    {
        Debug.Log("in end");

        controller.RemoveListener(listener);
        controller.Dispose();
    }
}

/**
 *
 */
public class HandMirrorListner : Listener
{
    int count = 0;
    private System.Object thisLock2 = new System.Object();

    private void SafeWriteLine(String line)
    {
        lock (thisLock2)
        {
            Console.WriteLine(line);
        }
    }

    public override void OnConnect(Controller controller)
    {
        SafeWriteLine("Connected");
    }


    public override void OnFrame(Controller controller)
    {
        Frame frame = controller.Frame();
        Hand hand = frame.Hands.Rightmost;
        FingerList fingers = hand.Fingers;
        String fingerStatus = "";

        count++;
        if (count > 10)
        {
            foreach (Finger finger in fingers)
            {
                if (finger.IsExtended)
                {
                    fingerStatus = "OPEN";
                }
                else
                {
                    fingerStatus = "CLOSED";
                }
                Debug.Log(finger.Type + "_" + fingerStatus);
            }
            count = 0;
        }
    }
}
