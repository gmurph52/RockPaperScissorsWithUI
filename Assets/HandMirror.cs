using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Leap;
using UnityEngine;
using System.IO.Ports;


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

    SerialPort stream = new SerialPort("COM5", 9600);
    

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
        if (count > 40)
        {
            Debug.Log("before before opened");

            stream.ReadTimeout = 50;
            Debug.Log("before opened");
            stream.Open();
            Debug.Log("opened");
         //   Finger finger = fingers[0];

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

                char[] delimiterChars = { '_' };
                String fingerType = finger.Type.ToString();
                String[] fingerParts = fingerType.Split('_');
                Debug.Log(fingerParts[1] + fingerStatus);

                       
                try
                {
                    Debug.Log("in try");
                    stream.WriteLine(fingerParts[1] + fingerStatus);
                    stream.BaseStream.Flush();
                }
                catch (System.IO.IOException exception)
                {
                    Debug.Log("Couldn't open port!");
                    Debug.Log(exception);
                }





            }

            stream.Close();
            count = 0;
        }
    }
}
