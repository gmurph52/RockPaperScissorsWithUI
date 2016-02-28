using UnityEngine;
using Leap;
using System;
using System.Threading;
using System.Collections;

using System.IO.Ports;

/**
 * This is where all "global variables" can be declared. 
 */
public static class MyStaticValues
{
    public static int count = 0;
    public static bool doneWithThread = false;
}

/**
* This class represents a rock, paper, scissers game. The game is intended to be played between 
* a human and a robotic hand. The human hand is reconized using a Leap Motion camera. The robotic 
* arm and hand are controlled by an arduino Uno and the character that is sent to it throught the 
* moveRobotHand function.
*/
public class rockPaperScissors : MonoBehaviour
{
    public Thread gameThread;
    public Thread resultsThread;
    private string gameResults;
    public bool resultsShown = false;
    public ToggleButtonDataBinder playingButton; // true = game is going, false = game waiting for button press
    public ToggleButtonDataBinder handMirrorButton; // true = hand mirroring, false = no hand mirroring
    SerialPort serialPort;
    HandMirror handMirror;
    bool handMirrorMode;


    /**
     * This is run once when the game first starts.
     */
    void Start()
    {
        // Set initial states of the buttons and text
        playingButton.SetCurrentData(false);
        handMirrorButton.SetCurrentData(false);
        GetComponent<TextMesh>().text = "Press 'Start' to play";
        handMirror = new HandMirror();
        handMirrorMode = false; // If the user is in handMirrorMode
    }

    /**
     * This method is call on every frame of the game. Anything that changes or is updated
     * after the initial setup will occure in this method.
     */
    void Update()
    {
        // Once the game results have been shown and it's time for a new game 
        if (resultsShown == true)
        {
            GetComponent<TextMesh>().text = "Press 'Start' to play";
        }

        // Once the playing button is pressed (can't be in hand mirroring mode)
        if (playingButton.GetCurrentData() && !handMirrorButton.GetCurrentData())
        {
            resultsShown = false;
            GetComponent<TextMesh>().text = "game started...";

            // Start the game thread
            gameThread = new Thread(theGame);
            gameThread.Start();
            // We don't have a join here so that the leap thread will continue to run
            // This allows the hand to still be displayed during the game
            Debug.Log("Starting game thread...");

            // Set false so only one game thread is started
            playingButton.SetCurrentData(false);
        }

        // Runs once the game thread is done
        if (MyStaticValues.doneWithThread)
        {
            // Displays game results and then starts a timer for how long the results will
            // be displayed
            GetComponent<TextMesh>().text = gameResults;
            StartCoroutine(displayResults());
            MyStaticValues.doneWithThread = false;
        }

        // If the hand mirroring button is pressed (can't be playing rps game)
        if (handMirrorButton.GetCurrentData() && !handMirrorMode && !playingButton.GetCurrentData())
        {
            handMirrorMode = true;
            Debug.Log("in hand Mirroring if");
            handMirror.start();
            Debug.Log("after start");

        }

        // If hand mirroring button is NOT pressed
        if (!handMirrorButton.GetCurrentData() && handMirrorMode)
        {
            Debug.Log(" in end if");

            handMirror.end();
            handMirrorMode = false; 
        }
    }

    /**
     * This method runs a timer while the game results are being displayed on the UI.
     * After the timer runs out resultsShown is set to true which allows the text on 
     * the UI to be changed back to its original text ("Press 'Start' to play").
     */
    public IEnumerator displayResults()
    {
        resultsShown = false;
        yield return new WaitForSeconds(4f); // waits 4 seconds
        resultsShown = true; // will make the update method pick up 
    }

    /**
     * This method is where the standard rock, paper, scissors logic takes place. Both the 
     * user move and the robot move are compared and a winner is determined. The results are 
     * then set to the gameResults string.
     */
    void theGame()
    {
        Debug.Log("in theGame function");
        String robotMove = getRobotMove();
        String userMove = getUserMove(robotMove);
        String winner = "";

        // If there is a tie
        if (userMove.Equals(robotMove))
        {
            winner = "tie";
        }
        // If robot chooses 'rock'
        else if (robotMove.Equals("rock"))
        {
            if (userMove.Equals("paper"))
            {
                winner = "human";
            }
            else if (userMove.Equals("scissors"))
            {
                winner = "robot";
            }
        }
        // If robot chooses 'paper'
        else if (robotMove.Equals("paper"))
        {
            if (userMove.Equals("scissors"))
            {
                winner = "human";
            }
            else if (userMove.Equals("rock"))
            {
                winner = "robot";
            }
        }
        // If robot chooses 'scissors'
        else if (robotMove.Equals("scissors"))
        {
            if (userMove.Equals("rock"))
            {
                winner = "human";
            }
            else if (userMove.Equals("paper"))
            {
                winner = "robot";
            }
        }

        // Sets the gameResults
        gameResults = "The human did a " + userMove + "\nThe robot did a " + robotMove + "\n\n";
        if (winner.Equals("tie"))
        {
            gameResults += "It is a tie!";
        }
        else
        {
            gameResults += "The " + winner + " wins!!!!\n";
        }

        // Set to true so we know the game is over
        MyStaticValues.doneWithThread = true;
    }

