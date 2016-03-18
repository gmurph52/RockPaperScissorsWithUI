using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Leap;
using UnityEngine;
using System.IO.Ports;


/**
 * This class is intended to be used with a robotic arm and a Leap Motion camera. 
 * A listener is created to get the finger positions of the users hand from the 
 * Leap Motion camera.
 */
public class HandMirror
{
    public bool doneWithHandMirroring = false;
    HandMirrorListner listener;
    Controller controller;
   
    /**
     * This method initiates the hand mirroring listener. The listener will 
     * continue to run until the end() method is called.
     */
    public void start()
    {
        Debug.Log("in start");

          listener = new HandMirrorListner();
          controller = new Controller();
          controller.AddListener(listener);
    }

    /**
     * This method ends the hand mirroring listener. This method should
     * be called when the user wants to turn off the hand mirroring mode.
     */
    public void end()
    {
        Debug.Log("in end");

        controller.RemoveListener(listener);
        controller.Dispose();
    }
}

/**
 * This class is a listener that has the purpose of getting the fingure posistions
 * of a hand. These positions are read from the Leap Motion camera every frame.
 */
public class HandMirrorListner : Listener
{
    int count = 0; // count is used to limit the rate of frames read
    private System.Object thisLock2 = new System.Object();
    SerialPort stream = new SerialPort("COM5", 9600);
    
    private void SafeWriteLine(String line)
    {
        lock (thisLock2)
        {
            Console.WriteLine(line);
        }
    }

    /**
    * This method executes when the listener first connects to the camera.
    */
    public override void OnConnect(Controller controller)
    {
        SafeWriteLine("Connected");
    }

    /**
    * This method is called on every camera frame. In this method the position of each
    * finger is sent throug a serial port to the robot arm.  
    */
    public override void OnFrame(Controller controller)
    {
        Frame frame = controller.Frame();
        Hand hand = frame.Hands.Rightmost;
        FingerList fingers = hand.Fingers;
        String fingerStatus = "";

        count++;
        if (count > 30)
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
