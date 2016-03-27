using UnityEngine;
using Leap;
using System;
using System.Threading;
using System.Collections;
using System.IO.Ports;
using System.Media;

/**
 * This is where all "global variables" can be declared. 
 */
public static class MyStaticValues
{
    // Used to keep track of when the user does thier move
    public static int count = 0;
}

/**
* This class represents a rock, paper, scissers game. The game is intended to be played between 
* a human and a robotic hand. The human hand is reconized using a Leap Motion camera. The robotic 
* arm and hand are controlled by an arduino Uno and the character that is sent to it throught the 
* moveRobotHand function. Ir you wanted to use this without the robotic arm just comment out the 
* method that sends data through the serial port and it will still work fine.
*/
public class rockPaperScissors : MonoBehaviour
{
    private string gameResults;
    private int movieSelector;
    private bool doneWithThread;
    private bool resultsShown;
    private bool useMovies;
    private HandMirror handMirror;
    private Movie movie;
    private AudioSource audio;

    public Thread gameThread;  
    public ToggleButtonDataBinder playingButton; // true = game is going, false = game waiting for button press
    public ToggleButtonDataBinder handMirrorButton; // true = hand mirroring, false = no hand mirroring   
    public bool handMirrorMode;
    public bool rpsMode;
    public AudioClip toggleSound;
    public AudioClip countDownSound;
    
    /**
     * This is run once when the game first starts.
     */
    void Start()
    {
        // Set initial states of the buttons and text
        playingButton.SetCurrentData(false);
        handMirrorButton.SetCurrentData(false);
        doneWithThread = false;
        resultsShown = false;
        GetComponent<TextMesh>().text = "Press 'Start' to play";
        handMirror = new HandMirror();
        handMirrorMode = false; // If the user is in handMirrorMode
        rpsMode = false; // If the user is in rpsMode 

        // Set true to use the true short moives for results, false to display results as text 
        useMovies = true;
        movie = FindObjectOfType(typeof(Movie)) as Movie;

        // For sound clips
        audio = GetComponent<AudioSource>();

        // this dosen't work. Why????
        Console.Beep(5000, 1000);
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
            rpsMode = false; // Game is done so user is not in rps mode anymore
        }

        // Once the playing button is pressed (can't be in hand mirroring mode)
        if (playingButton.GetCurrentData() && !rpsMode && !handMirrorButton.GetCurrentData())
        {
            audio.PlayOneShot(toggleSound);
            rpsMode = true; // User is in rps mode
            resultsShown = false;
            StartCoroutine(gameCountDown()); // starts the rps games

        }

        // Runs once the game thread is done
        if (doneWithThread)
        {
            if (useMovies)
            {  //  Debug.Log("using movies");

                // Displays the results as a short clip
                GetComponent<TextMesh>().text = "";
                displayMovieResults(); // displays short movie result    
            }
            else
            {
                // Displays game results and then starts a timer for how long the results will
                // be displayed
                GetComponent<TextMesh>().text = gameResults;
                StartCoroutine(displayResults()); // displays text result
            }
            doneWithThread = false;
        }

        // If the hand mirroring button is pressed (can't be playing rps game)
        if (handMirrorButton.GetCurrentData() && !handMirrorMode && !rpsMode)
        {
            audio.PlayOneShot(toggleSound);
            handMirrorMode = true;
            Debug.Log("in hand Mirroring if");
            handMirror.start();
            Debug.Log("after start");
        }

        // If hand mirroring button is turned off
        if (!handMirrorButton.GetCurrentData() && handMirrorMode)
        {
            Debug.Log(" in end if");
            audio.PlayOneShot(toggleSound);
            handMirror.end();
            moveRobotHand("paper"); // opens robo hand back up to releive stress on the servos
            handMirrorMode = false;
        }

        // Don't allow user to press hand mirror button while in rpsMode
        if(rpsMode)
        {
            handMirrorButton.SetCurrentData(false);
        }

        // Don't allow user to press play button while in rpsMode
        if (handMirrorMode)
        {
            playingButton.SetCurrentData(false);
        }