    /**
     * This method gets the users move. A listener is created for the Leap Motion camera. 
     * The users move is obtained from the listener and is returned as the move for this round.
     */
    public String getUserMove(String robotMove)
    {
        bool ready = false;
        HandListener listener = new HandListener();
        Controller controller = new Controller();
        controller.AddListener(listener);

        // Keep this process running until user makes thier move
        while (!ready)
        {
            // Wait until ready to get move
            if (MyStaticValues.count > 4)
            {

                /**** ONLY ONE OF THESE LINES SHOULD BE UNCOMMENTED. ****/
                /****      EACH HAS ABOUT THE SAME TIME DELAY        ****/
                // moveRobotHand(robotMove); // Uncoment this line if the robot arm is attached
                Thread.Sleep(1500); // Uncomment this line if the robot arm is  NOT attached

                ready = true;
                MyStaticValues.count = 0;
            }
        }

        controller.RemoveListener(listener);
        controller.Dispose();

        return listener.move;
    }

    /**
     * This method is where the robot's move is determined. A random number from 1-3 is
     * generated and used in a switch statement to choose either 'rock', 'paper', or 
     * 'scissors'.
     */
    public String getRobotMove()
    {
        String move;

        // Get random number 1-3    
        System.Random rnd = new System.Random();
        int random = rnd.Next(1, 4); // creates a number between 1 and 3
        int caseSwitch = random;

        // Set move based off of random number
        switch (caseSwitch)
        {
            case 1:
                move = "rock";
                break;
            case 2:
                move = "paper";
                break;
            case 3:
                move = "scissors";
                break;
            default:
                move = "rock";
                break;
        }

        // Send move to robotic hand  // THIS WAS MOVED TO GET USER MOVE FOR TIMING 
        //moveRobotHand(move);

        // Return move
        return move;
    }

    /**
     * This method controlls the movement of the robot hand. The move which is
     * to be made by the hand is passed in. 
     */
    void moveRobotHand(String move)
    {

        SerialPort stream = new SerialPort("COM5", 9600);
        stream.ReadTimeout = 50;
        stream.Open();
        Debug.Log("opened");
        try
        {
            Debug.Log("in try");
            stream.WriteLine(move);
            stream.BaseStream.Flush();
        }
        catch (System.IO.IOException exception)
        {
            Debug.Log("Couldn't open port!");
            Debug.Log(exception);
        }
    }
}

/**
 * This listener captures frames from the Leap Motion camera. Once a listener
 * is created it continually gets frames from the camera until the listener is
 * removed.
 */
class HandListener : Listener
{
    public String move = "";
    bool goingDown = false;
    bool goingUp = false;

    private System.Object thisLock = new System.Object();

    private void SafeWriteLine(String line)
    {
        lock (thisLock)
        {
            Console.WriteLine(line);
        }
    }

    /**
     * This method executes when the listener first connects to the camera.
     */
    public override void OnConnect(Controller controller)
    {
        // SafeWriteLine("Connected");
    }

    /**
     * This method is called each time a frame is captured. 
     */
    public override void OnFrame(Controller controller)
    {
        Frame frame = controller.Frame();
        Hand hand = frame.Hands.Rightmost;

        //The rate of change of the palm position in millimeters/second.
        Vector handSpeed = hand.PalmVelocity;

        // The speed the hand is moving in the y (up and down) direction
        // If y is negative, the hand is moving down
        // If y is positive, the hand is moving up
        float y = handSpeed.y;

        // Console.WriteLine("The hand speed is: " + handSpeed);
        // Console.WriteLine("y: " + y);

        /****************** DETERMINES WHEN TO GET THE USERS MOVE ********************/
        // Keeps track of how many times the user moves their hand up and down 
        // The users move should be taken on the fourth downward hand movement (rock, paper, scissors, shoot)
        if (y < -10 || goingDown) // moving down 
        {
            goingDown = true;

            if (y > 10 || goingUp)
            {
                goingUp = true;
                if (y < -10 && goingUp)
                {
                    MyStaticValues.count += 1;
                    // Console.WriteLine("count: " + MyStaticValues.count);
                    goingUp = false;
                    goingDown = false;
                }
            }
        }

        /************* DETERMINES THE USERS MOVE BASED OFF OF HAND SHAPE *************/

        // Used to check for "rock"
        float strength = hand.GrabStrength;

        // USed to check for "scissors"
        FingerList fingers = hand.Fingers.Extended();

        // Used to check for "paper"
        float pitch = hand.Direction.Pitch;
        float yaw = hand.Direction.Yaw;
        float roll = hand.PalmNormal.Roll;

        // Check for "rock"
        if (strength > .9) //  [0..1]. 0 open 1 fist
        {
            move = "rock";
        }
        // Check for scissors
        else if (fingers.Count > 0 && fingers.Count < 4)
        {
            move = "scissors";
        }
        // Check for "paper"
        else //if (pitch < .5 && yaw < .5 && roll < .5 /*&& !move.Equals("rock")*/)
        {
            move = "paper";
        }
        /* else
         {
             move = "invalid";
         }*/

        // For testing/debuging
        /* Console.WriteLine("Strength is: " + strength);
           Console.WriteLine("pitch  = " + pitch);
           Console.WriteLine("yaw  = " + yaw);
           Console.WriteLine("roll  = " + roll);
           Console.WriteLine("fingers  = " + fingers);*/

        //  Console.WriteLine("\nmove = " + move + "\n");
    }
}