        // Quits when the esc key is pressed
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Application.Quit();
        }
    }

    /**
     * This method starts the game. There is a three second countdown and then the 
     * the game thread is started. Once the game thread starts both the user move 
     * and the robot move are determined as well as the winner. 
     */
    public IEnumerator gameCountDown()
    {
        


        GetComponent<TextMesh>().text = "Do move in";
        yield return new WaitForSeconds(.7f); // waits 1 second
        /***********   UNCOMMENT IF ROBOT ARM IS ATTACHED   *************/
        // moveRobotHand("COUNTDOWN");
        //audio.PlayOneShot(countDownSound);
        yield return new WaitForSeconds(.3f); // waits 1 second
        GetComponent<TextMesh>().text = "3";
        SystemSounds.Beep.Play();
        yield return new WaitForSeconds(1f); // waits 1 second
        GetComponent<TextMesh>().text = "2";
        SystemSounds.Beep.Play();
        yield return new WaitForSeconds(1f); // waits 1 second
        GetComponent<TextMesh>().text = "1";
        SystemSounds.Beep.Play();
        yield return new WaitForSeconds(1f); // waits .5 second
        GetComponent<TextMesh>().text = "shoot!";
        SystemSounds.Beep.Play();

        // Start the game thread
        gameThread = new Thread(theGame);
        gameThread.Start();
        // We don't have a join here so that the leap thread will continue to run
        // This allows the hand to still be displayed during the game
        Debug.Log("Starting game thread...");
    }

    /**
     * This method runs a timer while the game results are being displayed on the UI.
     * After the timer runs out resultsShown is set to true which allows the text on 
     * the UI to be changed back to its original text ("Press 'Start' to play").
     *
     * NOTE: Use this function to display text results and displayMovieResults for 
     *       a short movie result but don't use both.
     */
    public IEnumerator displayResults()
    {
        // resultsShown = false;
        yield return new WaitForSeconds(4f); // waits 4 seconds
        resultsShown = true; // will make the update method pick up 
        playingButton.SetCurrentData(false); // Turn button off
        doneWithThread = false;
    }

    /**
     * We stay in this method until the movie results are done playing. There is a different
     * case for each movie. Depeneding on which movie is playing it will enter into a differnt
     * while loop where it will wait until the move is done playing. In each play method in the
     * movie class the plane that has a movie playing on it is moved forward by .5 so after the
     * movie finishes we need to move it back by tranlating it with a -.5. After the moive is 
     * done playing the Stop() method also needs called on it to reset/rewind it so it can be 
     * played again.
     */
    private IEnumerator FindEnd()
    {
        // Find which movie is playing and loop until it finishes
        if (movie.paperRock.isPlaying)
        {
            while (movie.paperRock.isPlaying)
                yield return 0;
            movie.transform.GetChild(0).transform.Translate(0, -.5f, 0);
            movie.paperRock.Stop();
        }
        else if (movie.paperScissors.isPlaying)
        {
            while (movie.paperScissors.isPlaying)
                yield return 0;
            movie.transform.GetChild(1).transform.Translate(0, -.5f, 0);
            movie.paperScissors.Stop();
        }
        else if (movie.rockPaper.isPlaying)
        {
            while (movie.rockPaper.isPlaying)
                yield return 0;
            movie.transform.GetChild(2).transform.Translate(0, -.5f, 0);
            movie.rockPaper.Stop();
        }
        else if (movie.rockScissors.isPlaying)
        {
            while (movie.rockScissors.isPlaying)
                yield return 0;
            movie.transform.GetChild(3).transform.Translate(0, -.5f, 0);
            movie.rockScissors.Stop();
        }
        else if (movie.scissorsRock.isPlaying)
        {
            while (movie.scissorsRock.isPlaying)
                yield return 0;
            movie.transform.GetChild(4).transform.Translate(0, -.5f, 0);
            movie.scissorsRock.Stop();
        }
        else if (movie.scissorsPaper.isPlaying)
        {
            while (movie.scissorsPaper.isPlaying)
                yield return 0;
            movie.transform.GetChild(5).transform.Translate(0, -.5f, 0);
            movie.scissorsPaper.Stop();
        }
        else if (movie.tie.isPlaying)
        {
            while (movie.tie.isPlaying)
                yield return 0;
            movie.transform.GetChild(6).transform.Translate(0, -.5f, 0);
            movie.tie.Stop();
        }

        // Move moive screen back out of the game scene once it is done playing
        movie.moveMovieOut();
        playingButton.SetCurrentData(false); // Turn button off
        doneWithThread = false; 
        resultsShown = true;
        
        yield break;
    }

    /**
     * This method displays the short movie results. There is a different movie for each case.
     * The movie that is played depends on the variable movieSelector which is assigned during
     * the game logic. Once the movie starts playing the FindEnd() method is called where we 
     * loop until the movie is done playing.
     *
     * NOTE: Use this function to display text results and displayMovieResults for 
     *       a short movie result but don't use both.
     */
    public void displayMovieResults()
    {
        // Moves the movie screen into the game scene
        movie.moveMovieIn();

        // Selects which movie should play based off of movieSelector
        // NOTE: moive methods put the humans move first followed by the robots
        // e.g. 'playPaperRock()' means play the move result for if the human did 
        //      paper and the robot did rock
        switch (movieSelector)
        {
            case 0: // Tie
                movie.playTie();
                break;
            case 1: // Paper beats rock
                movie.playPaperRock();
                break;
            case 2: // Scissors beaten by rock
                movie.playScissorsRock();
                break;
            case 3: // Scissors beats paper
                movie.playScissorsPaper();
                break;
            case 4: // Rock beaten by paper
                movie.playRockPaper();
                break;
            case 5: // Rock beats scissors
                movie.playRockScissors();
                break;
            case 6: // Paper beaten by scissors
                movie.playPaperScissors();
                break;
            default:
                Debug.Log("Default case");
                break;
        }

        // Starts looping until the end of the movie
        StartCoroutine(FindEnd());
    }

    /**
     * This method is where the standard rock, paper, scissors logic takes place. Both the 
     * user move and the robot move are compared and a winner is determined. The results are 
     * then set to the gameResults string. We also set the movieSelector here in case the 
     * movie results are choosen to be displayed instead of the text results.
     */
    void theGame()
    {
        //Debug.Log("in theGame function");
        String robotMove = getRobotMove();
        String userMove = getUserMove(robotMove);
        String winner = "";

        // If there is a tie
        if (userMove.Equals(robotMove))
        {
            winner = "tie";
            movieSelector = 0;
        }
        // If robot chooses 'rock'
        else if (robotMove.Equals("rock"))
        {
            if (userMove.Equals("paper"))
            {
                winner = "human";
                movieSelector = 1;
            }
            else if (userMove.Equals("scissors"))
            {
                winner = "robot";
                movieSelector = 2;
            }
        }
        // If robot chooses 'paper'
        else if (robotMove.Equals("paper"))
        {
            if (userMove.Equals("scissors"))
            {
                winner = "human";
                movieSelector = 3;
            }
            else if (userMove.Equals("rock"))
            {
                winner = "robot";
                movieSelector = 4;
            }
        }
        // If robot chooses 'scissors'
        else if (robotMove.Equals("scissors"))
        {
            if (userMove.Equals("rock"))
            {
                winner = "human";
                movieSelector = 5;
            }
            else if (userMove.Equals("paper"))
            {
                winner = "robot";
                movieSelector = 6;
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

        // Wait 2 seconds so user has time to see the robots move before showing reults
        Thread.Sleep(2000);

        // Set to true so we know the game is over
        doneWithThread = true;
    }

    /**
     * This method gets the users move. A listener is created for the Leap Motion camera. 
     * The users move is obtained from the listener and is returned as the move for this round.
     */
    public String getUserMove(String robotMove)
    {
        HandListener listener = new HandListener();
        Controller controller = new Controller();
        controller.AddListener(listener);

        /****      UNCOMMENT WITH ROBOT ARM    ****/
        //moveRobotHand(robotMove); // Uncoment this line if the robot arm is attached
        //Thread.Sleep(100); // Give time for the listener to grab the users move

        /****      UNCOMMENT WITH OUT ROBOT ARM    ****/
         Thread.Sleep(1000); // Uncomment this line if the robot arm is  NOT attached

        string move = listener.move;

        controller.RemoveListener(listener);
        controller.Dispose();

        return move;
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

        // Return move
        return move;
    }

    /**
     * This method controlls the movement of the robot hand. The move which is
     * to be made by the hand is passed in. 
     */
    void moveRobotHand(String move)
    {

        SerialPort stream = new SerialPort("COM4", 9600);  // change to COM4 for little comp and COM5 for mine
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
        stream.Close();
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
    //bool goingDown = false;
    //bool goingUp = false;

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
     * This method is called each time a frame is captured. This is where
     * we can determine what move the human is doing
     */
    public override void OnFrame(Controller controller)
    {
        Frame frame = controller.Frame();
        Hand hand = frame.Hands.Rightmost;

        // NOTE: I decided it would be better to have a count down on the screen as opposed 
        //       to waiting for the user to move their fist up and down 4 times to grab the 
        //       users move. The count down seems to be easier for users to figure out what 
        //       they are supposed to be doing but I will leave the code for tracking the 
        //       users up and down movement here in case anyone is interested in it. 

        // The rate of change of the palm position in millimeters/second.
        //Vector handSpeed = hand.PalmVelocity;

        // The speed the hand is moving in the y (up and down) direction
        // If y is negative, the hand is moving down
        // If y is positive, the hand is moving up
        //float y = handSpeed.y;

        // Console.WriteLine("The hand speed is: " + handSpeed);
        // Console.WriteLine("y: " + y);

        /****************** DETERMINES WHEN TO GET THE USERS MOVE ********************/
        // Keeps track of how many times the user moves their hand up and down 
        // The users move should be taken on the fourth downward hand movement (rock, paper, scissors, shoot)
        /* if (y < -10 || goingDown) // moving down 
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
        }*/



        /************* DETERMINES THE USERS MOVE BASED OFF OF HAND SHAPE *************/
        // Used to check for "rock"
        float strength = hand.GrabStrength;

        // USed to check for "scissors"
        FingerList fingers = hand.Fingers.Extended();

        // Used to check for "paper"
        // float pitch = hand.Direction.Pitch;
        // float yaw = hand.Direction.Yaw;
        // float roll = hand.PalmNormal.Roll;

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